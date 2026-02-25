using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Calendar;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Entity.Tables.Raid;
using Scruffy.Services.Raid;
using Scruffy.WebApp.DTOs.Raid;

namespace Scruffy.WebApp.Components.Pages.Raid;

/// <summary>
/// Raid commit page
/// </summary>
[Authorize(Roles = "PrivilegedMember")]
public partial class RaidCommitPage
{
    #region Fields

    /// <summary>
    /// Appointment ID
    /// </summary>
    private long? _appointmentId;

    /// <summary>
    /// Configuration ID
    /// </summary>
    private long? _configurationId;

    /// <summary>
    /// Timestamp of the appointment
    /// </summary>
    private DateTime? _appointmentTimeStamp;

    /// <summary>
    /// Commit users
    /// </summary>
    private List<RaidCommitUserDTO> _users;

    /// <summary>
    /// Is the commit completed?
    /// </summary>
    private bool _isCommitted;

    /// <summary>
    /// Error message
    /// </summary>
    private string _errorMessage;

    /// <summary>
    /// Is the commit in progress?
    /// </summary>
    private bool _isCommitting;

    /// <summary>
    /// Is the add-player overlay visible?
    /// </summary>
    private bool _isAddPlayerOverlayVisible;

    /// <summary>
    /// Search filter for the add-player overlay
    /// </summary>
    private string _addPlayerSearchFilter = string.Empty;

    /// <summary>
    /// All available guild members for the picker
    /// </summary>
    private List<RaidCommitUserDTO> _allGuildMembers;

    /// <summary>
    /// Reference to the search input element
    /// </summary>
    private ElementReference _searchInput;

    /// <summary>
    /// Should the search input be focused on next render?
    /// </summary>
    private bool _focusSearchInput;

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Message builder
    /// </summary>
    [Inject]
    private RaidMessageBuilder MessageBuilder { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Handle status change for a user
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="e">Change event args</param>
    private static void OnStatusChanged(RaidCommitUserDTO user, ChangeEventArgs e)
    {
        user.Status = (string)e.Value switch
                      {
                          "substitute" => RaidParticipationStatus.Substitute,
                          "noshow" => RaidParticipationStatus.NoShow,
                          _ => RaidParticipationStatus.Played,
                      };
    }

    /// <summary>
    /// Remove a user from the commit list
    /// </summary>
    /// <param name="user">User to remove</param>
    private void OnRemoveUser(RaidCommitUserDTO user)
    {
        _users?.Remove(user);
    }

    /// <summary>
    /// Open the add-player overlay
    /// </summary>
    private void OnOpenAddPlayerOverlay()
    {
        _addPlayerSearchFilter = string.Empty;
        _isAddPlayerOverlayVisible = true;
        _focusSearchInput = true;
    }

    /// <summary>
    /// Close the add-player overlay
    /// </summary>
    private void OnCloseAddPlayerOverlay()
    {
        _isAddPlayerOverlayVisible = false;
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

    /// <summary>
    /// Add a guild member to the commit list
    /// </summary>
    /// <param name="member">Guild member to add</param>
    private void OnAddPlayer(RaidCommitUserDTO member)
    {
        if (_users?.Any(u => u.UserId == member.UserId) == true)
        {
            return;
        }

        _users?.Add(new RaidCommitUserDTO
                    {
                        UserId = member.UserId,
                        DiscordAccountId = member.DiscordAccountId,
                        Name = member.Name,
                        ParticipationPoints = member.ParticipationPoints,
                        Status = RaidParticipationStatus.Substitute,
                        ExperienceLevelDescription = member.ExperienceLevelDescription
                    });

        _isAddPlayerOverlayVisible = false;
    }

    /// <summary>
    /// Get the filtered guild members for the overlay
    /// </summary>
    /// <returns>Filtered guild members not yet in the commit list</returns>
    private IEnumerable<RaidCommitUserDTO> GetFilteredGuildMembers()
    {
        if (_allGuildMembers == null)
        {
            return [];
        }

        var existingUserIds = _users?.Select(u => u.UserId).ToHashSet() ?? [];

        var filtered = _allGuildMembers.Where(m => existingUserIds.Contains(m.UserId) == false);

        if (string.IsNullOrWhiteSpace(_addPlayerSearchFilter) == false)
        {
            filtered = filtered.Where(m => m.Name != null && m.Name.Contains(_addPlayerSearchFilter, StringComparison.OrdinalIgnoreCase));
        }

        return filtered;
    }

    /// <summary>
    /// Commit the raid appointment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task OnCommitAsync()
    {
        if (_appointmentId == null || _appointmentTimeStamp == null || _isCommitting)
        {
            return;
        }

        _isCommitting = true;
        _errorMessage = null;

        try
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                foreach (var commitUser in _users)
                {
                    dbFactory.GetRepository<RaidRegistrationRepository>()
                             .AddOrRefresh(obj => obj.AppointmentId == _appointmentId.Value
                                                  && obj.UserId == commitUser.UserId,
                                           obj =>
                                           {
                                               if (obj.Id == 0)
                                               {
                                                   obj.AppointmentId = _appointmentId.Value;
                                                   obj.RegistrationTimeStamp = DateTime.Now;
                                                   obj.UserId = commitUser.UserId;
                                               }

                                               obj.Points = commitUser.Points;
                                           });
                }

                var dateLimit = _appointmentTimeStamp.Value.AddDays(-7 * 15);

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
                                                                                    var weekCount = (_appointmentTimeStamp.Value - obj2.TimeStamp).Days / 7;

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
                                                                       var weekCount = (_appointmentTimeStamp.Value - obj2.TimeStamp).Days / 7;

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
                         .Refresh(obj => obj.Id == _appointmentId.Value,
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

                var calendarAppointmentId = dbFactory.GetRepository<CalendarAppointmentRepository>()
                                                     .GetQuery()
                                                     .Where(obj => obj.TimeStamp == _appointmentTimeStamp.Value)
                                                     .Select(obj => obj.Id)
                                                     .FirstOrDefault();

                if (calendarAppointmentId > 0)
                {
                    foreach (var userId in dbFactory.GetRepository<RaidRegistrationRepository>()
                                                    .GetQuery()
                                                    .Where(obj => obj.AppointmentId == _appointmentId.Value)
                                                    .Select(obj => obj.UserId)
                                                    .ToList())
                    {
                        dbFactory.GetRepository<CalendarAppointmentParticipantRepository>()
                                 .AddOrRefresh(obj => obj.AppointmentId == calendarAppointmentId
                                                      && obj.UserId == userId,
                                               obj =>
                                               {
                                                   obj.AppointmentId = calendarAppointmentId;
                                                   obj.UserId = userId;
                                               });
                    }
                }

                if (_configurationId != null)
                {
                    await MessageBuilder.RefreshMessageAsync(_configurationId.Value)
                                        .ConfigureAwait(false);
                }
            }

            _isCommitted = true;
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
        }
        finally
        {
            _isCommitting = false;
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
            var now = DateTime.Now;

            var appointment = dbFactory.GetRepository<RaidAppointmentRepository>()
                                       .GetQuery()
                                       .Where(obj => obj.TimeStamp < now
                                                  && obj.IsCommitted == false)
                                       .OrderByDescending(obj => obj.TimeStamp)
                                       .Select(obj => new
                                                      {
                                                          obj.Id,
                                                          obj.ConfigurationId,
                                                          obj.TimeStamp,
                                                          obj.Deadline
                                                      })
                                       .FirstOrDefault();

            if (appointment?.Id > 0)
            {
                _appointmentId = appointment.Id;
                _configurationId = appointment.ConfigurationId;
                _appointmentTimeStamp = appointment.TimeStamp;

                var experienceLevels = dbFactory.GetRepository<RaidExperienceLevelRepository>()
                                                .GetQuery()
                                                .Select(obj => new
                                                               {
                                                                   obj.Id,
                                                                   obj.Description,
                                                                   obj.Rank,
                                                                   obj.ParticipationPoints
                                                               })
                                                .ToList();

                var fallbackExperienceLevel = experienceLevels.OrderByDescending(obj => obj.Rank)
                                                              .First();

                _users = dbFactory.GetRepository<RaidRegistrationRepository>()
                                  .GetQuery()
                                  .Where(obj => obj.AppointmentId == appointment.Id)
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
                                                     obj.User.RaidExperienceLevelId,
                                                     obj.LineupExperienceLevelId
                                                 })
                                  .AsEnumerable()
                                  .Select(entry =>
                                          {
                                              var experienceLevel = experienceLevels.FirstOrDefault(obj => obj.Id == entry.RaidExperienceLevelId)
                                                                        ?? fallbackExperienceLevel;

                                              return new RaidCommitUserDTO
                                                     {
                                                         UserId = entry.UserId,
                                                         DiscordAccountId = entry.DiscordAccountId,
                                                         Name = entry.Name,
                                                         ParticipationPoints = experienceLevel.ParticipationPoints,
                                                         Status = entry.LineupExperienceLevelId is null
                                                                      ? RaidParticipationStatus.Substitute
                                                                      : RaidParticipationStatus.Played,
                                                         ExperienceLevelDescription = experienceLevel.Description
                                                     };
                                          })
                                  .OrderByDescending(obj => obj.Points)
                                  .ToList();

                _allGuildMembers = dbFactory.GetRepository<DiscordServerMemberRepository>()
                                            .GetQuery()
                                            .Where(obj => obj.ServerId == WebAppConfiguration.DiscordServerId)
                                            .Select(obj => new
                                                           {
                                                               obj.AccountId,
                                                               obj.Name,
                                                               obj.Account.UserId,
                                                               obj.Account.User.RaidExperienceLevelId
                                                           })
                                            .AsEnumerable()
                                            .Select(obj =>
                                                    {
                                                        var experienceLevel = experienceLevels.FirstOrDefault(e => e.Id == obj.RaidExperienceLevelId)
                                                                                  ?? fallbackExperienceLevel;

                                                        return new RaidCommitUserDTO
                                                               {
                                                                   UserId = obj.UserId,
                                                                   DiscordAccountId = obj.AccountId,
                                                                   Name = obj.Name,
                                                                   ParticipationPoints = experienceLevel.ParticipationPoints,
                                                                   Status = RaidParticipationStatus.Substitute,
                                                                   ExperienceLevelDescription = experienceLevel.Description
                                                               };
                                                    })
                                            .OrderBy(obj => obj.Name)
                                            .ToList();
            }
        }
    }

    #endregion // ComponentBase
}