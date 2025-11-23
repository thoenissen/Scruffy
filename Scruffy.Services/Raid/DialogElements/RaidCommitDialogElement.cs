using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Entity.Tables.Raid;
using Scruffy.Data.Services.Raid;
using Scruffy.Services.Core.JobScheduler;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;
using Scruffy.Services.Discord;
using Scruffy.Services.Raid.DialogElements.Forms;
using Scruffy.Services.Raid.Jobs;

namespace Scruffy.Services.Raid.DialogElements;

/// <summary>
/// Committing the raid appointment
/// </summary>
public class RaidCommitDialogElement : DialogEmbedReactionElementBase<bool>
{
    #region Fields

    /// <summary>
    /// User management service
    /// </summary>
    private readonly UserManagementService _userManagementService;

    /// <summary>
    /// Job scheduler
    /// </summary>
    private readonly JobScheduler _jobScheduler;

    /// <summary>
    /// Commit data
    /// </summary>
    private readonly RaidCommitContainer _commitData;

    /// <summary>
    /// Localization service
    /// </summary>
    private readonly LocalizationService _localizationService;

    /// <summary>
    /// Reactions
    /// </summary>
    private List<ReactionData<bool>> _reactions;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="commitData">Commit data</param>
    /// <param name="userManagementService">User management service</param>
    /// <param name="localizationService">Localization service</param>
    /// <param name="jobScheduler">Job scheduler</param>
    public RaidCommitDialogElement(LocalizationService localizationService, UserManagementService userManagementService, RaidCommitContainer commitData, JobScheduler jobScheduler)
        : base(localizationService)
    {
        _localizationService = localizationService;
        _userManagementService = userManagementService;
        _commitData = commitData;
        _jobScheduler = jobScheduler;
    }

    #endregion // Constructor

    #region DialogEmbedReactionElementBase<bool>

    /// <inheritdoc/>
    public override async Task EditMessage(EmbedBuilder builder)
    {
        builder.WithTitle(LocalizationGroup.GetText("CommitTitle", "Raid points commit"));
        builder.WithDescription(LocalizationGroup.GetText("CommitText", "The following points will be committed:"));
        builder.WithColor(Color.Green);
        builder.WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64");
        builder.WithTimestamp(DateTime.Now);

        var message = new StringBuilder();

        var fieldCounter = 1;

        foreach (var user in _commitData.Users
                                        .OrderByDescending(obj => obj.Points))
        {
            var discordUser = await CommandContext.Client
                                                  .GetUserAsync(user.DiscordUserId)
                                                  .ConfigureAwait(false);

            var currentLine = $"{Format.Code(user.Points.ToString("0.0"))} - {DiscordEmoteService.GetGuildEmote(CommandContext.Client, user.DiscordEmoji)} {discordUser.Mention}";

            if (currentLine.Length + message.Length > 1024)
            {
                builder.AddField(LocalizationGroup.GetText("Users", "Users") + " #" + fieldCounter, message.ToString());
                fieldCounter++;

                message = new StringBuilder();
            }

            message.AppendLine(currentLine);
        }

        message.AppendLine("\u200b");

        var fieldName = LocalizationGroup.GetText("Users", "Users");

        if (fieldCounter > 1)
        {
            fieldName = fieldName + " #" + fieldCounter;
        }

        builder.AddField(fieldName, message.ToString());
    }

    /// <inheritdoc/>
    public override IReadOnlyList<ReactionData<bool>> GetReactions()
    {
        return _reactions ??= [
                                  new ReactionData<bool>
                                  {
                                      Emote = DiscordEmoteService.GetAddEmote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("AddCommand", "{0} Add user", DiscordEmoteService.GetAddEmote(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var data = await RunSubForm<RaidCommitUserFormData>().ConfigureAwait(false);

                                                 var user = _commitData.Users
                                                                       .FirstOrDefault(obj => obj.DiscordUserId == data.User.Id);

                                                 if (user != null)
                                                 {
                                                     user.Points = data.Points;
                                                 }
                                                 else
                                                 {
                                                     using (var dbFactory = RepositoryFactory.CreateInstance())
                                                     {
                                                         _commitData.Users.Add(new RaidCommitUserData
                                                                               {
                                                                                   Points = data.Points,
                                                                                   DiscordUserId = data.User.Id,
                                                                                   DiscordEmoji = dbFactory.GetRepository<DiscordAccountRepository>()
                                                                                                           .GetQuery()
                                                                                                           .Where(account => account.Id == data.User.Id)
                                                                                                           .Select(account => account.User.RaidExperienceLevel.DiscordEmoji)
                                                                                                           .FirstOrDefault()
                                                                               });
                                                     }
                                                 }

                                                 return true;
                                             }
                                  },
                                  new ReactionData<bool>
                                  {
                                      Emote = DiscordEmoteService.GetEditEmote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("SetPointsCommand", "{0} Set points", DiscordEmoteService.GetEditEmote(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var data = await RunSubForm<RaidCommitUserFormData>().ConfigureAwait(false);

                                                 var user = _commitData.Users
                                                                       .FirstOrDefault(obj => obj.DiscordUserId == data.User.Id);

                                                 user?.Points = data.Points;

                                                 return true;
                                             }
                                  },
                                  new ReactionData<bool>
                                  {
                                      Emote = DiscordEmoteService.GetTrashCanEmote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("RemoveCommand", "{0} Remove user", DiscordEmoteService.GetTrashCanEmote(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var discordUser = await RunSubElement<RaidCommitRemoveUserDialogElement, IUser>(new RaidCommitRemoveUserDialogElement(_localizationService)).ConfigureAwait(false);

                                                 var user = _commitData.Users
                                                                       .FirstOrDefault(obj => obj.DiscordUserId == discordUser.Id);

                                                 if (user != null)
                                                 {
                                                     _commitData.Users.Remove(user);
                                                 }

                                                 return true;
                                             }
                                  },
                                  new ReactionData<bool>
                                  {
                                      Emote = DiscordEmoteService.GetCheckEmote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("CommitCommand", "{0} Commit", DiscordEmoteService.GetCheckEmote(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 using (var dbFactory = RepositoryFactory.CreateInstance())
                                                 {
                                                     foreach (var commitUser in _commitData.Users)
                                                     {
                                                         var discordUser = await CommandContext.Client
                                                                                               .GetUserAsync(commitUser.DiscordUserId)
                                                                                               .ConfigureAwait(false);

                                                         var user = await _userManagementService.GetUserByDiscordAccountId(discordUser)
                                                                                                .ConfigureAwait(false);

                                                         dbFactory.GetRepository<RaidRegistrationRepository>()
                                                                  .AddOrRefresh(obj => obj.AppointmentId == _commitData.AppointmentId
                                                                                    && obj.UserId == user.Id,
                                                                                obj =>
                                                                                {
                                                                                    if (obj.Id == 0)
                                                                                    {
                                                                                        obj.AppointmentId = _commitData.AppointmentId;
                                                                                        obj.RegistrationTimeStamp = DateTime.Now;
                                                                                        obj.UserId = user.Id;
                                                                                    }

                                                                                    obj.Points = commitUser.Points;
                                                                                });
                                                     }

                                                     var dateLimit = _commitData.AppointmentTimeStamp.AddDays(-7 * 15);

                                                     var users = dbFactory.GetRepository<RaidRegistrationRepository>()
                                                                          .GetQuery()
                                                                          .Where(obj => obj.Points != null
                                                                                     && obj.RaidAppointment.TimeStamp > dateLimit)
                                                                          .Select(obj => new
                                                                                         {
                                                                                             obj.UserId,
                                                                                             obj.RaidAppointment.TimeStamp,
                                                                                             obj.Points
                                                                                         })
                                                                          .AsEnumerable()
                                                                          .GroupBy(obj => obj.UserId)
                                                                          .Select(obj => new
                                                                                         {
                                                                                             UserId = obj.Key,
                                                                                             Points = obj.Select(obj2 => new
                                                                                                                         {
                                                                                                                             obj2.TimeStamp,
                                                                                                                             obj2.Points
                                                                                                                         })
                                                                                         })
                                                                          .ToList();

                                                     dbFactory.GetRepository<RaidCurrentUserPointsRepository>()
                                                              .RefreshRange(obj => true,
                                                                            obj =>
                                                                            {
                                                                                var user = users.FirstOrDefault(obj2 => obj2.UserId == obj.UserId);

                                                                                if (user != null)
                                                                                {
                                                                                    obj.Points = user.Points.Sum(obj2 =>
                                                                                                                 {
                                                                                                                     var points = 0.0;

                                                                                                                     if (obj2.Points != null)
                                                                                                                     {
                                                                                                                         var weekCount = (_commitData.AppointmentTimeStamp - obj2.TimeStamp).Days / 7;

                                                                                                                         points = Math.Pow(10, -(weekCount - 15) / 14.6) * obj2.Points.Value;
                                                                                                                     }

                                                                                                                     return points;
                                                                                                                 })
                                                                                               / 66.147532745646117;

                                                                                    users.Remove(user);
                                                                                }
                                                                                else
                                                                                {
                                                                                    obj.Points = 0;
                                                                                }
                                                                            });

                                                     foreach (var user in users)
                                                     {
                                                         dbFactory.GetRepository<RaidCurrentUserPointsRepository>()
                                                                  .Add(new RaidCurrentUserPointsEntity
                                                                       {
                                                                           UserId = user.UserId,
                                                                           Points = user.Points.Sum(obj2 =>
                                                                                                    {
                                                                                                        var points = 0.0;

                                                                                                        if (obj2.Points != null)
                                                                                                        {
                                                                                                            var weekCount = (_commitData.AppointmentTimeStamp - obj2.TimeStamp).Days / 7;

                                                                                                            points = Math.Pow(10, -(weekCount - 15) / 14.6) * obj2.Points.Value;
                                                                                                        }

                                                                                                        return points;
                                                                                                    })
                                                                                  / 66.147532745646117
                                                                       });
                                                     }

                                                     dbFactory.GetRepository<RaidCurrentUserPointsRepository>()
                                                              .RemoveRange(obj => obj.Points <= 0.0);

                                                     var nextAppointment = new RaidAppointmentEntity();

                                                     dbFactory.GetRepository<RaidAppointmentRepository>()
                                                              .Refresh(obj => obj.Id == _commitData.AppointmentId,
                                                                       obj =>
                                                                       {
                                                                           obj.IsCommitted = true;

                                                                           nextAppointment.ConfigurationId = obj.ConfigurationId;
                                                                           nextAppointment.TemplateId = obj.TemplateId;
                                                                           nextAppointment.TimeStamp = obj.TimeStamp.AddDays(7);
                                                                           nextAppointment.Deadline = obj.Deadline.AddDays(7);
                                                                           nextAppointment.GroupCount = 1;
                                                                       });

                                                     dbFactory.GetRepository<RaidAppointmentRepository>()
                                                              .Add(nextAppointment);

                                                     _jobScheduler.AddJob(new RaidMessageRefreshJob(nextAppointment.ConfigurationId), nextAppointment.Deadline);
                                                     _jobScheduler.AddJob(new RaidMessageRefreshJob(nextAppointment.ConfigurationId), nextAppointment.TimeStamp);
                                                 }

                                                 return false;
                                             }
                                  },
                                  new ReactionData<bool>
                                  {
                                      Emote = DiscordEmoteService.GetCrossEmote(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("CancelCommand", "{0} Cancel", DiscordEmoteService.GetCrossEmote(CommandContext.Client)),
                                      Func = () => Task.FromResult(false)
                                  },
                              ];
    }

    /// <inheritdoc/>
    protected override string GetCommandTitle() => LocalizationGroup.GetText("CommandTitle", "Commands");

    /// <inheritdoc/>
    protected override bool DefaultFunc() => false;

    #endregion // DialogEmbedReactionElementBase<bool>
}