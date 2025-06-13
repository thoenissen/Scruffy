using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.Rest;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Entity.Tables.Raid;
using Scruffy.Data.Enumerations.Raid;
using Scruffy.Services.Core.Extensions;
using Scruffy.WebApp.Components.Controls.Raid;
using Scruffy.WebApp.DTOs.Raid;

namespace Scruffy.WebApp.Components.Pages.Raid;

/// <summary>
/// Line up
/// </summary>
[Authorize(Roles = "PrivilegedMember")]
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
    /// Appointment ID
    /// </summary>
    private long? _appointmentId;

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
    /// Are there players on the bench?
    /// </summary>
    private bool _isTheSubstitutesBenchAvailable;

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

    /// <summary>
    /// Authentication state provider
    /// </summary>
    [Inject]
    private AuthenticationStateProvider AuthenticationStateProvider { get; set; }

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
        if (_appointmentId == null || _timeStamp == null || _channelId == null)
        {
            return;
        }

        if (await DiscordClient.GetChannelAsync(_channelId.Value).ConfigureAwait(false) is ITextChannel textChannel)
        {
            await DeleteCurrentLineUp(textChannel).ConfigureAwait(false);

            foreach (var squad in _squads)
            {
                await SendSquadMessage(textChannel, squad).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Delete current line up
    /// </summary>
    /// <param name="textChannel">text Channel</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task DeleteCurrentLineUp(ITextChannel textChannel)
    {
        using (var repositoryFactory = new RepositoryFactory())
        {
            var lineUpMessages = repositoryFactory.GetRepository<RaidAppointmentLineUpSquadRepository>()
                                                  .GetQuery()
                                                  .Where(lineUp => lineUp.AppointmentId == _appointmentId)
                                                  .Select(lineUp => lineUp.MessageId)
                                                  .ToList();

            foreach (var lineUpMessage in lineUpMessages)
            {
                try
                {
                    await textChannel.DeleteMessageAsync(lineUpMessage)
                                     .ConfigureAwait(false);
                }
                catch
                {
                }
            }

            repositoryFactory.GetRepository<RaidAppointmentLineUpSquadRepository>()
                             .RemoveRange(lineUp => lineUp.AppointmentId == _appointmentId);
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
        var embed = new EmbedBuilder().WithTitle(LocalizationGroup.GetFormattedText("SquadTitle", "Line up - {0} - {1}", LocalizationGroup.CultureInfo.DateTimeFormat.GetDayName(_timeStamp!.Value.DayOfWeek), _timeStamp.Value.ToString("g", LocalizationGroup.CultureInfo)))
                                      .WithDescription(LocalizationGroup.GetFormattedText("SquadDescription", "Line up for squad {0}", squad.Key + 1))
                                      .WithThumbnailUrl(_thumbnailUrl)
                                      .WithColor(Color.Green)
                                      .WithTimestamp(DateTime.Now);

        var user = await GetCurrentUser().ConfigureAwait(false);

        if (user != null)
        {
            embed.WithFooter(LocalizationGroup.GetFormattedText("CreatedBy", "Created by {0}", user.TryGetDisplayName()), user.GetAvatarUrl());
        }

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

        if (string.IsNullOrWhiteSpace(squad.Value.Remarks) == false)
        {
            embed.AddField(LocalizationGroup.GetText("Remarks", "Remarks"), squad.Value.Remarks);
        }

        var message = await textChannel.SendMessageAsync(embed: embed.Build())
                                       .ConfigureAwait(false);

        SaveLineUp(squad.Key, squad.Value, message.Id);
    }

    /// <summary>
    /// Get current user
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task<IGuildUser> GetCurrentUser()
    {
        IGuildUser user = null;

        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync()
                                                         .ConfigureAwait(false);
        var nameIdentifier = authState.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(nameIdentifier) == false
            && int.TryParse(nameIdentifier, out var userId))
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var discordAccountId = dbFactory.GetRepository<DiscordAccountRepository>()
                                                .GetQuery()
                                                .Where(account => account.UserId == userId)
                                                .Select(account => account.Id)
                                                .FirstOrDefault();

                if (discordAccountId > 0)
                {
                    user = await DiscordClient.GetGuildUserAsync(WebAppConfiguration.DiscordServerId, discordAccountId)
                                              .ConfigureAwait(false);
                }
            }
        }

        return user;
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
                           field.WithName(LocalizationGroup.GetFormattedText("GroupFieldName", "Group {0}", groupNumber))
                                .WithIsInline(false);

                           var groupBuilder = new StringBuilder();

                           AppendHealer(groupBuilder, selectedHealer, isTank, selectedSupport);
                           AppendSupport(groupBuilder, selectedHealer, selectedSupport);
                           AppendDps(groupBuilder, selectedDps1);
                           AppendDps(groupBuilder, selectedDps2);
                           AppendDps(groupBuilder, selectedDps3);

                           field.WithValue(groupBuilder.ToString());
                       });
    }

    /// <summary>
    /// Save the line up
    /// </summary>
    /// <param name="groupNumber">Group number</param>
    /// <param name="squad">Squad</param>
    /// <param name="messageId">Message ID</param>
    private void SaveLineUp(int groupNumber, RaidSquadComponent squad, ulong messageId)
    {
        using (var repositoryFactory = RepositoryFactory.CreateInstance())
        {
            var lineUp = new RaidAppointmentLineUpSquadEntity
                         {
                             AppointmentId = _appointmentId!.Value,
                             GroupNumber = groupNumber,
                             MessageId = messageId,
                             TankUserId = squad.SelectedTankRole?.Player.Id,
                             Support1UserId = squad.SelectedDpsSupport1?.Player.Id,
                             Dps1UserId = squad.SelectedDps1?.Player.Id,
                             Dps2UserId = squad.SelectedDps2?.Player.Id,
                             Dps3UserId = squad.SelectedDps3?.Player.Id,
                             HealerUserId = squad.SelectedHealerRole?.Player.Id,
                             Support2UserId = squad.SelectedDpsSupport2?.Player.Id,
                             Dps4UserId = squad.SelectedDps4?.Player.Id,
                             Dps5UserId = squad.SelectedDps5?.Player.Id,
                             Dps6UserId = squad.SelectedDps6?.Player.Id,
                             Remarks = squad.Remarks
                         };

            if (squad.SelectedTankRole != null)
            {
                lineUp.TankRaidRole = squad.SelectedTankRole.Role;
            }
            else if (squad.SelectedDpsSupport1 != null)
            {
                lineUp.TankRaidRole = squad.SelectedDpsSupport1.Role == RaidRole.AlacrityDamageDealer
                                          ? RaidRole.QuicknessTankHealer
                                          : RaidRole.AlacrityTankHealer;
            }

            if (squad.SelectedHealerRole != null)
            {
                lineUp.HealerRaidRole = squad.SelectedHealerRole.Role;
            }
            else if (squad.SelectedDpsSupport2 != null)
            {
                lineUp.HealerRaidRole = squad.SelectedDpsSupport2.Role == RaidRole.AlacrityDamageDealer
                                            ? RaidRole.QuicknessHealer
                                            : RaidRole.AlacrityHealer;
            }

            repositoryFactory.GetRepository<RaidAppointmentLineUpSquadRepository>()
                             .Add(lineUp);
        }
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

            _appointmentId = appointment?.Id;
            _timeStamp = appointment?.TimeStamp;
            _groupCount = appointment?.GroupCount ?? 0;
            _channelId = appointment?.ChannelId;
            _thumbnailUrl = appointment?.ThumbnailUrl;

            var currentRaidPoints = dbFactory.GetRepository<RaidCurrentUserPointsRepository>()
                                             .GetQuery()
                                             .Select(points => points);

            _registrations = dbFactory.GetRepository<RaidRegistrationRepository>()
                                      .GetQuery()
                                      .Where(registration => registration.AppointmentId == appointment.Id)
                                      .OrderByDescending(registration => registration.RegistrationTimeStamp < registration.RaidAppointment.Deadline
                                                                             ? currentRaidPoints.Where(points => points.UserId == registration.UserId)
                                                                                                .Select(points => points.Points)
                                                                                                .FirstOrDefault()
                                                                             : 0D)
                                      .ThenBy(registration => registration.RegistrationTimeStamp)
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
                                                                                                  .ToList(),
                                                                  IsOnSubstitutesBench = registration.LineupExperienceLevelId == null
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
                                                                                                                                                  }),
                                                                  IsOnSubstitutesBench = registration.IsOnSubstitutesBench
                                                              })
                                      .ToList();

            _isTheSubstitutesBenchAvailable = _registrations.Any(registration => registration.IsOnSubstitutesBench);
        }
    }

    /// <inheritdoc />
    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        if (firstRender)
        {
            using (var repositoryFactory = new RepositoryFactory())
            {
                var lineUps = repositoryFactory.GetRepository<RaidAppointmentLineUpSquadRepository>()
                                               .GetQuery()
                                               .Where(lineUp => lineUp.AppointmentId == _appointmentId)
                                               .ToList();

                foreach (var squad in _squads)
                {
                    var lineUp = lineUps.FirstOrDefault(lineUp => lineUp.GroupNumber == squad.Key);

                    if (lineUp != null)
                    {
                        squad.Value.Initialize(lineUp);
                    }
                }
            }
        }
    }

#endregion // ComponentBase
}