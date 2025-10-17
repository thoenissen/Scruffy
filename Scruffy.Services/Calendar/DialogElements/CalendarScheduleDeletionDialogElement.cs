using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Calendar;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Calendar.DialogElements;

/// <summary>
/// Deletion of a schedule
/// </summary>
public class CalendarScheduleDeletionDialogElement : DialogReactionElementBase<bool>
{
    #region Fields

    /// <summary>
    /// Reactions
    /// </summary>
    private List<ReactionData<bool>> _reactions;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public CalendarScheduleDeletionDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogReactionElementBase<bool>

    /// <inheritdoc/>
    public override string GetMessage() => LocalizationGroup.GetText("DeletePrompt", "Are you sure you want to delete the schedule?");

    /// <inheritdoc/>
    public override IReadOnlyList<ReactionData<bool>> GetReactions()
    {
        return _reactions ??= [
                                  new ReactionData<bool>
                                  {
                                      Emote = DiscordEmoteService.GetCheckEmote(CommandContext.Client),
                                      Func = () =>
                                             {
                                                 using (var dbFactory = RepositoryFactory.CreateInstance())
                                                 {
                                                     var scheduleId = DialogContext.GetValue<long>("CalendarScheduleId");

                                                     if (dbFactory.GetRepository<CalendarAppointmentScheduleRepository>()
                                                                  .Refresh(obj => obj.Id == scheduleId, obj => obj.IsDeleted = true))
                                                     {
                                                         var now = DateTime.Now;

                                                         dbFactory.GetRepository<CalendarAppointmentRepository>()
                                                                  .RemoveRange(obj => obj.CalendarAppointmentScheduleId == scheduleId
                                                                                   && obj.TimeStamp > now);
                                                     }
                                                 }

                                                 return Task.FromResult(true);
                                             }
                                  },
                                  new ReactionData<bool>
                                  {
                                      Emote = DiscordEmoteService.GetCrossEmote(CommandContext.Client),
                                      Func = () => Task.FromResult(true)
                                  }
                              ];
    }

    /// <inheritdoc/>
    protected override bool DefaultFunc(IReaction reaction) => false;

    #endregion // DialogReactionElementBase<bool>
}