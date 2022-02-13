using Discord;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Services.Raid;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;
using Scruffy.Services.Discord;
using Scruffy.Services.Raid.DialogElements;

namespace Scruffy.Services.Raid;

/// <summary>
/// Committing a raid appointment
/// </summary>
public class RaidCommitService : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// Localization Service
    /// </summary>
    private LocalizationService _localizationService;

    /// <summary>
    /// Message builder
    /// </summary>
    private RaidMessageBuilder _messageBuilder;

    /// <summary>
    /// User management service
    /// </summary>
    private UserManagementService _userManagementService;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="userManagementService">User management service</param>
    /// <param name="messageBuilder">Message builder</param>
    public RaidCommitService(LocalizationService localizationService, UserManagementService userManagementService, RaidMessageBuilder messageBuilder)
        : base(localizationService)
    {
        _localizationService = localizationService;
        _userManagementService = userManagementService;
        _messageBuilder = messageBuilder;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Commit a raid appointment
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <param name="aliasName">Alias name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task CommitRaidAppointment(CommandContextContainer commandContext, string aliasName)
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var now = DateTime.Now;

            var appointment = await dbFactory.GetRepository<RaidAppointmentRepository>()
                                             .GetQuery()
                                             .Where(obj => obj.TimeStamp < now
                                                        && obj.IsCommitted == false
                                                        && obj.RaidDayConfiguration.AliasName == aliasName)
                                             .Select(obj => new
                                                            {
                                                                obj.Id,
                                                                obj.ConfigurationId,
                                                                obj.TimeStamp
                                                            })
                                             .FirstOrDefaultAsync()
                                             .ConfigureAwait(false);

            if (appointment?.Id > 0)
            {
                var users = new List<RaidCommitUserData>();

                var experienceLevels = dbFactory.GetRepository<RaidExperienceLevelRepository>()
                                                .GetQuery()
                                                .Select(obj => new
                                                               {
                                                                   obj.Id,
                                                                   obj.DiscordEmoji,
                                                                   obj.Rank,
                                                                   obj.ParticipationPoints
                                                               })
                                                .ToList();

                var fallbackExperienceLevel = experienceLevels.OrderByDescending(obj => obj.Rank)
                                                              .First();

                foreach (var entry in dbFactory.GetRepository<RaidRegistrationRepository>()
                                               .GetQuery()
                                               .Where(obj => obj.AppointmentId == appointment.Id)
                                               .Select(obj => new
                                                              {
                                                                  UserId = obj.User
                                                                              .DiscordAccounts
                                                                              .Select(obj2 => obj2.Id)
                                                                              .FirstOrDefault(),
                                                                  obj.User.RaidExperienceLevelId,
                                                                  obj.LineupExperienceLevelId
                                                              })
                                               .ToList())
                {
                    var experienceLevel = experienceLevels.FirstOrDefault(obj => obj.Id == entry.RaidExperienceLevelId) ?? fallbackExperienceLevel;

                    users.Add(new RaidCommitUserData
                              {
                                  DiscordUserId = entry.UserId,
                                  Points = experienceLevel.ParticipationPoints * (entry.LineupExperienceLevelId == null ? 3.0 : 1.0),
                                  DiscordEmoji = experienceLevel.DiscordEmoji
                              });
                }

                var container = new RaidCommitContainer
                                {
                                    AppointmentId = appointment.Id,
                                    AppointmentTimeStamp = appointment.TimeStamp,
                                    Users = users,
                                };

                var dialogHandler = new DialogHandler(commandContext);
                await using (dialogHandler.ConfigureAwait(false))
                {
                    while (await dialogHandler.Run<RaidCommitDialogElement, bool>(new RaidCommitDialogElement(_localizationService, _userManagementService, container)).ConfigureAwait(false))
                    {
                    }

                    await commandContext.Channel
                                        .SendMessageAsync(LocalizationGroup.GetText("CommitCompleted", "The raid appointment has been committed."))
                                        .ConfigureAwait(false);

                    await _messageBuilder.RefreshMessageAsync(appointment.ConfigurationId)
                                         .ConfigureAwait(false);
                }
            }
            else
            {
                await commandContext.Message
                                    .ReplyAsync(LocalizationGroup.GetText("NoOpenAppointment", "There is no uncommitted appointment available."))
                                    .ConfigureAwait(false);
            }
        }
    }

    #endregion // Methods

}