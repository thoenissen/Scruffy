using System;
using System.Collections.Generic;

using Scruffy.WebApp.DTOs.Raid;

namespace Scruffy.WebApp.Components.Pages.Raid.Data;

/// <summary>
/// Appointments of a user
/// </summary>
public class UserRaidAppointmentsData
{
    /// <summary>
    /// Appointments
    /// </summary>
    public List<DateTime> Appointments { get; init; }

    /// <summary>
    /// Roles
    /// </summary>
    public RaidRole Roles { get; init; }

    /// <summary>
    /// Special roles
    /// </summary>
    public RaidSpecialRole SpecialRoles { get; init; }
}