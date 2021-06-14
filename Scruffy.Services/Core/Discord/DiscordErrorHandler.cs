using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.General;
using Scruffy.Data.Entity.Tables.General;
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

        /// <summary>
        /// Localized group
        /// </summary>
        private LocalizationGroup _localizationGroup;

        #endregion

        #region Constructor

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

        #endregion // Constructor

        #region Properties

        /// <summary>
        /// Localized group
        /// </summary>
        public LocalizationGroup LocalizationGroup => _localizationGroup ??= GlobalServiceProvider.Current.GetServiceProvider().GetService<LocalizationService>().GetGroup(GetType().Name);

        #endregion // Properties

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
                using (var response = await WebRequest.CreateHttp("https://g.tenor.com/v1/search?q=what&key=RXM3VE2UGRU9&limit=100&contentfilter=high&ar_range=all")
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
            else if (e.Exception is TimeoutException)
            {
            }
            else
            {
                if (e.Context?.Channel != null)
                {
                    using (var dbFactory = RepositoryFactory.CreateInstance())
                    {
                        var logEntry = new LogEntryEntity
                                       {
                                           Message = e.Exception.ToString(),
                                           QualifiedCommandName = e.Command?.QualifiedName
                                       };

                        dbFactory.GetRepository<LogEntryRepository>()
                                 .Add(logEntry);

                        using (var response = await WebRequest.CreateHttp("https://g.tenor.com/v1/search?q=funny%20cat&key=RXM3VE2UGRU9&limit=50&contentfilter=high&ar_range=all")
                                                              .GetResponseAsync()
                                                              .ConfigureAwait(false))
                        {
                            using (var reader = new StreamReader(response.GetResponseStream()))
                            {
                                var jsonResult = await reader.ReadToEndAsync().ConfigureAwait(false);

                                var searchResult = JsonConvert.DeserializeObject<SearchResultRoot>(jsonResult);

                                using (var webClient = new WebClient())
                                {
                                    var tenorEntry = searchResult.Results[_rnd.Next(0, searchResult.Results.Count - 1)];

                                    await using (var stream = new MemoryStream(webClient.DownloadData(tenorEntry.Media[0].GIF.Url)))
                                    {
                                        var builder = new DiscordMessageBuilder().WithContent(LocalizationGroup.GetFormattedText("CommandFailedMessage", "The command could not be executed. But I have an error code ({0}) and funny cat picture.", logEntry.Id))
                                                                                 .WithFile("cat.gif", stream);

                                        await e.Context
                                               .Channel
                                               .SendMessageAsync(builder)
                                               .ConfigureAwait(false);
                                    }
                                }
                            }
                        }
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
