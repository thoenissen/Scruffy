using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Scruffy.Data.Json.GuildWars2.Account;
using Scruffy.Data.Json.GuildWars2.Guild;

namespace Scruffy.Services.WebApi
{
    /// <summary>
    /// Accessing the Guild Wars 2 WEB API
    /// </summary>
    public sealed class GuidWars2ApiConnector : IAsyncDisposable, IDisposable
    {
        #region Fields

        /// <summary>
        /// Api key
        /// </summary>
        private string _apiKey;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="apiKey">Api Key</param>
        public GuidWars2ApiConnector(string apiKey)
        {
            _apiKey = apiKey;
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Request the account information
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<AccountInformation> GetAccountInformationAsync()
        {
            using (var response = await CreateRequest("https://api.guildwars2.com/v2/account?v=latest").GetResponseAsync()
                                                                                                       .ConfigureAwait(false))
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var jsonResult = await reader.ReadToEndAsync().ConfigureAwait(false);

                    return JsonConvert.DeserializeObject<AccountInformation>(jsonResult);
                }
            }
        }

        /// <summary>
        /// Request the guild information
        /// </summary>
        /// <param name="id">Id of the guild</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public  async Task<GuildInformation> GetGuildInformation(string id)
        {
            using (var response = await CreateRequest($"https://api.guildwars2.com/v2/guild/{id}?v=latest").GetResponseAsync()
                                                                                                                                          .ConfigureAwait(false))
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var jsonResult = await reader.ReadToEndAsync().ConfigureAwait(false);

                    return JsonConvert.DeserializeObject<GuildInformation>(jsonResult);
                }
            }
        }

        /// <summary>
        /// Request the guild log
        /// </summary>
        /// <param name="guildId">Id of the log</param>
        /// <param name="sinceId">Since id</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<List<GuildLogEntry>> GetGuildLogEntries(string guildId, long sinceId)
        {
            using (var response = await CreateRequest($"https://api.guildwars2.com/v2/guild/{guildId}/log?since={sinceId}?v=latest").GetResponseAsync()
                                                                                                                                    .ConfigureAwait(false))
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var jsonResult = await reader.ReadToEndAsync().ConfigureAwait(false);

                    return JsonConvert.DeserializeObject<List<GuildLogEntry>>(jsonResult);
                }
            }
        }

        /// <summary>
        /// Create a new request
        /// </summary>
        /// <param name="uri">Uri</param>
        /// <returns>The created request</returns>
        private HttpWebRequest CreateRequest(string uri)
        {
            var request = WebRequest.CreateHttp(uri);

            request.Headers.Add("Authorization", "Bearer " + _apiKey);

            return request;
        }

        #endregion // Methods

        #region IAsyncDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        public async ValueTask DisposeAsync()
        {
            await Task.Run(Dispose).ConfigureAwait(false);
        }

        #endregion // IAsyncDisposable

        #region IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion // IDisposable
    }
}
