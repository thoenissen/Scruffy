using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Extensions;

using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Core.Discord;

/// <summary>
/// Dialog element with reactions
/// </summary>
/// <typeparam name="TData">Type of the result</typeparam>
public abstract class DialogReactionElementBase<TData> : DialogElementBase<TData>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    protected DialogReactionElementBase(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Returns the reactions which should be added to the message
    /// </summary>
    /// <returns>Reactions</returns>
    public virtual IReadOnlyList<ReactionData<TData>> GetReactions() => null;

    /// <summary>
    /// Editing the embedded message
    /// </summary>
    /// <returns>Message</returns>
    public abstract string GetMessage();

    /// <summary>
    /// Default case if none of the given reactions is used
    /// </summary>
    /// <param name="reaction">Reaction</param>
    /// <returns>Result</returns>
    protected abstract TData DefaultFunc(MessageReactionAddEventArgs reaction);

    /// <summary>
    /// Execute the dialog element
    /// </summary>
    /// <returns>Result</returns>
    public override async Task<TData> Run()
    {
        var message = await CommandContext.Channel
                                          .SendMessageAsync(GetMessage())
                                          .ConfigureAwait(false);

        DialogContext.Messages.Add(message);

        var userReactionTask = CommandContext.Client
                                             .GetInteractivity()
                                             .WaitForReactionAsync(message, CommandContext.User);

        var reactions = GetReactions();
        if (reactions?.Count > 0)
        {
            foreach (var reaction in reactions)
            {
                await message.CreateReactionAsync(reaction.Emoji).ConfigureAwait(false);
            }
        }

        var userReaction = await userReactionTask.ConfigureAwait(false);

        if (userReaction.TimedOut == false)
        {
            if (reactions?.Count > 0)
            {
                foreach (var reaction in reactions)
                {
                    if (reaction.Emoji.Id == userReaction.Result.Emoji.Id)
                    {
                        return await reaction.Func().ConfigureAwait(false);
                    }
                }
            }

            return DefaultFunc(userReaction.Result);
        }

        throw new TimeoutException();
    }

    #endregion // Methods
}