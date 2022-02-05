using Discord;

using Scruffy.Services.Core.Exceptions;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Discord;

/// <summary>
/// Dialog element with reactions
/// </summary>
/// <typeparam name="TData">Type of the result</typeparam>
public abstract class DialogEmbedReactionElementBase<TData> : DialogElementBase<TData>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    protected DialogEmbedReactionElementBase(LocalizationService localizationService)
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
    /// <param name="builder">Builder</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public virtual Task EditMessage(EmbedBuilder builder) => Task.CompletedTask;

    /// <summary>
    /// Returns the title of the commands
    /// </summary>
    /// <returns>Commands</returns>
    protected abstract string GetCommandTitle();

    /// <summary>
    /// Default case if none of the given reactions is used
    /// </summary>
    /// <returns>Result</returns>
    protected abstract TData DefaultFunc();

    /// <summary>
    /// Execute the dialog element
    /// </summary>
    /// <returns>Result</returns>
    public override async Task<TData> Run()
    {
        var builder = new EmbedBuilder();

        await EditMessage(builder).ConfigureAwait(false);

        var reactions = GetReactions();

        if (reactions?.Count > 0)
        {
            var commands = new StringBuilder();
            foreach (var reaction in reactions)
            {
                commands.AppendLine(reaction.CommandText);
            }

            builder.AddField(GetCommandTitle(), commands.ToString());
        }

        var currentBotMessage = await CommandContext.Channel
                                                    .SendMessageAsync(embed: builder.Build())
                                                    .ConfigureAwait(false);

        DialogContext.Messages.Add(currentBotMessage);

        var userReactionTask = CommandContext.Interaction
                                             .WaitForReactionAsync(currentBotMessage, CommandContext.User);

        if (reactions?.Count > 0)
        {
            foreach (var reaction in reactions)
            {
                await currentBotMessage.AddReactionAsync(reaction.Emote).ConfigureAwait(false);
            }
        }

        var userReaction = await userReactionTask.ConfigureAwait(false);
        if (userReaction != null)
        {
            if (reactions?.Count > 0)
            {
                foreach (var reaction in reactions)
                {
                    if (reaction.Emote.Name == userReaction.Emote.Name)
                    {
                        return await reaction.Func().ConfigureAwait(false);
                    }
                }
            }

            return DefaultFunc();
        }

        throw new ScruffyTimeoutException();
    }

    #endregion // Methods
}