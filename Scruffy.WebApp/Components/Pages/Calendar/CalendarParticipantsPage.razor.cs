using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Discord.WebSocket;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Calendar;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Entity.Tables.Calendar;
using Scruffy.WebApp.DTOs.Calendar;

namespace Scruffy.WebApp.Components.Pages.Calendar;

/// <summary>
/// Calendar participants management page
/// </summary>
[Authorize(Roles = "PrivilegedMember")]
public partial class CalendarParticipantsPage
{
    #region Fields

    /// <summary>
    /// Available appointments for selection
    /// </summary>
    private List<(long Id, string Description)> _appointments;

    /// <summary>
    /// Selected appointment ID
    /// </summary>
    private long? _selectedAppointmentId;

    /// <summary>
    /// Current participants of the selected appointment
    /// </summary>
    private List<CalendarParticipantDTO> _participants;

    /// <summary>
    /// All guild members for the add-member overlay
    /// </summary>
    private List<CalendarParticipantDTO> _allGuildMembers;

    /// <summary>
    /// Is the add-member overlay visible?
    /// </summary>
    private bool _isAddMemberOverlayVisible;

    /// <summary>
    /// Is the voice channel overlay visible?
    /// </summary>
    private bool _isVoiceChannelOverlayVisible;

    /// <summary>
    /// Search filter for the add-member overlay
    /// </summary>
    private string _addMemberSearchFilter = string.Empty;

    /// <summary>
    /// Reference to the search input element
    /// </summary>
    private ElementReference _searchInput;

    /// <summary>
    /// Should the search input be focused on next render?
    /// </summary>
    private bool _focusSearchInput;

    /// <summary>
    /// Active voice channels with connected members
    /// </summary>
    private List<VoiceChannelDTO> _voiceChannels;

    /// <summary>
    /// Is the commit completed?
    /// </summary>
    private bool _isCommitted;

    /// <summary>
    /// Is the commit in progress?
    /// </summary>
    private bool _isCommitting;

    /// <summary>
    /// Error message
    /// </summary>
    private string _errorMessage;

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Discord socket client for voice channel access
    /// </summary>
    [Inject]
    private DiscordSocketClient DiscordSocketClient { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Handle appointment selection change
    /// </summary>
    /// <param name="e">Change event args</param>
    private void OnAppointmentChanged(ChangeEventArgs e)
    {
        if (long.TryParse(e.Value?.ToString(), out var appointmentId))
        {
            _selectedAppointmentId = appointmentId;
            _isCommitted = false;
            _errorMessage = null;

            LoadParticipants(appointmentId);
        }
        else
        {
            _selectedAppointmentId = null;
            _participants = null;
        }
    }

    /// <summary>
    /// Load participants for the selected appointment
    /// </summary>
    /// <param name="appointmentId">Appointment ID</param>
    private void LoadParticipants(long appointmentId)
    {
        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            _participants = dbFactory.GetRepository<CalendarAppointmentParticipantRepository>()
                                     .GetQuery()
                                     .Where(obj => obj.AppointmentId == appointmentId)
                                     .Select(obj => new
                                                    {
                                                        obj.UserId,
                                                        DiscordAccountId = obj.User
                                                                              .DiscordAccounts
                                                                              .Select(obj2 => obj2.Id)
                                                                              .FirstOrDefault(),
                                                        Name = obj.User.DiscordAccounts
                                                                       .Select(account => account.Members
                                                                                                  .Where(member => member.ServerId == WebAppConfiguration.DiscordServerId)
                                                                                                  .Select(member => member.Name)
                                                                                                  .FirstOrDefault())
                                                                       .FirstOrDefault()
                                                                   ?? obj.User.UserName,
                                                        obj.IsLeader
                                                    })
                                     .AsEnumerable()
                                     .Select(entry => new CalendarParticipantDTO
                                                      {
                                                          UserId = entry.UserId,
                                                          DiscordAccountId = entry.DiscordAccountId,
                                                          Name = entry.Name,
                                                          IsLeader = entry.IsLeader
                                                      })
                                     .OrderBy(obj => obj.Name)
                                     .ToList();

            if (_participants.Count == 0)
            {
                var leaderId = dbFactory.GetRepository<CalendarAppointmentRepository>()
                                        .GetQuery()
                                        .Where(obj => obj.Id == appointmentId && obj.LeaderId != null)
                                        .Select(obj => new
                                                       {
                                                           obj.Leader.Id,
                                                           DiscordAccountId = obj.Leader
                                                                                 .DiscordAccounts
                                                                                 .Select(obj2 => obj2.Id)
                                                                                 .FirstOrDefault(),
                                                           Name = obj.Leader.DiscordAccounts
                                                                            .Select(account => account.Members
                                                                                                       .Where(member => member.ServerId == WebAppConfiguration.DiscordServerId)
                                                                                                       .Select(member => member.Name)
                                                                                                       .FirstOrDefault())
                                                                            .FirstOrDefault()
                                                                      ?? obj.Leader.UserName
                                                       })
                                        .FirstOrDefault();

                if (leaderId != null)
                {
                    _participants.Add(new CalendarParticipantDTO
                                      {
                                          UserId = leaderId.Id,
                                          DiscordAccountId = leaderId.DiscordAccountId,
                                          Name = leaderId.Name,
                                          IsLeader = true
                                      });
                }
            }
        }
    }

    /// <summary>
    /// Remove a participant from the list
    /// </summary>
    /// <param name="participant">Participant to remove</param>
    private void OnRemoveParticipant(CalendarParticipantDTO participant)
    {
        _participants?.Remove(participant);
    }

    /// <summary>
    /// Toggle the leader flag of a participant
    /// </summary>
    /// <param name="participant">Participant to toggle</param>
    private void OnToggleLeader(CalendarParticipantDTO participant)
    {
        participant.IsLeader = participant.IsLeader == false;
    }

    /// <summary>
    /// Open the add-member overlay
    /// </summary>
    private void OnOpenAddMemberOverlay()
    {
        _addMemberSearchFilter = string.Empty;
        _isAddMemberOverlayVisible = true;
        _focusSearchInput = true;
    }

    /// <summary>
    /// Close the add-member overlay
    /// </summary>
    private void OnCloseAddMemberOverlay()
    {
        _isAddMemberOverlayVisible = false;
    }

    /// <summary>
    /// Open the voice channel overlay
    /// </summary>
    private void OnOpenVoiceChannelOverlay()
    {
        LoadVoiceChannels();
        _isVoiceChannelOverlayVisible = true;
    }

    /// <summary>
    /// Close the voice channel overlay
    /// </summary>
    private void OnCloseVoiceChannelOverlay()
    {
        _isVoiceChannelOverlayVisible = false;
    }

    /// <summary>
    /// Load active voice channels with their connected members
    /// </summary>
    private void LoadVoiceChannels()
    {
        _voiceChannels = [];

        var guild = DiscordSocketClient.GetGuild(WebAppConfiguration.DiscordServerId);

        if (guild == null)
        {
            return;
        }

        foreach (var voiceChannel in guild.VoiceChannels)
        {
            var connectedUsers = voiceChannel.ConnectedUsers;

            if (connectedUsers.Count == 0)
            {
                continue;
            }

            _voiceChannels.Add(new VoiceChannelDTO
                               {
                                   ChannelId = voiceChannel.Id,
                                   Name = voiceChannel.Name,
                                   Members = connectedUsers.Select(user => new VoiceChannelMemberDTO
                                                                           {
                                                                               DiscordAccountId = user.Id,
                                                                               Name = user.DisplayName
                                                                           })
                                                           .OrderBy(m => m.Name)
                                                           .ToList()
                               });
        }

        _voiceChannels = _voiceChannels.OrderBy(c => c.Name).ToList();
    }

    /// <summary>
    /// Add all members from a voice channel to the participants list
    /// </summary>
    /// <param name="channel">Voice channel</param>
    private void OnAddVoiceChannel(VoiceChannelDTO channel)
    {
        foreach (var member in channel.Members)
        {
            AddMemberByDiscordId(member.DiscordAccountId, member.Name);
        }

        _isVoiceChannelOverlayVisible = false;
    }

    /// <summary>
    /// Add a single member from a voice channel to the participants list
    /// </summary>
    /// <param name="member">Voice channel member</param>
    private void OnAddVoiceChannelMember(VoiceChannelMemberDTO member)
    {
        AddMemberByDiscordId(member.DiscordAccountId, member.Name);
    }

    /// <summary>
    /// Add a member to the participants list by Discord account ID
    /// </summary>
    /// <param name="discordAccountId">Discord account ID</param>
    /// <param name="displayName">Display name</param>
    private void AddMemberByDiscordId(ulong discordAccountId, string displayName)
    {
        if (_participants?.Any(p => p.DiscordAccountId == discordAccountId) == true)
        {
            return;
        }

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var userData = dbFactory.GetRepository<DiscordAccountRepository>()
                                    .GetQuery()
                                    .Where(obj => obj.Id == discordAccountId)
                                    .Select(obj => new
                                                   {
                                                       obj.UserId,
                                                       Name = obj.Members
                                                                 .Where(m => m.ServerId == WebAppConfiguration.DiscordServerId)
                                                                 .Select(m => m.Name)
                                                                 .FirstOrDefault()
                                                                  ?? obj.User.UserName
                                                   })
                                    .FirstOrDefault();

            if (userData == null)
            {
                return;
            }

            _participants?.Add(new CalendarParticipantDTO
                               {
                                   UserId = userData.UserId,
                                   DiscordAccountId = discordAccountId,
                                   Name = userData.Name ?? displayName,
                                   IsLeader = false
                               });
        }
    }

    /// <summary>
    /// Add a guild member to the participants list
    /// </summary>
    /// <param name="member">Guild member to add</param>
    private void OnAddMember(CalendarParticipantDTO member)
    {
        if (_participants?.Any(p => p.UserId == member.UserId) == true)
        {
            return;
        }

        _participants?.Add(new CalendarParticipantDTO
                           {
                               UserId = member.UserId,
                               DiscordAccountId = member.DiscordAccountId,
                               Name = member.Name,
                               IsLeader = false
                           });

        _isAddMemberOverlayVisible = false;
    }

    /// <summary>
    /// Get the filtered guild members for the overlay
    /// </summary>
    /// <returns>Filtered guild members not yet in the participants list</returns>
    private IEnumerable<CalendarParticipantDTO> GetFilteredGuildMembers()
    {
        if (_allGuildMembers == null)
        {
            return [];
        }

        var existingUserIds = _participants?.Select(p => p.UserId).ToHashSet() ?? [];
        var filtered = _allGuildMembers.Where(m => existingUserIds.Contains(m.UserId) == false);

        if (string.IsNullOrWhiteSpace(_addMemberSearchFilter) == false)
        {
            filtered = filtered.Where(m => m.Name != null
                                           && m.Name.Contains(_addMemberSearchFilter, StringComparison.OrdinalIgnoreCase));
        }

        return filtered;
    }

    /// <summary>
    /// Commit the participants to the database
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private Task OnCommitAsync()
    {
        if (_selectedAppointmentId == null || _participants == null || _isCommitting)
        {
            return Task.CompletedTask;
        }

        _isCommitting = true;
        _errorMessage = null;

        try
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                dbFactory.GetRepository<CalendarAppointmentParticipantRepository>()
                         .RemoveRange(obj => obj.AppointmentId == _selectedAppointmentId.Value);

                foreach (var participant in _participants)
                {
                    if (dbFactory.GetRepository<CalendarAppointmentParticipantRepository>()
                                 .Add(new CalendarAppointmentParticipantEntity
                                      {
                                          AppointmentId = _selectedAppointmentId.Value,
                                          UserId = participant.UserId,
                                          IsLeader = participant.IsLeader
                                      }) == false)
                    {
                        throw dbFactory.LastError;
                    }
                }

                _isCommitted = true;
            }
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
        }
        finally
        {
            _isCommitting = false;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_focusSearchInput)
        {
            _focusSearchInput = false;

            await _searchInput.FocusAsync()
                              .ConfigureAwait(false);
        }
    }

    #endregion // Methods

    #region ComponentBase

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var limit = DateTime.Now.AddMinutes(60);

            var raidAppointments = dbFactory.GetRepository<RaidAppointmentRepository>()
                                            .GetQuery()
                                            .Select(appointment => appointment);

            _appointments = dbFactory.GetRepository<CalendarAppointmentRepository>()
                                     .GetQuery()
                                     .Where(calendarAppointment => calendarAppointment.TimeStamp < limit
                                                                   && raidAppointments.Any(raidAppointment => raidAppointment.TimeStamp == calendarAppointment.TimeStamp) == false)
                                     .OrderByDescending(calendarAppointment => calendarAppointment.TimeStamp)
                                     .Take(10)
                                     .Select(calendarAppointment => new
                                                                    {
                                                                        calendarAppointment.Id,
                                                                        calendarAppointment.TimeStamp,
                                                                        calendarAppointment.CalendarAppointmentTemplate.Description
                                                                    })
                                     .AsEnumerable()
                                     .Select(calendarAppointment => (calendarAppointment.Id, Description: $"{calendarAppointment.Description} - {calendarAppointment.TimeStamp.ToString("g", LocalizationGroup.CultureInfo)}"))
                                     .ToList();

            _allGuildMembers = dbFactory.GetRepository<DiscordServerMemberRepository>()
                                        .GetQuery()
                                        .Where(member => member.ServerId == WebAppConfiguration.DiscordServerId)
                                        .Select(member => new
                                                       {
                                                           member.AccountId,
                                                           member.Name,
                                                           member.Account.UserId
                                                       })
                                        .AsEnumerable()
                                        .Select(participant => new CalendarParticipantDTO
                                                               {
                                                                   UserId = participant.UserId,
                                                                   DiscordAccountId = participant.AccountId,
                                                                   Name = participant.Name
                                                               })
                                        .OrderBy(participant => participant.Name)
                                        .ToList();
        }
    }

    #endregion // ComponentBase
}