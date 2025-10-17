using Discord;

using Scruffy.Services.Core.Exceptions;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Discord;

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
    private readonly LocalizationService _localizationService;

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
    public abstract EmbedBuilder GetMessage();

    /// <summary>
    /// Converting the response message
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public virtual Task<TData> ConvertMessage(IUserMessage message) => Task.FromResult((TData)Convert.ChangeType(message.Content, typeof(TData)));

    /// <inheritdoc/>
    public override async Task<TData> Run()
    {
        var repeat = false;

        do
        {
            var currentBotMessage = await CommandContext.SendMessageAsync(embed: GetMessage().Build())
                                                        .ConfigureAwait(false);

            DialogContext.Messages.Add(currentBotMessage);

            var currentUserResponse = await CommandContext.Interactivity
                                                          .WaitForMessageAsync(obj => obj.Author.Id == CommandContext.User.Id
                                                                              && obj.Channel.Id == CommandContext.Channel.Id)
                                                          .ConfigureAwait(false);

            if (currentUserResponse != null)
            {
                DialogContext.Messages.Add(currentUserResponse);

                try
                {
                    return await ConvertMessage(currentUserResponse).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
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

        throw new ScruffyTimeoutException();
    }

    #endregion // Methods
}