using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using Scruffy.Data.Json.Tenor;
using Scruffy.Services.Core.Localization;

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
            else if (e.Exception is ArgumentException
                  || e.Exception is InvalidOperationException)
            {
                var cmd = e.Context.CommandsNext.FindCommand("help " + e.Command.QualifiedName, out var customArgs);

                cmd ??= e.Context.CommandsNext.FindCommand("help", out customArgs);

                if (e.Context.Channel.IsPrivate)
                {
                    if (e.Context.Client.PrivateChannels.ContainsKey(e.Context.Channel.Id) == false)
                    {
                        if (e.Context.Member != null)
                        {
                            await e.Context
                                   .Member
                                   .CreateDmChannelAsync()
                                   .ConfigureAwait(false);
                        }
                    }
                }

                var fakeContext = e.Context.CommandsNext.CreateFakeContext(e.Context.Member ?? e.Context.User, e.Context.Channel, "help " + e.Command.QualifiedName, e.Context.Prefix, cmd, customArgs);

                await e.Context
                       .CommandsNext
                       .ExecuteCommandAsync(fakeContext)
                       .ConfigureAwait(false);
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
