using Discord;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Raid;

/// <summary>
/// Raid line up service
/// </summary>
public class RaidLineUpService : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// Repository factory
    /// </summary>
    private readonly RepositoryFactory _repositoryFactory;

    /// <summary>
    /// Raid role service
    /// </summary>
    private readonly RaidRolesService _raidRoleService;

    /// <summary>
    /// Discord client
    /// </summary>
    private readonly IDiscordClient _discordClient;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="repositoryFactory">Repository factory</param>
    /// <param name="raidRolesService">Raid role service</param>
    /// <param name="discordClient">Discord client</param>
    public RaidLineUpService(LocalizationService localizationService,
                             RepositoryFactory repositoryFactory,
                             RaidRolesService raidRolesService,
                             IDiscordClient discordClient)
        : base(localizationService)
    {
        _repositoryFactory = repositoryFactory;
        _raidRoleService = raidRolesService;
        _discordClient = discordClient;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Post the line up of the given appointment
    /// </summary>
    /// <param name="appointmentId">Id of the appointment</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task<bool> PostLineUp(long appointmentId)
    {
        var success = false;

        var discordAccountQuery = _repositoryFactory.GetRepository<DiscordAccountRepository>()
                                                    .GetQuery()
                                                    .Select(obj => obj);

        var data = await _repositoryFactory.GetRepository<RaidAppointmentRepository>()
                                           .GetQuery()
                                           .Where(obj => obj.Id == appointmentId)
                                           .Select(obj => new
                                                          {
                                                              obj.RaidDayConfiguration.DiscordChannelId,
                                                              obj.TimeStamp,
                                                              Registrations = obj.RaidRegistrations
                                                                                 .Where(obj2 => obj2.Group > 0
                                                                                             && obj2.LineUpRoleId != null)
                                                                                 .Select(obj2 => new
                                                                                                 {
                                                                                                     obj2.Group,
                                                                                                     obj2.LineUpRole,
                                                                                                     DiscordAccountId = discordAccountQuery.Where(obj3 => obj3.UserId == obj2.UserId)
                                                                                                                                           .Select(obj3 => (ulong?)obj3.Id)
                                                                                                                                           .FirstOrDefault()
                                                                                                 })
                                                                                 .ToList()
                                                          })
                                           .FirstOrDefaultAsync()
                                           .ConfigureAwait(false);

        if (data != null
         && data.Registrations.Count > 0)
        {
            if (await _discordClient.GetChannelAsync(data.DiscordChannelId)
                                    .ConfigureAwait(false) is ITextChannel channel)
            {
                foreach (var group in data.Registrations
                                          .GroupBy(obj => obj.Group)
                                          .OrderBy(obj => obj.Key))
                {
                    var embedBuilder = new EmbedBuilder().WithTitle(LocalizationGroup.GetFormattedText("LineUpTitle", "Raid line up - Group {0}", group.Key))
                                                         .WithDescription(LocalizationGroup.GetFormattedText("LineUpDescription", "Appointment: `{0:G}`", data.TimeStamp))
                                                         .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                                                         .WithColor(Color.Green)
                                                         .WithTimestamp(DateTime.Now);

                    var support = new StringBuilder();
                    var dps = new StringBuilder();

                    foreach (var user in group.OrderByDescending(obj => obj.LineUpRole.IsProvidingQuickness)
                                              .ThenByDescending(obj => obj.LineUpRole.IsHealer)
                                              .ThenByDescending(obj => obj.LineUpRole.IsProvidingAlacrity)
                                              .ThenByDescending(obj => obj.LineUpRole.IsHealer))
                    {
                        var isSupport = user.LineUpRole.IsProvidingAlacrity
                                     || user.LineUpRole.IsProvidingQuickness
                                     || user.LineUpRole.IsHealer
                                     || user.LineUpRole.IsTank;

                        var line = _raidRoleService.GetDescriptionAsEmoji(user.LineUpRole, isSupport);

                        line += ' ';

                        var mention = "invalid user";

                        if (user.DiscordAccountId != null)
                        {
                            var discordUser = await _discordClient.GetUserAsync(user.DiscordAccountId.Value)
                                                                  .ConfigureAwait(false);

                            mention = discordUser.Mention;
                        }

                        line += mention;

                        if (isSupport)
                        {
                            support.AppendLine(line);
                        }
                        else
                        {
                            dps.AppendLine(line);
                        }
                    }

                    support.AppendLine("\u200b");
                    dps.AppendLine("\u200b");

                    embedBuilder.AddField(LocalizationGroup.GetFormattedText("SupportTitle", "Support", group.Key), support.ToString(), true);
                    embedBuilder.AddField(LocalizationGroup.GetFormattedText("DpsTitle", "DPS", group.Key), dps.ToString(), true);

                    await channel.SendMessageAsync(embed: embedBuilder.Build())
                                 .ConfigureAwait(false);
                }

                success = true;
            }
        }

        return success;
    }

    #endregion // Methods
}