using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;

using Newtonsoft.Json;

using Scruffy.Data.Json.Tenor;

namespace Scruffy.Services.Core.Discord
{
    /// <summary>
    /// Handling error events
    /// </summary>
    public sealed class DiscordErrorHandler : IDisposable
    {
        #region Fields

        /// <summary>
        /// Commands interface
        /// </summary>
        private readonly CommandsNextExtension _commandsNextExtension;

        /// <summary>
        /// Random number generation
        /// </summary>
        private Random _rnd;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="commandsNextExtension">Commands interface</param>
        public DiscordErrorHandler(CommandsNextExtension commandsNextExtension)
        {
            _rnd = new Random(DateTime.Now.Millisecond);

            _commandsNextExtension = commandsNextExtension;
            _commandsNextExtension.CommandErrored += OnCommandErrored;
        }

        #region Methods

        /// <summary>
        /// Triggered whenever a command throws an exception during execution.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task OnCommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            if (e.Exception is CommandNotFoundException)
            {
                using (var response = await WebRequest.CreateHttp("https://g.tenor.com/v1/search?q=what&key=RXM3VE2UGRU9&limit=100")
                                                      .GetResponseAsync()
                                                      .ConfigureAwait(false))
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        var jsonResult = await reader.ReadToEndAsync().ConfigureAwait(false);

                        var searchResult = JsonConvert.DeserializeObject<SearchResultRoot>(jsonResult);

                        await e.Context
                               .Message
                               .RespondAsync(searchResult.Results[_rnd.Next(0, searchResult.Results.Count - 1)].ItemUrl)
                               .ConfigureAwait(false);
                    }
                }
            }
        }

        #endregion // Methods

        #region IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _commandsNextExtension.CommandErrored -= OnCommandErrored;
        }

        #endregion // IDisposable
    }
}
