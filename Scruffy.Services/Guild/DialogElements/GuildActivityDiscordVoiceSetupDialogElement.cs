using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Enumerations.General;
using Scruffy.Data.Enumerations.Guild;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;
using Scruffy.Services.Guild.DialogElements.Forms;

namespace Scruffy.Services.Guild.DialogElements;

/// <summary>
/// Discord voice activity point configuration
/// </summary>
public class GuildActivityDiscordVoiceSetupDialogElement : DialogEmbedReactionElementBase<bool>
{
    #region Fields

    /// <summary>
    /// Reactions
    /// </summary>
    private List<ReactionData<bool>> _reactions;

    /// <summary>
    /// Special ranks
    /// </summary>
    private List<(string Mention, double Points)> _roles;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public GuildActivityDiscordVoiceSetupDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Return the configured roles
    /// </summary>
    /// <returns>Roles</returns>
    private List<(string Mention, double Points)> GetRoles()
    {
        if (_roles == null)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var roles = dbFactory.GetRepository<GuildDiscordActivityPointsAssignmentRepository>()
                                     .GetQuery()
                                     .Where(obj => obj.Guild.DiscordServerId == CommandContext.Guild.Id
                                                && obj.Type == DiscordActivityPointsType.Voice)
                                     .Select(obj => new
                                                    {
                                                        obj.RoleId,
                                                        obj.Points
                                                    })
                                     .ToList();

                _roles = roles.Select(obj => new
                                             {
                                                 Role = CommandContext.Guild.Roles.FirstOrDefault(obj2 => obj2.Id  == obj.RoleId),
                                                 obj.Points
                                             })
                              .Where(obj => obj.Role != null)
                              .OrderByDescending(obj => obj.Role.Position)
                              .Select(obj => (obj.Role.Mention, obj.Points))
                              .ToList();
            }
        }

        return _roles;
    }

    #endregion // Methods

    #region DialogEmbedReactionElementBase<bool>

    /// <inheritdoc/>
    protected override string GetCommandTitle() => LocalizationGroup.GetText("CommandTitle", "Commands");

    /// <inheritdoc/>
    public override Task EditMessage(EmbedBuilder builder)
    {
        builder.WithTitle(LocalizationGroup.GetText("ChooseCommandTitle", "Discord voice activity configuration"));
        builder.WithDescription(LocalizationGroup.GetText("ChooseCommandDescription", "With this assistant you are able to configure the discord voice activity roles. The following special ranks are already created:"));

        var levelsBuilder = new StringBuilder();

        var roles = GetRoles();

        if (roles.Count > 0)
        {
            foreach (var (mention, points) in roles)
            {
                levelsBuilder.AppendLine(Format.Bold($"{mention} ({points.ToString("0.##", LocalizationGroup.CultureInfo)})"));
            }
        }
        else
        {
            levelsBuilder.Append('\u200B');
        }

        builder.AddField(LocalizationGroup.GetText("RolesFields", "Roles"), levelsBuilder.ToString());

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override IReadOnlyList<ReactionData<bool>> GetReactions()
    {
        if (_reactions == null)
        {
            _reactions = [
                             new ReactionData<bool>
                             {
                                 Emote = DiscordEmoteService.GetAddEmote(CommandContext.Client),
                                 CommandText = LocalizationGroup.GetFormattedText("AddCommand", "{0} Add role", DiscordEmoteService.GetAddEmote(CommandContext.Client)),
                                 Func = async () =>
                                        {
                                            var data = await DialogHandler.RunForm<GuildActivityDiscordVoiceAddFormData>(CommandContext, false)
                                                                          .ConfigureAwait(false);

                                            using (var dbFactory = RepositoryFactory.CreateInstance())
                                            {
                                                if (dbFactory.GetRepository<GuildDiscordActivityPointsAssignmentRepository>()
                                                             .AddOrRefresh(obj => obj.Guild.DiscordServerId == CommandContext.Guild.Id
                                                                               && obj.Type == DiscordActivityPointsType.Voice
                                                                               && obj.RoleId == data.RoleId,
                                                                           obj =>
                                                                           {
                                                                               if (obj.GuildId == default)
                                                                               {
                                                                                   obj.GuildId = dbFactory.GetRepository<GuildRepository>()
                                                                                                          .GetQuery()
                                                                                                          .Where(obj2 => obj2.DiscordServerId == CommandContext.Guild.Id)
                                                                                                          .Select(obj2 => obj2.Id)
                                                                                                          .First();
                                                                                   obj.Type = DiscordActivityPointsType.Voice;
                                                                                   obj.RoleId = data.RoleId;
                                                                               }

                                                                               obj.Points = data.Points;
                                                                           })
                                                 == false)
                                                {
                                                    LoggingService.AddInteractionLogEntry(LogEntryLevel.Error,
                                                                                          CommandContext.CustomId,
                                                                                          nameof(GuildActivityDiscordVoiceAddFormData),
                                                                                          null,
                                                                                          dbFactory.LastError);
                                                }
                                            }

                                            return true;
                                        }
                             }
                         ];

            if (GetRoles().Count > 0)
            {
                _reactions.Add(new ReactionData<bool>
                               {
                                   Emote = DiscordEmoteService.GetEditEmote(CommandContext.Client),
                                   CommandText = LocalizationGroup.GetFormattedText("EditCommand", "{0} Edit role", DiscordEmoteService.GetEditEmote(CommandContext.Client)),
                                   Func = async () =>
                                          {
                                              var data = await DialogHandler.RunForm<GuildActivityDiscordVoiceEditFormData>(CommandContext, false)
                                                                            .ConfigureAwait(false);

                                              using (var dbFactory = RepositoryFactory.CreateInstance())
                                              {
                                                  if (dbFactory.GetRepository<GuildDiscordActivityPointsAssignmentRepository>()
                                                               .AddOrRefresh(obj => obj.Guild.DiscordServerId == CommandContext.Guild.Id
                                                                                 && obj.Type == DiscordActivityPointsType.Voice
                                                                                 && obj.RoleId == data.RoleId,
                                                                             obj =>
                                                                             {
                                                                                 if (obj.GuildId == default)
                                                                                 {
                                                                                     obj.GuildId = dbFactory.GetRepository<GuildRepository>()
                                                                                                            .GetQuery()
                                                                                                            .Where(obj2 => obj2.DiscordServerId == CommandContext.Guild.Id)
                                                                                                            .Select(obj2 => obj2.Id)
                                                                                                            .First();
                                                                                     obj.Type = DiscordActivityPointsType.Voice;
                                                                                     obj.RoleId = data.RoleId;
                                                                                 }

                                                                                 obj.Points = data.Points;
                                                                             })
                                                   == false)
                                                  {
                                                      LoggingService.AddInteractionLogEntry(LogEntryLevel.Error,
                                                                                            CommandContext.CustomId,
                                                                                            nameof(GuildActivityDiscordVoiceEditFormData),
                                                                                            null,
                                                                                            dbFactory.LastError);
                                                  }
                                              }

                                              return true;
                                          }
                               });

                _reactions.Add(new ReactionData<bool>
                               {
                                   Emote = DiscordEmoteService.GetTrashCanEmote(CommandContext.Client),
                                   CommandText = LocalizationGroup.GetFormattedText("DeleteCommand", "{0} Delete role", DiscordEmoteService.GetTrashCanEmote(CommandContext.Client)),
                                   Func = async () =>
                                          {
                                              var roleId = await RunSubElement<GuildActivityDiscordVoiceRemoveDialogElement, ulong>()
                                                               .ConfigureAwait(false);

                                              using (var dbFactory = RepositoryFactory.CreateInstance())
                                              {
                                                  if (dbFactory.GetRepository<GuildDiscordActivityPointsAssignmentRepository>()
                                                               .Remove(obj => obj.Guild.DiscordServerId == CommandContext.Guild.Id && obj.Type == DiscordActivityPointsType.Voice && obj.RoleId == roleId)
                                                   == false)
                                                  {
                                                      LoggingService.AddInteractionLogEntry(LogEntryLevel.Error,
                                                                                            CommandContext.CustomId,
                                                                                            nameof(GuildActivityDiscordVoicePointsDialogElement),
                                                                                            null,
                                                                                            dbFactory.LastError);
                                                  }
                                              }

                                              return true;
                                          }
                               });
            }

            _reactions.Add(new ReactionData<bool>
                           {
                               Emote = DiscordEmoteService.GetCrossEmote(CommandContext.Client),
                               CommandText = LocalizationGroup.GetFormattedText("CancelCommand", "{0} Cancel", DiscordEmoteService.GetCrossEmote(CommandContext.Client)),
                               Func = () => Task.FromResult(false)
                           });
        }

        return _reactions;
    }

    /// <inheritdoc/>
    protected override bool DefaultFunc() => false;

    #endregion // DialogEmbedReactionElementBase<bool>
}