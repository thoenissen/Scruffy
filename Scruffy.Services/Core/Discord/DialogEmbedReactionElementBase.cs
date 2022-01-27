
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Core.Discord;

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
    public virtual Task EditMessage(DiscordEmbedBuilder builder) => Task.CompletedTask;

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
        var builder = new DiscordEmbedBuilder();

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
                                                    .SendMessageAsync(builder)
                                                    .ConfigureAwait(false);

        DialogContext.Messages.Add(currentBotMessage);

        var userReactionTask = CommandContext.Client
                                             .GetInteractivity()
                                             .WaitForReactionAsync(currentBotMessage, CommandContext.User);

        if (reactions?.Count > 0)
        {
            foreach (var reaction in reactions)
            {
                await currentBotMessage.CreateReactionAsync(reaction.Emoji).ConfigureAwait(false);
            }
        }

        var userReaction = await userReactionTask.ConfigureAwait(false);

        if (userReaction.TimedOut == false)
        {
            if (reactions?.Count > 0)
            {
                foreach (var reaction in reactions)
                {
                    if (reaction.Emoji.ToString() == userReaction.Result.Emoji.ToString())
                    {
                        return await reaction.Func().ConfigureAwait(false);
                    }
                }
            }

            return DefaultFunc();
        }

        throw new TimeoutException();
    }

    #endregion // Methods
}