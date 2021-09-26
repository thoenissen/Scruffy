using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Scruffy.Data.Json.ThatShaman;

namespace Scruffy.Services.WebApi
{
    /// <summary>
    /// thatshaman.com connector
    /// </summary>
    public sealed class ThatShamanConnector : IAsyncDisposable, IDisposable
    {
        #region Methods

        /// <summary>
        /// Get the next update
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<NextUpdateData> GetNextUpdate()
        {
            using (var response = await WebRequest.Create("https://thatshaman.com/tools/countdown/?format=json")
                                                  .GetResponseAsync()
                                                  .ConfigureAwait(false))
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var jsonResult = await reader.ReadToEndAsync()
                                                 .ConfigureAwait(false);

                    return JsonConvert.DeserializeObject<NextUpdateData>(jsonResult);
                }
            }
        }

        /// <summary>
        /// Get the next update
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<NextUpdateData> GetEODRelease()
        {
            using (var response = await WebRequest.Create("https://thatshaman.com/tools/eod/?format=json")
                                                  .GetResponseAsync()
                                                  .ConfigureAwait(false))
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var jsonResult = await reader.ReadToEndAsync()
                                                 .ConfigureAwait(false);

                    return JsonConvert.DeserializeObject<NextUpdateData>(jsonResult);
                }
            }
        }

        #endregion // Methods

        #region IAsyncDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        #endregion // IAsyncDisposable

        #region IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        #endregion // IDisposable
    }
}
