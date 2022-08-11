using System.Collections.Concurrent;
using System.Security.Claims;

using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;

using IdentityModel;

namespace Scruffy.ServiceHosts.IdentityServer.Services
{
    /// <summary>
    /// Profile service
    /// </summary>
    public class ProfileService : IProfileService
    {
        #region Fields

        /// <summary>
        /// Developers
        /// </summary>
        private static readonly ConcurrentDictionary<string, byte> _developers;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        static ProfileService()
        {
            var developers = Environment.GetEnvironmentVariable("SCRUFFY_DEVELOPER_USER_IDS") ?? string.Empty;

            _developers = new ConcurrentDictionary<string, byte>(developers.Split(";").ToDictionary(obj => obj, obj => (byte)0));
        }

        #endregion // Constructor

        #region IProfileService

        /// <summary>
        /// This method is called whenever claims about the user are requested (e.g. during token creation or via the userinfo endpoint)
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var id = context.Subject.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (id != null
             && _developers.ContainsKey(id))
            {
                context.IssuedClaims = new List<Claim>
                                       {
                                           new(JwtClaimTypes.Role, "Developer")
                                       };
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// This method gets called whenever identity server needs to determine if the user is valid or active (e.g. if the user's account has been deactivated since they logged in).
        /// (e.g. during token issuance or validation).
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        public Task IsActiveAsync(IsActiveContext context) => Task.CompletedTask;

        #endregion // IProfileService
    }
}