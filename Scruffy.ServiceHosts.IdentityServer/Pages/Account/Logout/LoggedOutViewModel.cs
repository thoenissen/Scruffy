namespace Scruffy.ServiceHosts.IdentityServer.Pages.Account.Logout
{
    /// <summary>
    /// Logged out view model
    /// </summary>
    public class LoggedOutViewModel
    {
        /// <summary>
        /// Post logout redirect uri
        /// </summary>
        public string PostLogoutRedirectUri { get; set; }

        /// <summary>
        /// Client name
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// Sign out IFrame url
        /// </summary>
        public string SignOutIframeUrl { get; set; }
    }
}