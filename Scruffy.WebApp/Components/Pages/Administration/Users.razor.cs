using System;
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
    private IQueryable<UserDTO> _users;

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
                                                      Name = obj.UserName
                                                  })
                                   .OrderBy(obj => obj.Name);
    }

    #endregion // ComponentsBase

    #region IDisposable

    /// <inheritdoc/>
    public void Dispose()
    {
        _repositoryFactory?.Dispose();
    }

    #endregion // IDisposable
}