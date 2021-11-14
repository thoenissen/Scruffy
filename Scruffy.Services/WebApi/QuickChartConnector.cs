using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Scruffy.Data.Json.QuickChart;

namespace Scruffy.Services.WebApi
{
    /// <summary>
    /// QuickChart.io - Connector
    /// </summary>
    public sealed class QuickChartConnector : IDisposable,
                                              IAsyncDisposable
    {
        #region Methods

        /// <summary>
        /// Request the account information
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<Stream> GetChartAsStream(ChartData data)
        {
            var request = WebRequest.CreateHttp("https://quickchart.io:443/chart");

            request.Method = HttpMethod.Post.ToString();
            request.ContentType = "application/json";

            var requestStream = await request.GetRequestStreamAsync()
                                             .ConfigureAwait(false);
            await using (requestStream.ConfigureAwait(false))
            {
                var jsonData = JsonConvert.SerializeObject(data,
                                                           new JsonSerializerSettings
                                                           {
                                                               NullValueHandling = NullValueHandling.Ignore
                                                           });

                await requestStream.WriteAsync(Encoding.UTF8.GetBytes(jsonData))
                                   .ConfigureAwait(false);

                await requestStream.FlushAsync()
                                   .ConfigureAwait(false);

                using (var response = await request.GetResponseAsync()
                                                   .ConfigureAwait(false))
                {
                    var stream = response.GetResponseStream();
                    await using (stream.ConfigureAwait(false))
                    {
                        var memoryStream = new MemoryStream();

                        await stream.CopyToAsync(memoryStream)
                                    .ConfigureAwait(false);

                        memoryStream.Position = 0;

                        return memoryStream;
                    }
                }
            }
        }

        #endregion // Methods

        #region IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
        }

        #endregion // IDisposable

        #region IAsyncDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.</summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        #endregion // IAsyncDisposable
    }
}