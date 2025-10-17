using Discord;

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

    /// <inheritdoc/>
    public override Task EditMessage(EmbedBuilder builder)
    {
        builder.WithTitle(LocalizationGroup.GetText("Title", "Schedule options selection"));
        builder.WithDescription(LocalizationGroup.GetText("Description", "Please choose one of the following options:"));

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    protected override string GetCommandTitle()
    {
        return LocalizationGroup.GetText("Reactions", "Days");
    }

    /// <inheritdoc/>
    public override IReadOnlyList<ReactionData<WeekDayOfMonthSpecialOptions>> GetReactions()
    {
        return _reactions ??= [
                                  new ReactionData<WeekDayOfMonthSpecialOptions>
                                  {
                                      Emote = Emoji.Parse(":one:"),
                                      CommandText = $"{Emoji.Parse(":one:")} {LocalizationGroup.GetText(WeekDayOfMonthSpecialOptions.None.ToString(), "No options")}",
                                      Func = () => Task.FromResult(WeekDayOfMonthSpecialOptions.None)
                                  },
                                  new ReactionData<WeekDayOfMonthSpecialOptions>
                                  {
                                      Emote = Emoji.Parse(":two:"),
                                      CommandText = $"{Emoji.Parse(":two:")} {LocalizationGroup.GetText(WeekDayOfMonthSpecialOptions.EvenMonth.ToString(), "Even month")}",
                                      Func = () => Task.FromResult(WeekDayOfMonthSpecialOptions.EvenMonth)
                                  },
                                  new ReactionData<WeekDayOfMonthSpecialOptions>
                                  {
                                      Emote = Emoji.Parse(":three:"),
                                      CommandText = $"{Emoji.Parse(":three:")} {LocalizationGroup.GetText(WeekDayOfMonthSpecialOptions.UnevenMonth.ToString(), "Uneven month")}",
                                      Func = () => Task.FromResult(WeekDayOfMonthSpecialOptions.UnevenMonth)
                                  },
                                  new ReactionData<WeekDayOfMonthSpecialOptions>
                                  {
                                      Emote = Emoji.Parse(":four:"),
                                      CommandText = $"{Emoji.Parse(":four:")} {LocalizationGroup.GetText(WeekDayOfMonthSpecialOptions.MonthSelection.ToString(), "Month selection")}",
                                      Func = () => Task.FromResult(WeekDayOfMonthSpecialOptions.MonthSelection)
                                  }
                              ];
    }

    /// <inheritdoc/>
    protected override WeekDayOfMonthSpecialOptions DefaultFunc()
    {
        throw new InvalidOperationException();
    }

    #endregion // DialogReactionElementBase<WeekDayOfMonthSpecialOptions>
}