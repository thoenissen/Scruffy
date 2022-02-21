using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.CoreData;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;
using Scruffy.Services.Discord.Interfaces;

namespace Scruffy.Services.Developer;

/// <summary>
/// Developer service
/// </summary>
public class DeveloperService : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// User management service
    /// </summary>
    private readonly UserManagementService _userManagementService;

    /// <summary>
    /// Repository factory
    /// </summary>
    private readonly RepositoryFactory _repositoryFactory;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="userManagementService">User management service</param>
    /// <param name="repositoryFactory">Repository factory</param>
    public DeveloperService(LocalizationService localizationService,
                            UserManagementService userManagementService,
                            RepositoryFactory repositoryFactory)
        : base(localizationService)
    {
        _userManagementService = userManagementService;
        _repositoryFactory = repositoryFactory;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Set account name
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="accountName">Account name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SetAccount(IContextContainer context, string accountName)
    {
        var user = await _userManagementService.GetUserByDiscordAccountId(context.User.Id)
                                               .ConfigureAwait(false);
        if (user != null)
        {
            if (_repositoryFactory.GetRepository<UserRepository>()
                                  .Refresh(obj => obj.Id == user.Id,
                                           obj => obj.GitHubAccount = accountName))
            {
                await context.ReplyAsync(LocalizationGroup.GetText("AccountRefresh", "The account has been updated."))
                             .ConfigureAwait(false);
            }
            else
            {
                throw _repositoryFactory.LastError;
            }
        }
    }

    #endregion // Methods
}