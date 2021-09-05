using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.Entities;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;

namespace Scruffy.Services.Raid.DialogElements
{
    /// <summary>
    /// Selection of a role
    /// </summary>
    public class RaidRoleSelectionDialogElement : DialogEmbedMessageElementBase<long?>
    {
        #region Fields

        /// <summary>
        /// Templates
        /// </summary>
        private Dictionary<int, long?> _roles;

        /// <summary>
        /// Id of the main role
        /// </summary>
        private long? _mainRoleId;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        /// <param name="mainRoleId">Id of the main role</param>
        public RaidRoleSelectionDialogElement(LocalizationService localizationService, long? mainRoleId)
            : base(localizationService)
        {
            _mainRoleId = mainRoleId;
        }

        #endregion // Constructor

        #region DialogEmbedMessageElementBase<long?>

        /// <summary>
        /// Return the message of element
        /// </summary>
        /// <returns>Message</returns>
        public override DiscordEmbedBuilder GetMessage()
        {
            var builder = new DiscordEmbedBuilder();
            builder.WithTitle(LocalizationGroup.GetText("ChooseLevelTitle", "Raid role selection"));
            builder.WithDescription(LocalizationGroup.GetText("ChooseLevelDescription", "Please choose one of the following roles:"));

            _roles = new Dictionary<int, long?>();
            var levelsFieldsText = new StringBuilder();

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var mainRoles = dbFactory.GetRepository<RaidRoleRepository>()
                                         .GetQuery()
                                         .Where(obj => obj.MainRoleId == _mainRoleId
                                                    && obj.IsDeleted == false)
                                         .Select(obj => new
                                         {
                                             obj.Id,
                                             obj.Description,
                                             obj.DiscordEmojiId
                                         })
                                         .OrderBy(obj => obj.Description)
                                         .ToList();

                levelsFieldsText.Append("`0` - ");
                levelsFieldsText.Append(LocalizationGroup.GetText("NoRole", "No additional role"));
                levelsFieldsText.Append('\n');
                _roles[0] = null;

                var i = 1;
                foreach (var role in mainRoles)
                {
                    levelsFieldsText.Append('`');
                    levelsFieldsText.Append(i);
                    levelsFieldsText.Append("` - ");
                    levelsFieldsText.Append(' ');
                    levelsFieldsText.Append(DiscordEmojiService.GetGuildEmoji(CommandContext.Client, role.DiscordEmojiId));
                    levelsFieldsText.Append(' ');
                    levelsFieldsText.Append(role.Description);
                    levelsFieldsText.Append('\n');

                    _roles[i] = role.Id;

                    i++;
                }

                builder.AddField(LocalizationGroup.GetText("RolesField", "Roles"), levelsFieldsText.ToString());
            }

            return builder;
        }

        /// <summary>
        /// Converting the response message
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override Task<long?> ConvertMessage(DiscordMessage message)
        {
            return Task.FromResult(int.TryParse(message.Content, out var index) && _roles.TryGetValue(index, out var selectedRoleId) ? selectedRoleId : throw new InvalidOperationException());
        }

        #endregion // DialogEmbedMessageElementBase<long?>
    }
}
