using Discord;

using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Discord;

/// <summary>
/// Dialog element with message response
/// </summary>
/// <typeparam name="TData">Type of the result</typeparam>
public abstract class DialogMessageElementBase<TData> : DialogElementBase<TData>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    protected DialogMessageElementBase(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Return the message of element
    /// </summary>
    /// <returns>Message</returns>
    public abstract string GetMessage();

    /// <summary>
    /// Converting the response message
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>Result</returns>
    public virtual TData ConvertMessage(IUserMessage message) => (TData)Convert.ChangeType(message.Content, typeof(TData));

    /// <summary>
    /// Execute the dialog element
    /// </summary>
    /// <returns>Result</returns>
    public override async Task<TData> Run()
    {
        var currentBotMessage = await CommandContext.Channel
                                                    .SendMessageAsync(GetMessage())
                                                    .ConfigureAwait(false);

        DialogContext.Messages.Add(currentBotMessage);

        var currentUserResponse = await CommandContext.Interaction
                                                      .WaitForMessageAsync(obj => obj.Author.Id == CommandContext.User.Id
                                                                          && obj.Channel.Id == CommandContext.Channel.Id)
                                                      .ConfigureAwait(false);

        DialogContext.Messages.Add(currentUserResponse);

        return ConvertMessage(currentUserResponse);
    }

    #endregion // Methods
}