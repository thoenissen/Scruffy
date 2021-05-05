using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;

using Scruffy.Commands.Base;
using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Fractals;
using Scruffy.Data.Entity.Tables.Fractals;
using Scruffy.Services.Core;
using Scruffy.Services.Fractals;

namespace Scruffy.Commands.Fractals
{
    /// <summary>
    /// Fractal lfg setup commands
    /// </summary>
    [ModuleLifespan(ModuleLifespan.Transient)]
    [Group("fractal")]
    public class FractalLfgSetupCommandModule : LocatedCommandModuleBase
    {
        #region Properties

        /// <summary>
        /// Message builder
        /// </summary>
        public FractalLfgMessageBuilder MessageBuilder { get; set; }

        #endregion // Properties

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public FractalLfgSetupCommandModule(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Creation of a new lfg entry
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("setup")]
        public async Task Setup(CommandContext commandContext)
        {
            string title = null;
            string description = null;

            var interactivity = commandContext.Client.GetInteractivity();

            await commandContext.RespondAsync(LocalizationGroup.GetText("TitlePrompt", "Please enter a title.")).ConfigureAwait(false);

            var responseMessage = await interactivity.WaitForMessageAsync(obj => obj.Author.Id == commandContext.Message.Author.Id).ConfigureAwait(false);

            if (responseMessage.TimedOut == false)
            {
                title = responseMessage.Result.Content;

                await responseMessage.Result.RespondAsync(LocalizationGroup.GetText("DescriptionPrompt", "Please enter a description.")).ConfigureAwait(false);

                responseMessage = await interactivity.WaitForMessageAsync(obj => obj.Author.Id == commandContext.Message.Author.Id).ConfigureAwait(false);
            }

            if (responseMessage.TimedOut == false)
            {
                description = responseMessage.Result.Content;

                await responseMessage.Result.RespondAsync(LocalizationGroup.GetText("AliasNamePrompt", "Please enter a alias name.")).ConfigureAwait(false);

                responseMessage = await interactivity.WaitForMessageAsync(obj => obj.Author.Id == commandContext.Message.Author.Id).ConfigureAwait(false);
            }

            if (responseMessage.TimedOut == false)
            {
                var aliasName = responseMessage.Result.Content;

                var entry = new FractalLfgConfigurationEntity
                            {
                                Title = title,
                                Description = description,
                                AliasName = aliasName,
                                ChannelId = commandContext.Channel.Id,
                                MessageId = (await commandContext.Channel.SendMessageAsync(LocalizationGroup.GetText("BuildingProgress", "Building...")).ConfigureAwait(false)).Id
                            };

                using (var dbFactory = RepositoryFactory.CreateInstance())
                {
                    dbFactory.GetRepository<FractalLfgConfigurationRepository>()
                             .Add(entry);
                }

                await MessageBuilder.RefreshMessageAsync(entry.Id).ConfigureAwait(false);
            }
        }

        #endregion // Methods
    }
}
