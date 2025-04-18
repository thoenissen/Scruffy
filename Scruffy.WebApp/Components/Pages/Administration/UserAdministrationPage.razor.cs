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
    /// Users
    /// </summary>
    private List<UserDTO> _users;

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
                                                                      account.UserId,
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
                                                                                          || account.ApiKey == "Free-To-Play")
                                                                  })
                                   .ToList();

        var guilds = _repositoryFactory.GetRepository<GuildRepository>()
                                       .GetQuery();

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
                                                           && member.GuildId == guilds.Where(guild => guild.DiscordServerId == WebAppConfiguration.DiscordServerId)
                                                                                      .Select(guild => guild.Id)
                                                                                      .FirstOrDefault()
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
                                                                                                                              || guildWarsAccount.ApiKey == "Free-To-Play"))
                                                            })
                                          .ToList());

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

    #endregion // Methods

    #region IDisposable

    /// <inheritdoc/>
    public void Dispose()
    {
        _repositoryFactory?.Dispose();
    }

    #endregion // IDisposable
}