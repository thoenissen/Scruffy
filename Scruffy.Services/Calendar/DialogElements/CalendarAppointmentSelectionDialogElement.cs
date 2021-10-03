using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Calendar;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Calendar.DialogElements
{
    /// <summary>
    /// Selection of an appointment
    /// </summary>
    public class CalendarAppointmentSelectionDialogElement : DialogEmbedMessageElementBase<long>
    {
        #region Fields

        /// <summary>
        /// Appointments
        /// </summary>
        private Dictionary<int, long> _appointments;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public CalendarAppointmentSelectionDialogElement(LocalizationService localizationService)
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
            builder.WithTitle(LocalizationGroup.GetText("ChooseTitle", "Appointment selection"));
            builder.WithDescription(LocalizationGroup.GetText("ChooseDescription", "Please choose one of the following appointments:"));

            _appointments = new Dictionary<int, long>();
            var fieldText = new StringBuilder();

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var limit = DateTime.Now.AddMinutes(15);

                var entries = dbFactory.GetRepository<CalendarAppointmentRepository>()
                                         .GetQuery()
                                         .Where(obj => obj.TimeStamp < limit)
                                         .Select(obj => new
                                                        {
                                                            obj.Id,
                                                            obj.TimeStamp,
                                                            obj.CalendarAppointmentTemplate.Description
                                                        })
                                         .OrderByDescending(obj => obj.TimeStamp)
                                         .Take(10)
                                         .ToList();

                var i = 0;
                foreach (var entry in entries)
                {
                    fieldText.Append('`');
                    fieldText.Append(i);
                    fieldText.Append("` - ");
                    fieldText.Append(' ');
                    fieldText.Append(Formatter.Bold(entry.Description));
                    fieldText.Append(' ');
                    fieldText.Append('(');
                    fieldText.Append(entry.TimeStamp.ToString("g", LocalizationGroup.CultureInfo));
                    fieldText.Append(')');
                    fieldText.Append('\n');

                    _appointments[i] = entry.Id;

                    i++;
                }

                builder.AddField(LocalizationGroup.GetText("Field", "Appointments"), fieldText.ToString());
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
            return Task.FromResult(int.TryParse(message.Content, out var index) && _appointments.TryGetValue(index, out var selected) ? selected : throw new InvalidOperationException());
        }

        #endregion // DialogEmbedMessageElementBase<long>
    }
}