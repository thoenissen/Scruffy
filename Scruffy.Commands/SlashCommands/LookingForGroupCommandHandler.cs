using Discord.Interactions;

using Scruffy.Services.Discord;
using Scruffy.Services.LookingForGroup;

namespace Scruffy.Commands.SlashCommands
{
    /// <summary>
    /// Looking for group command handler
    /// </summary>
    [Group("lfg", "Looking for group commands")]
    public class LookingForGroupCommandModule : SlashCommandModuleBase
    {
        #region Properties

        /// <summary>
        /// Command handler
        /// </summary>
        public LookingForGroupCommandHandler CommandHandler { get; set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Creation of an new lfg appointment
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [SlashCommand("create", "Creation of an new lfg appointment")]
        public Task Create() => CommandHandler.StartCreation(Context);

        #endregion // Methods
    }
}