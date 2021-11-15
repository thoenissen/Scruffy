using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Entity.Tables.Raid;
using Scruffy.Data.Services.Raid;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;
using Scruffy.Services.Raid.DialogElements.Forms;

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
    /// Reactions
    /// </summary>
    private List<ReactionData<bool>> _reactions;

    /// <summary>
    /// Commit data
    /// </summary>
    private RaidCommitContainer _commitData;

    /// <summary>
    /// Localization service
    /// </summary>
    private LocalizationService _localizationService;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="commitData">Commit data</param>
    /// <param name="userManagementService">User management service</param>
    /// <param name="localizationService">Localization service</param>
    public RaidCommitDialogElement(LocalizationService localizationService, UserManagementService userManagementService, RaidCommitContainer commitData)
        : base(localizationService)
    {
        _localizationService = localizationService;
        _userManagementService = userManagementService;
        _commitData = commitData;
    }

    #endregion // Constructor

    #region DialogEmbedReactionElementBase<bool>

    /// <summary>
    /// Editing the embedded message
    /// </summary>
    /// <param name="builder">Builder</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override async Task EditMessage(DiscordEmbedBuilder builder)
    {
        builder.WithTitle(LocalizationGroup.GetText("CommitTitle", "Raid points commit"));
        builder.WithDescription(LocalizationGroup.GetText("CommitText", "The following points will be committed:"));
        builder.WithColor(DiscordColor.Green);
        builder.WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/ef1f3e1f3f40100fb3750f8d7d25c657.png?size=64");
        builder.WithTimestamp(DateTime.Now);

        var message = new StringBuilder();

        var fieldCounter = 1;

        foreach (var user in _commitData.Users
                                        .OrderByDescending(obj => obj.Points))
        {
            var discordUser = await CommandContext.Client
                                                  .GetUserAsync(user.DiscordUserId)
                                                  .ConfigureAwait(false);

            var currentLine = $"{Formatter.InlineCode(user.Points.ToString("0.0"))} - {DiscordEmojiService.GetGuildEmoji(CommandContext.Client, user.DiscordEmoji)} {discordUser.Mention}";
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

    /// <summary>
    /// Returns the reactions which should be added to the message
    /// </summary>
    /// <returns>Reactions</returns>
    public override IReadOnlyList<ReactionData<bool>> GetReactions()
    {
        return _reactions ??= new List<ReactionData<bool>>
                              {
                                  new ()
                                  {
                                      Emoji = DiscordEmojiService.GetAddEmoji(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("AddCommand", "{0} Add user", DiscordEmojiService.GetAddEmoji(CommandContext.Client)),
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
                                                     _commitData.Users
                                                                .Add(new RaidCommitUserData
                                                                     {
                                                                         Points = data.Points,
                                                                         DiscordUserId = data.User.Id
                                                                     });
                                                 }

                                                 return true;
                                             }
                                  },
                                  new ()
                                  {
                                      Emoji = DiscordEmojiService.GetEditEmoji(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("SetPointsCommand", "{0} Set points", DiscordEmojiService.GetEditEmoji(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var data = await RunSubForm<RaidCommitUserFormData>().ConfigureAwait(false);

                                                 var user = _commitData.Users
                                                                       .FirstOrDefault(obj => obj.DiscordUserId == data.User.Id);

                                                 if (user != null)
                                                 {
                                                     user.Points = data.Points;
                                                 }

                                                 return true;
                                             }
                                  },
                                  new ()
                                  {
                                      Emoji = DiscordEmojiService.GetTrashCanEmoji(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("RemoveCommand", "{0} Remove user", DiscordEmojiService.GetTrashCanEmoji(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 var discordUser = await RunSubElement<RaidCommitRemoveUserDialogElement, DiscordUser>(new RaidCommitRemoveUserDialogElement(_localizationService)).ConfigureAwait(false);

                                                 var user = _commitData.Users
                                                                       .FirstOrDefault(obj => obj.DiscordUserId == discordUser.Id);

                                                 if (user != null)
                                                 {
                                                     _commitData.Users.Remove(user);
                                                 }

                                                 return true;
                                             }
                                  },
                                  new ()
                                  {
                                      Emoji = DiscordEmojiService.GetCheckEmoji(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("CommitCommand", "{0} Commit", DiscordEmojiService.GetCheckEmoji(CommandContext.Client)),
                                      Func = async () =>
                                             {
                                                 using (var dbFactory = RepositoryFactory.CreateInstance())
                                                 {
                                                     foreach (var commitUser in _commitData.Users)
                                                     {
                                                         var user = await _userManagementService.GetUserByDiscordAccountId(commitUser.DiscordUserId)
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
                                                                       });

                                                     dbFactory.GetRepository<RaidAppointmentRepository>()
                                                              .Add(nextAppointment);
                                                 }

                                                 return false;
                                             }
                                  },
                                  new ()
                                  {
                                      Emoji = DiscordEmojiService.GetCrossEmoji(CommandContext.Client),
                                      CommandText = LocalizationGroup.GetFormattedText("CancelCommand", "{0} Cancel", DiscordEmojiService.GetCrossEmoji(CommandContext.Client)),
                                      Func = () => Task.FromResult(false)
                                  },
                              };
    }

    /// <summary>
    /// Returns the title of the commands
    /// </summary>
    /// <returns>Commands</returns>
    protected override string GetCommandTitle() => LocalizationGroup.GetText("CommandTitle", "Commands");

    /// <summary>
    /// Default case if none of the given reactions is used
    /// </summary>
    /// <returns>Result</returns>
    protected override bool DefaultFunc() => false;

    #endregion // DialogEmbedReactionElementBase<bool>
}