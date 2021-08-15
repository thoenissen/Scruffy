using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Scruffy.Data.Json.GuildWars2.Account;
using Scruffy.Data.Json.GuildWars2.Core;
using Scruffy.Data.Json.GuildWars2.Guild;
using Scruffy.Data.Json.GuildWars2.Items;
using Scruffy.Data.Json.GuildWars2.Quaggans;

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
        /// Request the token information
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<TokenInformation> GetTokenInformationAsync()
        {
            using (var response = await CreateRequest("https://api.guildwars2.com/v2/tokeninfo").GetResponseAsync()
                                                                                                .ConfigureAwait(false))
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var jsonResult = await reader.ReadToEndAsync().ConfigureAwait(false);

                    return JsonConvert.DeserializeObject<TokenInformation>(jsonResult);
                }
            }
        }

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
        /// Request all available guild emblem foregrounds
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<List<long>> GetGuildEmblemForegrounds()
        {
            using (var response = await CreateRequest($"https://api.guildwars2.com/v2/emblem/foregrounds").GetResponseAsync()
                                                                                                                   .ConfigureAwait(false))
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var jsonResult = await reader.ReadToEndAsync().ConfigureAwait(false);

                    return JsonConvert.DeserializeObject<List<long>>(jsonResult);
                }
            }
        }

        /// <summary>
        /// Request all available guild emblem backgrounds
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<List<long>> GetGuildEmblemBackgrounds()
        {
            using (var response = await CreateRequest($"https://api.guildwars2.com/v2/emblem/backgrounds").GetResponseAsync()
                                                                                                                                    .ConfigureAwait(false))
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var jsonResult = await reader.ReadToEndAsync().ConfigureAwait(false);

                    return JsonConvert.DeserializeObject<List<long>>(jsonResult);
                }
            }
        }

        /// <summary>
        /// Request the layers of a guild emblem background
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<GuildEmblemLayerData> GetGuildEmblemBackgroundLayer(long id)
        {
            using (var response = await CreateRequest($"https://api.guildwars2.com/v2/emblem/backgrounds?ids={id}").GetResponseAsync()
                                                                                                                   .ConfigureAwait(false))
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var jsonResult = await reader.ReadToEndAsync().ConfigureAwait(false);

                    return JsonConvert.DeserializeObject<List<GuildEmblemLayerData>>(jsonResult).FirstOrDefault();
                }
            }
        }

        /// <summary>
        /// Request the layers of a guild emblem foreground
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<GuildEmblemLayerData> GetGuildEmblemForegroundLayer(long id)
        {
            using (var response = await CreateRequest($"https://api.guildwars2.com/v2/emblem/foregrounds?ids={id}").GetResponseAsync()
                                                                                                                            .ConfigureAwait(false))
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var jsonResult = await reader.ReadToEndAsync().ConfigureAwait(false);

                    return JsonConvert.DeserializeObject<List<GuildEmblemLayerData>>(jsonResult).FirstOrDefault();
                }
            }
        }

        /// <summary>
        /// Request the guild stash
        /// </summary>
        /// <param name="guildId">Id of the guild</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<List<GuildStash>> GetGuildVault(string guildId)
        {
            using (var response = await CreateRequest($"https://api.guildwars2.com/v2/guild/{guildId}/stash").GetResponseAsync()
                                                                                                             .ConfigureAwait(false))
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var jsonResult = await reader.ReadToEndAsync().ConfigureAwait(false);

                    return JsonConvert.DeserializeObject<List<GuildStash>>(jsonResult);
                }
            }
        }

        /// <summary>
        /// Request the item data
        /// </summary>
        /// <param name="itemId">Id of the item</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<Item> GetItem(int itemId)
        {
            using (var response = await CreateRequest($"https://api.guildwars2.com/v2/items/{itemId}").GetResponseAsync()
                                                                                                      .ConfigureAwait(false))
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var jsonResult = await reader.ReadToEndAsync().ConfigureAwait(false);

                    return JsonConvert.DeserializeObject<Item>(jsonResult);
                }
            }
        }

        /// <summary>
        /// Request the list of quaggans
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<List<string>> GetQuaggans()
        {
            using (var response = await CreateRequest($"https://api.guildwars2.com/v2/quaggans").GetResponseAsync()
                                                                                                .ConfigureAwait(false))
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var jsonResult = await reader.ReadToEndAsync().ConfigureAwait(false);

                    return JsonConvert.DeserializeObject<List<string>>(jsonResult);
                }
            }
        }

        /// <summary>
        /// Request the quaggan data
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<QuagganData> GetQuaggan(string name)
        {
            using (var response = await CreateRequest($"https://api.guildwars2.com/v2/quaggans/{name}").GetResponseAsync()
                                                                                                       .ConfigureAwait(false))
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var jsonResult = await reader.ReadToEndAsync().ConfigureAwait(false);

                    return JsonConvert.DeserializeObject<QuagganData>(jsonResult);
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

            if (_apiKey != null)
            {
                request.Headers.Add("Authorization", "Bearer " + _apiKey);
            }

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
