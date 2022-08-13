using System.ComponentModel.DataAnnotations;

namespace Scruffy.ServiceHosts.IdentityServer.Pages.Account.Login
{
    /// <summary>
    /// Input model
    /// </summary>
    public class InputModel
    {
        /// <summary>
        /// User name
        /// </summary>
        [Required]
        public string UserName { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        [Required]
        public string Password { get; set; }

        /// <summary>
        /// Remember login
        /// </summary>
        public bool RememberLogin { get; set; }

        /// <summary>
        /// Return url
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// Button
        /// </summary>
        public string Button { get; set; }
    }
}