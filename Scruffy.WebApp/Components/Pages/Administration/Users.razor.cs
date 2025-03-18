using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Authorization;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.CoreData;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.WebApp.DTOs.Administration;

namespace Scruffy.WebApp.Components.Pages.Administration;

/// <summary>
/// Users overview
/// </summary>
[Authorize(Roles = "Administrator")]
public sealed partial class Users : IDisposable
{
    #region Fields

    /// <summary>
    /// Repository factory
    /// </summary>
    private readonly RepositoryFactory _repositoryFactory = RepositoryFactory.CreateInstance();

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

        var serverMembers = _repositoryFactory.GetRepository<DiscordServerMemberRepository>()
                                              .GetQuery()
                                              .Where(e => e.ServerId == WebAppConfiguration.DiscordServerId);

        _users = _repositoryFactory.GetRepository<UserRepository>()
                                   .GetQuery()
                                   .Where(user => user.DiscordAccounts.Any(account => serverMembers.Any(member => member.AccountId == account.Id)))
                                   .Select(obj => new UserDTO
                                                  {
                                                      Id = obj.Id,
                                                      Name = serverMembers.Where(member => member.AccountId == obj.DiscordAccounts.Select(account => account.Id).FirstOrDefault())
                                                                          .Select(member => member.Name)
                                                                          .FirstOrDefault(),
                                                      GuildWarsAccountName = obj.GuildWarsAccounts.Select(account => account.Name).FirstOrDefault()
                                                  })
                                   .OrderBy(obj => obj.Name)
                                   .ToList();

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

        _filteredUsers = _users.Where(obj => obj.Name.Contains(_filter, StringComparison.OrdinalIgnoreCase)).AsQueryable();
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