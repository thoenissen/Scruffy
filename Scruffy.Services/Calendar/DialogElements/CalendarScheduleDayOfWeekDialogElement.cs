using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus.Entities;

using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;

namespace Scruffy.Services.Calendar.DialogElements
{
    /// <summary>
    /// Acquisition the week day of the schedule
    /// </summary>
    public class CalendarScheduleDayOfWeekDialogElement : DialogEmbedReactionElementBase<DayOfWeek>
    {
        #region Fields

        /// <summary>
        /// Reactions
        /// </summary>
        private List<ReactionData<DayOfWeek>> _reactions;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public CalendarScheduleDayOfWeekDialogElement(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        /// <summary>
        /// Returning task with the result
        /// </summary>
        /// <param name="dayOfWeek">Day of week</param>
        /// <returns>Task Day of week</returns>
        private Task<DayOfWeek> GetReturnValue(DayOfWeek dayOfWeek)
        {
            DialogContext.SetValue("DayOfWeek", dayOfWeek);

            return Task.FromResult(dayOfWeek);
        }

        #region DialogReactionElementBase<DayOfWeek>

        /// <summary>
        /// Editing the embedded message
        /// </summary>
        /// <param name="builder">Builder</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override Task EditMessage(DiscordEmbedBuilder builder)
        {
            builder.WithTitle(LocalizationGroup.GetText("Title", "Schedule selection"));
            builder.WithDescription(LocalizationGroup.GetText("Description", "Please choose the day of the week:"));

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
        public override IReadOnlyList<ReactionData<DayOfWeek>> GetReactions()
        {
            return _reactions ??= new List<ReactionData<DayOfWeek>>
                                  {
                                      new ReactionData<DayOfWeek>
                                      {
                                          Emoji = DiscordEmoji.FromName(CommandContext.Client, ":one:"),
                                          CommandText = $"{DiscordEmoji.FromName(CommandContext.Client, ":one:")} {LocalizationGroup.CultureInfo.DateTimeFormat.GetDayName(DayOfWeek.Monday)}",
                                          Func = () => GetReturnValue(DayOfWeek.Monday)
                                      },
                                      new ReactionData<DayOfWeek>
                                      {
                                          Emoji = DiscordEmoji.FromName(CommandContext.Client, ":two:"),
                                          CommandText = $"{DiscordEmoji.FromName(CommandContext.Client, ":two:")} {LocalizationGroup.CultureInfo.DateTimeFormat.GetDayName(DayOfWeek.Tuesday)}",
                                          Func = () => GetReturnValue(DayOfWeek.Tuesday)
                                      },
                                      new ReactionData<DayOfWeek>
                                      {
                                          Emoji = DiscordEmoji.FromName(CommandContext.Client, ":three:"),
                                          CommandText = $"{DiscordEmoji.FromName(CommandContext.Client, ":three:")} {LocalizationGroup.CultureInfo.DateTimeFormat.GetDayName(DayOfWeek.Wednesday)}",
                                          Func = () => GetReturnValue(DayOfWeek.Wednesday)
                                      },
                                      new ReactionData<DayOfWeek>
                                      {
                                          Emoji = DiscordEmoji.FromName(CommandContext.Client, ":four:"),
                                          CommandText = $"{DiscordEmoji.FromName(CommandContext.Client, ":four:")} {LocalizationGroup.CultureInfo.DateTimeFormat.GetDayName(DayOfWeek.Thursday)}",
                                          Func = () => GetReturnValue(DayOfWeek.Thursday)
                                      },
                                      new ReactionData<DayOfWeek>
                                      {
                                          Emoji = DiscordEmoji.FromName(CommandContext.Client, ":five:"),
                                          CommandText = $"{DiscordEmoji.FromName(CommandContext.Client, ":five:")} {LocalizationGroup.CultureInfo.DateTimeFormat.GetDayName(DayOfWeek.Friday)}",
                                          Func = () => GetReturnValue(DayOfWeek.Friday)
                                      },
                                      new ReactionData<DayOfWeek>
                                      {
                                          Emoji = DiscordEmoji.FromName(CommandContext.Client, ":six:"),
                                          CommandText = $"{DiscordEmoji.FromName(CommandContext.Client, ":six:")} {LocalizationGroup.CultureInfo.DateTimeFormat.GetDayName(DayOfWeek.Saturday)}",
                                          Func = () => GetReturnValue(DayOfWeek.Saturday)
                                      },
                                      new ReactionData<DayOfWeek>
                                      {
                                          Emoji = DiscordEmoji.FromName(CommandContext.Client, ":seven:"),
                                          CommandText = $"{DiscordEmoji.FromName(CommandContext.Client, ":seven:")} {LocalizationGroup.CultureInfo.DateTimeFormat.GetDayName(DayOfWeek.Sunday)}",
                                          Func = () => GetReturnValue(DayOfWeek.Sunday)
                                      },
                                  };
        }

        /// <summary>
        /// Default case if none of the given reactions is used
        /// </summary>
        /// <returns>Result</returns>
        protected override DayOfWeek DefaultFunc()
        {
            throw new InvalidOperationException();
        }

        #endregion DialogReactionElementBase<DayOfWeek>
    }
}