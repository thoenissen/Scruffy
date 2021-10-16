using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus.Entities;

using Scruffy.Data.Enumerations.Calendar;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Calendar.DialogElements
{
    /// <summary>
    /// Acquisition of the special options of the schedule
    /// </summary>
    public class CalendarScheduleOptionsDialogElement : DialogEmbedReactionElementBase<WeekDayOfMonthSpecialOptions>
    {
        #region Fields

        /// <summary>
        /// Reactions
        /// </summary>
        private List<ReactionData<WeekDayOfMonthSpecialOptions>> _reactions;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public CalendarScheduleOptionsDialogElement(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region DialogReactionElementBase<WeekDayOfMonthSpecialOptions>

        /// <summary>
        /// Editing the embedded message
        /// </summary>
        /// <param name="builder">Builder</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override Task EditMessage(DiscordEmbedBuilder builder)
        {
            builder.WithTitle(LocalizationGroup.GetText("Title", "Schedule options selection"));
            builder.WithDescription(LocalizationGroup.GetText("Description", "Please choose one of the following options:"));

            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns the title of the commands
        /// </summary>
        /// <returns>Commands</returns>
        protected override string GetCommandTitle()
        {
            return LocalizationGroup.GetText("Reactions", "Days");
        }

        /// <summary>
        /// Returns the reactions which should be added to the message
        /// </summary>
        /// <returns>Reactions</returns>
        public override IReadOnlyList<ReactionData<WeekDayOfMonthSpecialOptions>> GetReactions()
        {
            return _reactions ??= new List<ReactionData<WeekDayOfMonthSpecialOptions>>
                                  {
                                      new ()
                                      {
                                          Emoji = DiscordEmoji.FromName(CommandContext.Client, ":one:"),
                                          CommandText = $"{DiscordEmoji.FromName(CommandContext.Client, ":one:")} {LocalizationGroup.GetText(WeekDayOfMonthSpecialOptions.None.ToString(), "No options")}",
                                          Func = () => Task.FromResult(WeekDayOfMonthSpecialOptions.None)
                                      },
                                      new ()
                                      {
                                          Emoji = DiscordEmoji.FromName(CommandContext.Client, ":two:"),
                                          CommandText = $"{DiscordEmoji.FromName(CommandContext.Client, ":two:")} {LocalizationGroup.GetText(WeekDayOfMonthSpecialOptions.EvenMonth.ToString(), "Even month")}",
                                          Func = () => Task.FromResult(WeekDayOfMonthSpecialOptions.EvenMonth)
                                      },
                                      new ()
                                      {
                                          Emoji = DiscordEmoji.FromName(CommandContext.Client, ":three:"),
                                          CommandText = $"{DiscordEmoji.FromName(CommandContext.Client, ":three:")} {LocalizationGroup.GetText(WeekDayOfMonthSpecialOptions.UnevenMonth.ToString(), "Uneven month")}",
                                          Func = () => Task.FromResult(WeekDayOfMonthSpecialOptions.UnevenMonth)
                                      }
                                  };
        }

        /// <summary>
        /// Default case if none of the given reactions is used
        /// </summary>
        /// <returns>Result</returns>
        protected override WeekDayOfMonthSpecialOptions DefaultFunc()
        {
            throw new InvalidOperationException();
        }

        #endregion DialogReactionElementBase<WeekDayOfMonthSpecialOptions>
    }
}