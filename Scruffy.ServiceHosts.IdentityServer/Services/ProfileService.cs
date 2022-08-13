using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;

using Microsoft.AspNetCore.Identity;

using Scruffy.Data.Entity.Tables.CoreData;

using Serilog;

namespace Scruffy.ServiceHosts.IdentityServer.Services
{
    /// <summary>
    /// Profile service
    /// </summary>
    public class ProfileService : IProfileService
    {
        #region Fields

        /// <summary>
        /// Claims factory
        /// </summary>
        private readonly IUserClaimsPrincipalFactory<UserEntity> _claimsFactory;

        /// <summary>
        /// User manger
        /// </summary>
        private readonly UserManager<UserEntity> _userManager;

        /// <summary>
        /// Logging
        /// </summary>
        private readonly ILogger<ProfileService> _logger;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userManager">User manager</param>
        /// <param name="claimsFactory">Claims factory</param>
        /// <param name="logger">Logger</param>
        public ProfileService(UserManager<UserEntity> userManager,
                              IUserClaimsPrincipalFactory<UserEntity> claimsFactory,
                              ILogger<ProfileService> logger)
        {
            _userManager = userManager;
            _claimsFactory = claimsFactory;
            _logger = logger;
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Get profile data
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject?.GetSubjectId();
            if (sub == null)
            {
                throw new Exception("No sub claim present");
            }

            var user = await _userManager.FindByIdAsync(sub)
                                         .ConfigureAwait(false);
            if (user == null)
            {
                _logger?.LogWarning("No user found matching subject Id: {0}", sub);
            }
            else
            {
                var principal = await _claimsFactory.CreateAsync(user)
                                                    .ConfigureAwait(false);
                if (principal == null)
                {
                    throw new Exception("ClaimsFactory failed to create a principal");
                }

                context.IssuedClaims.AddRange(principal.Claims.Where(obj => obj.Type == "role"));
            }
        }

        /// <summary>
        /// Ist the user active?
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject?.GetSubjectId();
            if (sub == null)
            {
                throw new Exception("No subject Id claim present");
            }

            var user = await _userManager.FindByIdAsync(sub)
                                         .ConfigureAwait(false);
            if (user == null)
            {
                _logger?.LogWarning("No user found matching subject Id: {0}", sub);
            }

            context.IsActive = user != null;
        }

        #endregion // Methods
    }
}