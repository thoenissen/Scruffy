﻿using Discord;

using Scruffy.Data.Enumerations.Calendar;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Calendar.DialogElements;

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
    public override Task EditMessage(EmbedBuilder builder)
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
                                  new()
                                  {
                                      Emote = Emoji.Parse(":one:"),
                                      CommandText = $"{Emoji.Parse(":one:")} {LocalizationGroup.GetText(WeekDayOfMonthSpecialOptions.None.ToString(), "No options")}",
                                      Func = () => Task.FromResult(WeekDayOfMonthSpecialOptions.None)
                                  },
                                  new()
                                  {
                                      Emote = Emoji.Parse(":two:"),
                                      CommandText = $"{Emoji.Parse(":two:")} {LocalizationGroup.GetText(WeekDayOfMonthSpecialOptions.EvenMonth.ToString(), "Even month")}",
                                      Func = () => Task.FromResult(WeekDayOfMonthSpecialOptions.EvenMonth)
                                  },
                                  new()
                                  {
                                      Emote = Emoji.Parse(":three:"),
                                      CommandText = $"{Emoji.Parse(":three:")} {LocalizationGroup.GetText(WeekDayOfMonthSpecialOptions.UnevenMonth.ToString(), "Uneven month")}",
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