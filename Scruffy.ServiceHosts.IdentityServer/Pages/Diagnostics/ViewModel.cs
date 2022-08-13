using System.Text;
using System.Text.Json;

using IdentityModel;

using Microsoft.AspNetCore.Authentication;

namespace Scruffy.ServiceHosts.IdentityServer.Pages.Diagnostics
{
    /// <summary>
    /// Diagnostics view model
    /// </summary>
    public class ViewModel
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="result">Authentication result</param>
        public ViewModel(AuthenticateResult result)
        {
            AuthenticateResult = result;

            if (result.Properties?.Items.ContainsKey("client_list") == true)
            {
                var encoded = result.Properties.Items["client_list"];
                var bytes = Base64Url.Decode(encoded);
                var value = Encoding.UTF8.GetString(bytes);

                Clients = JsonSerializer.Deserialize<string[]>(value);
            }
        }

        #endregion // Constructor

        #region Properties

        /// <summary>
        /// Authentication results
        /// </summary>
        public AuthenticateResult AuthenticateResult { get; }

        /// <summary>
        /// Clients
        /// </summary>
        public IEnumerable<string> Clients { get; } = new List<string>();

        #endregion // Properties
    }
}