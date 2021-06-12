using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DSharpPlus.Entities;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;

namespace Scruffy.Services.Raid.DialogElements
{
    /// <summary>
    /// Selection of a template
    /// </summary>
    public class RaidTemplateSelectionDialogElement : DialogEmbedMessageElementBase<long>
    {
        #region Fields

        /// <summary>
        /// Templates
        /// </summary>
        private Dictionary<int, long> _templates;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public RaidTemplateSelectionDialogElement(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region DialogEmbedMessageElementBase<long>

        /// <summary>
        /// Return the message of element
        /// </summary>
        /// <returns>Message</returns>
        public override DiscordEmbedBuilder GetMessage()
        {
            var builder = new DiscordEmbedBuilder();
            builder.WithTitle(LocalizationGroup.GetText("ChooseTemplateTitle", "Raid template selection"));
            builder.WithDescription(LocalizationGroup.GetText("ChooseTemplateDescription", "Please choose one of the following templates:"));

            _templates = new Dictionary<int, long>();
            var rolesFieldText = new StringBuilder();

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var mainRoles = dbFactory.GetRepository<RaidDayTemplateRepository>()
                                         .GetQuery()
                                         .Where(obj => obj.IsDeleted == false)
                                         .Select(obj => new
                                         {
                                             obj.Id,
                                             obj.AliasName
                                         })
                                         .OrderBy(obj => obj.AliasName)
                                         .ToList();

                var i = 1;
                foreach (var role in mainRoles)
                {
                    rolesFieldText.Append('`');
                    rolesFieldText.Append(i);
                    rolesFieldText.Append("` - ");
                    rolesFieldText.Append(' ');
                    rolesFieldText.Append(role.AliasName);
                    rolesFieldText.Append('\n');

                    _templates[i] = role.Id;

                    i++;
                }

                builder.AddField(LocalizationGroup.GetText("TemplatesField", "Templates"), rolesFieldText.ToString());
            }

            return builder;
        }

        /// <summary>
        /// Converting the response message
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>Result</returns>
        public override long ConvertMessage(DiscordMessage message)
        {
            return int.TryParse(message.Content, out var index) && _templates.TryGetValue(index, out var selectedRoleId)
                       ? selectedRoleId
                       : throw new InvalidOperationException();
        }

        #endregion // DialogEmbedMessageElementBase<long>
    }
}
