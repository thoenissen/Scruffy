using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace Scruffy.Services.Core.Discord
{
    /// <summary>
    /// Dialog element with message response
    /// </summary>
    /// <typeparam name="TData">Type of the result</typeparam>
    public abstract class DialogEmbedMessageElementBase<TData> : DialogElementBase<TData>
    {
        #region Fields

        /// <summary>
        /// Localization service
        /// </summary>
        private LocalizationService _localizationService;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        protected DialogEmbedMessageElementBase(LocalizationService localizationService)
            : base(localizationService)
        {
            _localizationService = localizationService;
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Return the message of element
        /// </summary>
        /// <returns>Message</returns>
        public abstract DiscordEmbedBuilder GetMessage();

        /// <summary>
        /// Converting the response message
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual Task<TData> ConvertMessage(DiscordMessage message) => Task.FromResult((TData)Convert.ChangeType(message.Content, typeof(TData)));

        /// <summary>
        /// Execute the dialog element
        /// </summary>
        /// <returns>Result</returns>
        public override async Task<TData> Run()
        {
            var repeat = false;

            do
            {
                var currentBotMessage = await CommandContext.Channel
                                                            .SendMessageAsync(GetMessage())
                                                            .ConfigureAwait(false);

                DialogContext.Messages.Add(currentBotMessage);

                var currentUserResponse = await CommandContext.Client
                                                              .GetInteractivity()
                                                              .WaitForMessageAsync(obj => obj.Author.Id == CommandContext.User.Id
                                                                                       && obj.ChannelId == CommandContext.Channel.Id)
                                                              .ConfigureAwait(false);

                if (currentUserResponse.TimedOut == false)
                {
                    CommandContext.LastUserMessage = currentUserResponse.Result;

                    DialogContext.Messages.Add(currentUserResponse.Result);

                    try
                    {
                        return await ConvertMessage(currentUserResponse.Result).ConfigureAwait(false);
                    }
                    catch (Exception ex) when (ex is InvalidOperationException || ex is KeyNotFoundException)
                    {
                        var repeatMessage = await CommandContext.Channel
                                                                .SendMessageAsync(_localizationService.GetGroup(nameof(DialogElementBase))
                                                                                                      .GetText("PleaseTryAgain", "Your input was invalid. Please try again."))
                                                                .ConfigureAwait(false);

                        DialogContext.Messages.Add(repeatMessage);

                        repeat = true;
                    }
                }
            }
            while (repeat);

            throw new TimeoutException();
        }

        #endregion // Methods
    }
}