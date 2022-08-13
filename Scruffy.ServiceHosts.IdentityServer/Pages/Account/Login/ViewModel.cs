namespace Scruffy.ServiceHosts.IdentityServer.Pages.Account.Login
{
    /// <summary>
    /// Login view model
    /// </summary>
    public class ViewModel
    {
        /// <summary>
        /// External login providers
        /// </summary>
        public IEnumerable<ExternalProvider> ExternalProviders { get; set; } = Enumerable.Empty<ExternalProvider>();

        /// <summary>
        /// External provider
        /// </summary>
        public class ExternalProvider
        {
            /// <summary>
            /// Display name
            /// </summary>
            public string DisplayName { get; set; }

            /// <summary>
            /// Authentication scheme
            /// </summary>
            public string AuthenticationScheme { get; set; }
        }
    }
}