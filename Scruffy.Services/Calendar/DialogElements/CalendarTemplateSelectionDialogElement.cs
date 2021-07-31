using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.Entities;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Calendar;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;

namespace Scruffy.Services.Calendar.DialogElements
{
    /// <summary>
    /// Selection of a template
    /// </summary>
    public class CalendarTemplateSelectionDialogElement : DialogEmbedMessageElementBase<long>
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
        public CalendarTemplateSelectionDialogElement(LocalizationService localizationService)
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
            builder.WithTitle(LocalizationGroup.GetText("ChooseTemplateTitle", "Calendar template selection"));
            builder.WithDescription(LocalizationGroup.GetText("ChooseTemplateDescription", "Please choose one of the following calendar templates:"));

            _templates = new Dictionary<int, long>();
            var levelsFieldsText = new StringBuilder();

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var serverId = CommandContext.Guild.Id;

                var mainRoles = dbFactory.GetRepository<CalendarAppointmentTemplateRepository>()
                                         .GetQuery()
                                         .Where(obj => obj.IsDeleted == false
                                                       && obj.ServerId == serverId)
                                         .Select(obj => new
                                         {
                                             obj.Id,
                                             obj.Description
                                         })
                                         .OrderBy(obj => obj.Description)
                                         .ToList();

                var i = 1;
                foreach (var role in mainRoles)
                {
                    levelsFieldsText.Append('`');
                    levelsFieldsText.Append(i);
                    levelsFieldsText.Append("` - ");
                    levelsFieldsText.Append(' ');
                    levelsFieldsText.Append(role.Description);
                    levelsFieldsText.Append('\n');

                    _templates[i] = role.Id;

                    i++;
                }

                builder.AddField(LocalizationGroup.GetText("TemplateField", "Templates"), levelsFieldsText.ToString());
            }

            return builder;
        }

        /// <summary>
        /// Converting the response message
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>Result</returns>
        public override Task<long> ConvertMessage(DiscordMessage message)
        {
            return Task.FromResult(int.TryParse(message.Content, out var index) && _templates.TryGetValue(index, out var selectedTemplateId) ? selectedTemplateId : throw new InvalidOperationException());
        }

        #endregion // DialogEmbedMessageElementBase<long>
    }
}