using Discord;

using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Administration;

/// <summary>
/// Administration service
/// </summary>
public class AdministrationService : LocatedServiceBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public AdministrationService(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Rename user
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="name">Name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task RenameMember(IGuildUser user, string name)
    {
        return user.ModifyAsync(obj => obj.Nickname = name);
    }

    /// <summary>
    /// Rename user
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="name">Name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task RenameRole(IRole user, string name)
    {
        return user.ModifyAsync(obj => obj.Name = name);
    }

    #endregion // Methods
}