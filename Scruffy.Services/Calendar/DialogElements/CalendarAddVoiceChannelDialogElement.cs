using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Calendar.DialogElements
{
    /// <summary>
    /// Adding participants
    /// </summary>
    public class CalendarAddVoiceChannelDialogElement : DialogEmbedMessageElementBase<List<DiscordMember>>
    {
        #region Fields

        /// <summary>
        /// Appointments
        /// </summary>
        private Dictionary<int, ulong> _channels;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public CalendarAddVoiceChannelDialogElement(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region DialogEmbedMessageElementBase

        /// <summary>
        /// Return the message of element
        /// </summary>
        /// <returns>Message</returns>
        public override DiscordEmbedBuilder GetMessage()
        {
            var builder = new DiscordEmbedBuilder();
            builder.WithTitle(LocalizationGroup.GetText("ChooseTitle", "Voice channel selection"));
            builder.WithDescription(LocalizationGroup.GetText("ChooseDescription", "Please choose one of the voice channels:"));

            _channels = new Dictionary<int, ulong>();
            var fieldText = new StringBuilder();

            var i = 0;

            foreach (var (key, channel) in CommandContext.Guild.Channels.Where(obj => obj.Value.Type == ChannelType.Voice))
            {
                fieldText.Append('`');
                fieldText.Append(i);
                fieldText.Append("` - ");
                fieldText.Append(' ');
                fieldText.Append(channel.Mention);
                fieldText.Append('\n');

                _channels[i] = key;

                i++;
            }

            builder.AddField(LocalizationGroup.GetText("Field", "Channels"), fieldText.ToString());

            return builder;
        }

        /// <summary>
        /// Converting the response message
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>Result</returns>
        public override async Task<List<DiscordMember>> ConvertMessage(DiscordMessage message)
        {
            var members = new List<DiscordMember>();

            if (int.TryParse(message.Content, out var index)
             && _channels.TryGetValue(index, out var selected))
            {
                foreach (var entry in CommandContext.Guild.VoiceStates.Where(obj => obj.Value.Channel?.Id == selected
                                                                                    && obj.Value.User != null))
                {
                    members.Add(await CommandContext.Guild
                                                    .GetMemberAsync(entry.Value.User.Id)
                                                    .ConfigureAwait(false));
                }
            }

            return members;
        }

        #endregion // DialogEmbedMessageElementBase
    }
}