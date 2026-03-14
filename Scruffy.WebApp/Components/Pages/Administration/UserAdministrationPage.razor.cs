using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.QuickGrid;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Data.Entity.Repositories.GuildWars2.Guild;
using Scruffy.Data.Enumerations.GuildWars2;
using Scruffy.WebApp.DTOs.Administration;

namespace Scruffy.WebApp.Components.Pages.Administration;

/// <summary>
/// Users overview
/// </summary>
[Authorize(Roles = "Administrator")]
public sealed partial class UserAdministrationPage : IDisposable
{
    #region Fields

    /// <summary>
    /// Repository factory
    /// </summary>
    private readonly RepositoryFactory _repositoryFactory = RepositoryFactory.CreateInstance();

    /// <summary>
    /// Sort by <see cref="UserDTO.IsGuildMember"/>
    /// </summary>
    private readonly GridSort<UserDTO> _gridSortIsGuildMember = GridSort<UserDTO>.ByAscending(e => e.IsGuildMember);

    /// <summary>
    /// Sort by <see cref="UserDTO.IsApiKeyValid"/>
    /// </summary>
    private readonly GridSort<UserDTO> _gridSortIsApiKeyValid = GridSort<UserDTO>.ByAscending(e => e.IsApiKeyValid);

    /// <summary>
    /// Sort by <see cref="UserDTO.IsFixedRank"/>
    /// </summary>
    private readonly GridSort<UserDTO> _gridSortIsFixedRank = GridSort<UserDTO>.ByAscending(e => e.IsFixedRank);

    /// <summary>
    /// Sort by <see cref="UserDTO.IsInactive"/>
    /// </summary>
    private readonly GridSort<UserDTO> _gridSortIsInactive = GridSort<UserDTO>.ByAscending(e => e.IsInactive);

    /// <summary>
    /// Users
    /// </summary>
    private List<UserDTO> _users;

    /// <summary>
    /// Currently selected user for the overlay
    /// </summary>
    private UserDTO _selectedUser;

    /// <summary>
    /// Filtered users
    /// </summary>
    private IQueryable<UserDTO> _filteredUsers;

    /// <summary>
    /// Filter
    /// </summary>
    private string _filter;

    #endregion // Fields

    #region ComponentsBase

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();

        var guildId = _repositoryFactory.GetRepository<GuildRepository>()
                                        .GetQuery()
                                        .Where(obj => obj.DiscordServerId == WebAppConfiguration.DiscordServerId)
                                        .Select(obj => obj.Id)
                                        .FirstOrDefault();

        _users = _repositoryFactory.GetRepository<DiscordServerMemberRepository>()
                                   .GetQuery()
                                   .Where(e => e.ServerId == WebAppConfiguration.DiscordServerId)
                                   .GroupJoin(_repositoryFactory.GetRepository<DiscordAccountRepository>()
                                                                .GetQuery(),
                                               member => member.AccountId,
                                               account => account.Id,
                                               (member, accounts) => new
                                                                     {
                                                                         Member = member,
                                                                         Accounts = accounts
                                                                     })
                                   .SelectMany(join => join.Accounts.DefaultIfEmpty(),
                                               (join, account) => new
                                                                   {
                                                                       join.Member.Name,
                                                                       UserId = account != null
                                                                                    ? (long?)account.UserId
                                                                                    : null,
                                                                   })
                                   .GroupJoin(_repositoryFactory.GetRepository<GuildWarsAccountRepository>()
                                                                .GetQuery(),
                                               member => member.UserId,
                                               account => account.UserId,
                                               (member, accounts) => new
                                                                     {
                                                                         Member = member,
                                                                         Accounts = accounts
                                                                     })
                                   .SelectMany(join => join.Accounts.DefaultIfEmpty(),
                                               (join, account) => new UserDTO
                                                                  {
                                                                      DiscordAccountName = join.Member.Name,
                                                                      GuildWarsAccountName = account != null
                                                                                                 ? account.Name
                                                                                                 : string.Empty,
                                                                      IsGuildMember = account != null,
                                                                      IsApiKeyValid = account != null
                                                                                      && (account.Permissions.HasFlag(GuildWars2ApiPermission.RequiredPermissions)
                                                                                          || account.ApiKey == "Free-To-Play"),
                                                                      UserId = join.Member.UserId,
                                                                      GuildId = guildId,
                                                                      GuildWarsAccountApiKey = account != null
                                                                                                   ? account.ApiKey
                                                                                                   : null
                                                                  })
                                   .ToList();

        var guildWarsAccounts = _repositoryFactory.GetRepository<GuildWarsAccountRepository>()
                                                  .GetQuery();

        var discordAccounts = _repositoryFactory.GetRepository<DiscordAccountRepository>()
                                                .GetQuery();

        var discordMembers = _repositoryFactory.GetRepository<DiscordServerMemberRepository>()
                                               .GetQuery()
                                               .Where(member => member.ServerId == WebAppConfiguration.DiscordServerId);

        _users.AddRange(_repositoryFactory.GetRepository<GuildWarsGuildHistoricMemberRepository>()
                                          .GetQuery()
                                          .Where(member => member.Date == DateTime.Today
                                                           && member.GuildId == guildId
                                                           && guildWarsAccounts.Any(guildWarsAccount => guildWarsAccount.Name == member.Name
                                                                                                        && discordAccounts.Any(discordAccount => discordAccount.UserId == guildWarsAccount.UserId
                                                                                                                                                 && discordMembers.Any(discordMember => discordMember.AccountId == discordAccount.Id))) == false)
                                          .Select(member => new UserDTO
                                                            {
                                                                DiscordAccountName = string.Empty,
                                                                GuildWarsAccountName = member.Name,
                                                                IsGuildMember = true,
                                                                IsApiKeyValid = guildWarsAccounts.Any(guildWarsAccount => guildWarsAccount.Name == member.Name
                                                                                                                          && (guildWarsAccount.Permissions.HasFlag(GuildWars2ApiPermission.RequiredPermissions)
                                                                                                                              || guildWarsAccount.ApiKey == "Free-To-Play")),
                                                                UserId = guildWarsAccounts.Where(guildWarsAccount => guildWarsAccount.Name == member.Name)
                                                                                          .Select(guildWarsAccount => (long?)guildWarsAccount.UserId)
                                                                                          .FirstOrDefault(),
                                                                GuildId = guildId,
                                                                GuildWarsAccountApiKey = guildWarsAccounts.Where(guildWarsAccount => guildWarsAccount.Name == member.Name)
                                                                                                          .Select(guildWarsAccount => guildWarsAccount.ApiKey)
                                                                                                          .FirstOrDefault()
                                                            })
                                          .ToList());

        var configurations = _repositoryFactory.GetRepository<GuildUserConfigurationRepository>()
                                               .GetQuery()
                                               .Where(obj => obj.GuildId == guildId)
                                               .ToDictionary(obj => obj.UserId);

        foreach (var user in _users)
        {
            if (user.UserId.HasValue
                && configurations.TryGetValue(user.UserId.Value, out var config))
            {
                user.IsFixedRank = config.IsFixedRank;
                user.IsInactive = config.IsInactive;
            }
        }

        OnFilterChanged();
    }

    #endregion // ComponentsBase

    #region Methods

    /// <summary>
    /// Filter changed
    /// </summary>
    private void OnFilterChanged()
    {
        _filter ??= string.Empty;

        _filteredUsers = _users.Where(obj => obj.DiscordAccountName?.Contains(_filter, StringComparison.OrdinalIgnoreCase) == true
                                             || obj.GuildWarsAccountName?.Contains(_filter, StringComparison.OrdinalIgnoreCase) == true)
                               .AsQueryable();
    }

    /// <summary>
    /// Opens the configuration overlay for the given user
    /// </summary>
    /// <param name="user">User to configure</param>
    private void OnUserSelected(UserDTO user)
    {
        if (user.UserId.HasValue)
        {
            _selectedUser = user;
        }
    }

    /// <summary>
    /// Closes the configuration overlay
    /// </summary>
    private void OnCloseOverlay()
    {
        _selectedUser = null;
    }

    /// <summary>
    /// Synchronizes the configuration of all entries with the same user id
    /// </summary>
    private void OnConfigurationChanged()
    {
        if (_selectedUser?.UserId == null)
        {
            return;
        }

        foreach (var user in _users)
        {
            if (user != _selectedUser
                && user.UserId == _selectedUser.UserId)
            {
                user.IsFixedRank = _selectedUser.IsFixedRank;
                user.IsInactive = _selectedUser.IsInactive;
            }
        }
    }

    #endregion // Methods

    #region IDisposable

    /// <inheritdoc/>
    public void Dispose()
    {
        _repositoryFactory?.Dispose();
    }

    #endregion // IDisposable
}