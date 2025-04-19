using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.Rest;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.WebApp.Components.Controls.Raid;
using Scruffy.WebApp.DTOs.Raid;

namespace Scruffy.WebApp.Components.Pages.Raid;

/// <summary>
/// Line up
/// </summary>
[Authorize(Roles = "Administrator")]
public partial class RaidLineUpPage
{
    #region Constants

    /// <summary>
    /// Alacrity emote
    /// </summary>
    private const string AlacrityEmote = "<:a:948901575517151232>";

    /// <summary>
    /// Quickness emote
    /// </summary>
    private const string QuicknessEmote = "<:q:948901318213394442>";

    /// <summary>
    /// Alacrity or Quickness emote
    /// </summary>
    private const string AlacrityOrQuicknessEmote = "<:o:948901318213394442>";

    #endregion // Constants

    #region Fields

    /// <summary>
    /// Time stamp of the appointment
    /// </summary>
    private DateTime? _timeStamp;

    /// <summary>
    /// Number of groups
    /// </summary>
    private int _groupCount;

    /// <summary>
    /// Discord channel ID
    /// </summary>
    private ulong? _channelId;

    /// <summary>
    /// Thumbnail
    /// </summary>
    private string _thumbnailUrl;

    /// <summary>
    /// Registrations
    /// </summary>
    private List<PlayerDTO> _registrations;

    /// <summary>
    /// Squads
    /// </summary>
    private Dictionary<int, RaidSquadComponent> _squads;

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Discord client
    /// </summary>
    [Inject]
    private DiscordRestClient DiscordClient { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Append the healer to the group
    /// </summary>
    /// <param name="groupBuilder">Group builder</param>
    /// <param name="selectedHealer">Healer</param>
    /// <param name="isTank">Is the healer also tank?</param>
    /// <param name="selectedSupport">Support</param>
    private static void AppendHealer(StringBuilder groupBuilder, PlayerRoleDTO selectedHealer, bool isTank, PlayerRoleDTO selectedSupport)
    {
        groupBuilder.Append(isTank ? "<:t:744104738534064159>" : "<:e:1151815722905895007>");
        groupBuilder.Append("<:h:744104722797166694>");

        if (selectedHealer != null)
        {
            if (selectedHealer.Role.HasFlag(RaidRole.AlacrityTankHealer)
                || selectedHealer.Role.HasFlag(RaidRole.AlacrityHealer))
            {
                groupBuilder.Append(AlacrityEmote);
            }
            else if (selectedHealer.Role.HasFlag(RaidRole.QuicknessTankHealer)
                     || selectedHealer.Role.HasFlag(RaidRole.QuicknessHealer))
            {
                groupBuilder.Append(QuicknessEmote);
            }
            else
            {
                groupBuilder.Append(AlacrityOrQuicknessEmote);
            }

            groupBuilder.Append("<@");
            groupBuilder.Append(selectedHealer.Player.DiscordAccountId);
            groupBuilder.Append('>');
        }
        else if (selectedSupport != null)
        {
            if (selectedSupport.Role.HasFlag(RaidRole.AlacrityDamageDealer))
            {
                groupBuilder.Append(QuicknessEmote);
            }
            else if (selectedSupport.Role.HasFlag(RaidRole.QuicknessDamageDealer))
            {
                groupBuilder.Append(AlacrityEmote);
            }
            else
            {
                groupBuilder.Append(AlacrityOrQuicknessEmote);
            }
        }
        else
        {
            groupBuilder.Append(AlacrityOrQuicknessEmote);
        }

        groupBuilder.AppendLine();
    }

    /// <summary>
    /// Append the support to the group
    /// </summary>
    /// <param name="groupBuilder">Group builder</param>
    /// <param name="selectedHealer">Healer</param>
    /// <param name="selectedSupport">Support</param>
    private static void AppendSupport(StringBuilder groupBuilder, PlayerRoleDTO selectedHealer, PlayerRoleDTO selectedSupport)
    {
        groupBuilder.Append("<:e:1151815722905895007><:d:744104683081171004>");

        if (selectedSupport != null)
        {
            if (selectedSupport.Role.HasFlag(RaidRole.AlacrityDamageDealer))
            {
                groupBuilder.Append(AlacrityEmote);
            }
            else if (selectedSupport.Role.HasFlag(RaidRole.QuicknessDamageDealer))
            {
                groupBuilder.Append(QuicknessEmote);
            }
            else
            {
                groupBuilder.Append(AlacrityOrQuicknessEmote);
            }

            groupBuilder.Append("<@");
            groupBuilder.Append(selectedSupport.Player.DiscordAccountId);
            groupBuilder.Append('>');
        }
        else if (selectedHealer != null)
        {
            if (selectedHealer.Role.HasFlag(RaidRole.AlacrityTankHealer)
                || selectedHealer.Role.HasFlag(RaidRole.AlacrityHealer))
            {
                groupBuilder.Append(QuicknessEmote);
            }
            else if (selectedHealer.Role.HasFlag(RaidRole.QuicknessTankHealer)
                     || selectedHealer.Role.HasFlag(RaidRole.QuicknessHealer))
            {
                groupBuilder.Append(AlacrityEmote);
            }
            else
            {
                groupBuilder.Append(AlacrityOrQuicknessEmote);
            }

            groupBuilder.Append("<@");
            groupBuilder.Append(selectedHealer.Player.DiscordAccountId);
            groupBuilder.Append('>');
        }
        else
        {
            groupBuilder.Append(AlacrityOrQuicknessEmote);
        }

        groupBuilder.AppendLine();
    }

    /// <summary>
    /// Append the DPS to the group
    /// </summary>
    /// <param name="groupBuilder">Group builder</param>
    /// <param name="selectedPlayer">DPS</param>
    private static void AppendDps(StringBuilder groupBuilder, PlayerRoleDTO selectedPlayer)
    {
        groupBuilder.Append("<:e:1151815722905895007><:e:1151815722905895007><:dps:744104683081171004>");

        if (selectedPlayer != null)
        {
            groupBuilder.Append("<@");
            groupBuilder.Append(selectedPlayer.Player.DiscordAccountId);
            groupBuilder.Append('>');
        }

        groupBuilder.AppendLine();
    }

    /// <summary>
    /// Called when the squad assignment has changed
    /// </summary>
    private void OnSquadAssignmentChanged()
    {
        StateHasChanged();
    }

    /// <summary>
    /// Commit line up
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task OnCommit()
    {
        if (_timeStamp == null || _channelId == null)
        {
            return;
        }

        if (await DiscordClient.GetChannelAsync(_channelId.Value).ConfigureAwait(false) is ITextChannel textChannel)
        {
            foreach (var squad in _squads)
            {
                await SendSquadMessage(textChannel, squad).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Send the squad message to the text channel
    /// </summary>
    /// <param name="textChannel">Text channel</param>
    /// <param name="squad">Squad</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task SendSquadMessage(ITextChannel textChannel, KeyValuePair<int, RaidSquadComponent> squad)
    {
        var embed = new EmbedBuilder().WithTitle(LocalizationGroup.GetFormattedText("SquadTitle", "Line up - {0} - {1}", LocalizationGroup.CultureInfo.DateTimeFormat.GetDayName(_timeStamp!.Value.DayOfWeek), _timeStamp.Value.ToString("f", LocalizationGroup.CultureInfo)))
                                      .WithDescription(LocalizationGroup.GetFormattedText("SquadDescription", "Line up for squad {0}", squad.Key + 1))
                                      .WithThumbnailUrl(_thumbnailUrl)
                                      .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                                      .WithColor(Color.Green)
                                      .WithTimestamp(DateTime.Now);

        AddGroup(embed,
                 1,
                 squad.Value.SelectedTankRole,
                 true,
                 squad.Value.SelectedDpsSupport1,
                 squad.Value.SelectedDps1,
                 squad.Value.SelectedDps2,
                 squad.Value.SelectedDps3);
        AddGroup(embed,
                 2,
                 squad.Value.SelectedHealerRole,
                 false,
                 squad.Value.SelectedDpsSupport2,
                 squad.Value.SelectedDps4,
                 squad.Value.SelectedDps5,
                 squad.Value.SelectedDps6);

        await textChannel.SendMessageAsync(embed: embed.Build())
                         .ConfigureAwait(false);
    }

    /// <summary>
    /// Add a group to the embed
    /// </summary>
    /// <param name="embed">Embed builder</param>
    /// <param name="groupNumber">Group number</param>
    /// <param name="selectedHealer">Healer</param>
    /// <param name="isTank">Is the healer also tank?</param>
    /// <param name="selectedSupport">Support</param>
    /// <param name="selectedDps1">DPS 1</param>
    /// <param name="selectedDps2">DPS 2</param>
    /// <param name="selectedDps3">DPS 3</param>
    private void AddGroup(EmbedBuilder embed, int groupNumber, PlayerRoleDTO selectedHealer, bool isTank, PlayerRoleDTO selectedSupport, PlayerRoleDTO selectedDps1, PlayerRoleDTO selectedDps2, PlayerRoleDTO selectedDps3)
    {
        embed.AddField(field =>
                       {
                           field.WithName(LocalizationGroup.GetFormattedText("Group", "Group {0}", groupNumber))
                                .WithIsInline(true);

                           var groupBuilder = new StringBuilder();

                           AppendHealer(groupBuilder, selectedHealer, isTank, selectedSupport);
                           AppendSupport(groupBuilder, selectedHealer, selectedSupport);
                           AppendDps(groupBuilder, selectedDps1);
                           AppendDps(groupBuilder, selectedDps2);
                           AppendDps(groupBuilder, selectedDps3);

                           field.WithValue(groupBuilder.ToString());
                       });
    }

    #endregion // Methods

    #region ComponentBase

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();

        _squads = new Dictionary<int, RaidSquadComponent>();

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var appointment = dbFactory.GetRepository<RaidAppointmentRepository>()
                                       .GetQuery()
                                       .Where(appointment => appointment.IsCommitted == false
                                                              && appointment.TimeStamp > DateTime.Now)
                                       .Select(obj => new
                                               {
                                                   obj.Id,
                                                   obj.TimeStamp,
                                                   obj.GroupCount,
                                                   ChannelId = obj.RaidDayConfiguration.DiscordChannelId,
                                                   ThumbnailUrl = obj.RaidDayTemplate.Thumbnail
                                               })
                                       .OrderBy(appointment => appointment.TimeStamp)
                                       .FirstOrDefault();

            _timeStamp = appointment?.TimeStamp;
            _groupCount = appointment?.GroupCount ?? 0;
            _channelId = appointment?.ChannelId;
            _thumbnailUrl = appointment?.ThumbnailUrl;

            _registrations = dbFactory.GetRepository<RaidRegistrationRepository>()
                                      .GetQuery()
                                      .Where(registration => registration.LineupExperienceLevelId != null
                                                             && registration.AppointmentId == appointment.Id)
                                      .Select(registration => new
                                                              {
                                                                  Id = registration.UserId,
                                                                  Name = registration.User.DiscordAccounts.Select(account => account.Members.Where(member => member.ServerId == WebAppConfiguration.DiscordServerId)
                                                                                                                                            .Select(member => member.Name)
                                                                                                                                            .FirstOrDefault())
                                                                                                          .FirstOrDefault(),
                                                                  DiscordAccountId = registration.User.DiscordAccounts.Select(account => account.Id).FirstOrDefault(),
                                                                  Roles = registration.User.RaidUserRoles.Select(userRole => userRole.RoleId)
                                                                                                         .ToList(),
                                                                  RegistrationRoles = registration.RaidRegistrationRoleAssignments
                                                                                                  .Select(role => role.RoleId)
                                                                                                  .ToList()
                                                              })
                                      .AsEnumerable()
                                      .Select(registration => new PlayerDTO
                                                              {
                                                                  Id = registration.Id,
                                                                  Name = registration.Name,
                                                                  DiscordAccountId = registration.DiscordAccountId,
                                                                  Roles = registration.Roles.Count > 0
                                                                          || registration.RegistrationRoles.Count > 0
                                                                                 ? registration.Roles.Concat(registration.RegistrationRoles)
                                                                                                     .Aggregate(RaidRole.None,
                                                                                                                (current, role) => current
                                                                                                                                   | role switch
                                                                                                                                     {
                                                                                                                                         1 => RaidRole.DamageDealer,
                                                                                                                                         2 => RaidRole.AlacrityDamageDealer,
                                                                                                                                         3 => RaidRole.QuicknessDamageDealer,
                                                                                                                                         4 => RaidRole.AlacrityHealer,
                                                                                                                                         5 => RaidRole.QuicknessHealer,
                                                                                                                                         8 => RaidRole.AlacrityTankHealer,
                                                                                                                                         9 => RaidRole.QuicknessTankHealer,
                                                                                                                                         _ => current
                                                                                                                                     })
                                                                                 : RaidRole.DamageDealer,
                                                                  RegistrationRoles = registration.RegistrationRoles.Aggregate(RaidRole.None,
                                                                                                                               (current, role) => current
                                                                                                                                                  | role switch
                                                                                                                                                  {
                                                                                                                                                      1 => RaidRole.DamageDealer,
                                                                                                                                                      2 => RaidRole.AlacrityDamageDealer,
                                                                                                                                                      3 => RaidRole.QuicknessDamageDealer,
                                                                                                                                                      4 => RaidRole.AlacrityHealer,
                                                                                                                                                      5 => RaidRole.QuicknessHealer,
                                                                                                                                                      8 => RaidRole.AlacrityTankHealer,
                                                                                                                                                      9 => RaidRole.QuicknessTankHealer,
                                                                                                                                                      _ => current
                                                                                                                                                  })
                                                              })
                                      .ToList();
        }
    }

    #endregion // ComponentBase
}