using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Components;

using Scruffy.Data.Entity.Tables.Raid;
using Scruffy.Data.Enumerations.Raid;
using Scruffy.WebApp.DTOs.Raid;

namespace Scruffy.WebApp.Components.Controls.Raid;

/// <summary>
/// Squad
/// </summary>
public partial class RaidSquadComponent
{
    #region Fields

    /// <summary>
    /// Tank roles
    /// </summary>
    private List<PlayerRoleDTO> _tankRoles = [];

    /// <summary>
    /// Healer roles
    /// </summary>
    private List<PlayerRoleDTO> _healerRoles = [];

    /// <summary>
    /// DPS Support roles
    /// </summary>
    private List<PlayerRoleDTO> _dpsSupportRoles = [];

    /// <summary>
    /// DPS roles
    /// </summary>
    private List<PlayerRoleDTO> _dpsRoles = [];

    #endregion // Fields

    #region Properties

    #region Parameter

    /// <summary>
    /// Group number
    /// </summary>
    [Parameter]
    public int GroupNumber { get; set; }

    /// <summary>
    /// Registrations
    /// </summary>
    [Parameter]
    public List<PlayerDTO> Registrations { get; set; }

    /// <summary>
    /// Notification for assignment changes
    /// </summary>
    [Parameter]
    public EventCallback AssignmentChanged { get; set; }

    #endregion // Parameter

    /// <summary>
    /// Selected Tank Role
    /// </summary>
    public PlayerRoleDTO SelectedTankRole
    {
        get;
        set => SetSelectedRole(ref field, value);
    }

    /// <summary>
    /// Selected Healer Role
    /// </summary>
    public PlayerRoleDTO SelectedHealerRole
    {
        get;
        set => SetSelectedRole(ref field, value);
    }

    /// <summary>
    /// Selected DPS Support 1 Role
    /// </summary>
    public PlayerRoleDTO SelectedDpsSupport1
    {
        get;
        set => SetSelectedRole(ref field, value);
    }

    /// <summary>
    /// Selected DPS Support 2 Role
    /// </summary>
    public PlayerRoleDTO SelectedDpsSupport2
    {
        get;
        set => SetSelectedRole(ref field, value);
    }

    /// <summary>
    /// Selected DPS 1 Role
    /// </summary>
    public PlayerRoleDTO SelectedDps1
    {
        get;
        set => SetSelectedRole(ref field, value);
    }

    /// <summary>
    /// Selected DPS 2 Role
    /// </summary>
    public PlayerRoleDTO SelectedDps2
    {
        get;
        set => SetSelectedRole(ref field, value);
    }

    /// <summary>
    /// Selected DPS 3 Role
    /// </summary>
    public PlayerRoleDTO SelectedDps3
    {
        get;
        set => SetSelectedRole(ref field, value);
    }

    /// <summary>
    /// Selected DPS 4 Role
    /// </summary>
    public PlayerRoleDTO SelectedDps4
    {
        get;
        set => SetSelectedRole(ref field, value);
    }

    /// <summary>
    /// Selected DPS 5 Role
    /// </summary>
    public PlayerRoleDTO SelectedDps5
    {
        get;
        set => SetSelectedRole(ref field, value);
    }

    /// <summary>
    /// Selected DPS 6 Role
    /// </summary>
    public PlayerRoleDTO SelectedDps6
    {
        get;
        set => SetSelectedRole(ref field, value);
    }

    /// <summary>
    /// Remarks
    /// </summary>
    public string Remarks { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Initialize
    /// </summary>
    /// <param name="lineUp">Line up</param>
    public void Initialize(RaidAppointmentLineUpSquadEntity lineUp)
    {
        if (lineUp.TankRaidRole == RaidRole.AlacrityTankHealer)
        {
            SelectedTankRole = _tankRoles.FirstOrDefault(entry => entry.Player.Id == lineUp.TankUserId && entry.Role == RaidRole.AlacrityTankHealer);
            SelectedDpsSupport1 = _dpsSupportRoles.FirstOrDefault(entry => entry.Player.Id == lineUp.Support1UserId && entry.Role == RaidRole.QuicknessDamageDealer);
        }
        else
        {
            SelectedTankRole = _tankRoles.FirstOrDefault(entry => entry.Player.Id == lineUp.TankUserId && entry.Role == RaidRole.QuicknessTankHealer);
            SelectedDpsSupport1 = _dpsSupportRoles.FirstOrDefault(entry => entry.Player.Id == lineUp.Support1UserId && entry.Role == RaidRole.AlacrityDamageDealer);
        }

        if (lineUp.HealerRaidRole == RaidRole.AlacrityHealer)
        {
            SelectedHealerRole = _healerRoles.FirstOrDefault(entry => entry.Player.Id == lineUp.HealerUserId && entry.Role == RaidRole.AlacrityHealer);
            SelectedDpsSupport2 = _dpsSupportRoles.FirstOrDefault(entry => entry.Player.Id == lineUp.Support2UserId && entry.Role == RaidRole.QuicknessDamageDealer);
        }
        else
        {
            SelectedHealerRole = _healerRoles.FirstOrDefault(entry => entry.Player.Id == lineUp.HealerUserId && entry.Role == RaidRole.QuicknessHealer);
            SelectedDpsSupport2 = _dpsSupportRoles.FirstOrDefault(entry => entry.Player.Id == lineUp.Support2UserId && entry.Role == RaidRole.AlacrityDamageDealer);
        }

        SelectedDps1 = _dpsRoles.FirstOrDefault(entry => entry.Player.Id == lineUp.Dps1UserId);
        SelectedDps2 = _dpsRoles.FirstOrDefault(entry => entry.Player.Id == lineUp.Dps2UserId);
        SelectedDps3 = _dpsRoles.FirstOrDefault(entry => entry.Player.Id == lineUp.Dps3UserId);
        SelectedDps4 = _dpsRoles.FirstOrDefault(entry => entry.Player.Id == lineUp.Dps4UserId);
        SelectedDps5 = _dpsRoles.FirstOrDefault(entry => entry.Player.Id == lineUp.Dps5UserId);
        SelectedDps6 = _dpsRoles.FirstOrDefault(entry => entry.Player.Id == lineUp.Dps6UserId);
        Remarks = lineUp.Remarks;

        StateHasChanged();
    }

    /// <summary>
    /// Set the selected role
    /// </summary>
    /// <param name="field">Field</param>
    /// <param name="value">Value</param>
    private void SetSelectedRole(ref PlayerRoleDTO field, PlayerRoleDTO value)
    {
        field?.Player.IsAssigned = false;

        field = value;

        field?.Player.IsAssigned = true;

        AssignmentChanged.InvokeAsync();
    }

    /// <summary>
    /// Get roles of the given types
    /// </summary>
    /// <param name="roles">Roles</param>
    /// <returns>Player roles</returns>
    private IEnumerable<PlayerRoleDTO> GetRoles(params RaidRole[] roles)
    {
        foreach (var role in roles)
        {
            foreach (var registration in Registrations)
            {
                if (registration.Roles.HasFlag(role))
                {
                    yield return new PlayerRoleDTO
                                 {
                                     Group = role switch
                                             {
                                                 RaidRole.DamageDealer => "DPS",
                                                 RaidRole.AlacrityDamageDealer => "Alacrity",
                                                 RaidRole.QuicknessDamageDealer => "Quickness",
                                                 RaidRole.AlacrityHealer => "Alacrity",
                                                 RaidRole.QuicknessHealer => "Quickness",
                                                 RaidRole.AlacrityTankHealer => "Alacrity",
                                                 RaidRole.QuicknessTankHealer => "Quickness",
                                                 _ => null
                                             },
                                     Role = role,
                                     Player = registration,
                                 };
                }
            }
        }
    }

    /// <summary>
    /// Filter for Tank
    /// </summary>
    /// <param name="playerRole">Player role</param>
    /// <returns>Should the entry be displayed?</returns>
    private bool OnTankFilter(PlayerRoleDTO playerRole)
    {
        if (SelectedDpsSupport1 == null)
        {
            return true;
        }

        if (SelectedDpsSupport1.Role == RaidRole.AlacrityDamageDealer)
        {
            return playerRole.Role == RaidRole.QuicknessTankHealer;
        }

        return playerRole.Role == RaidRole.AlacrityTankHealer;
    }

    /// <summary>
    /// Filter for Healer
    /// </summary>
    /// <param name="playerRole">Player role</param>
    /// <returns>Should the entry be displayed?</returns>
    private bool OnHealerFilter(PlayerRoleDTO playerRole)
    {
        if (SelectedDpsSupport2 == null)
        {
            return true;
        }

        if (SelectedDpsSupport2.Role == RaidRole.AlacrityDamageDealer)
        {
            return playerRole.Role == RaidRole.QuicknessHealer;
        }

        return playerRole.Role == RaidRole.AlacrityHealer;
    }

    /// <summary>
    /// Filter for DPS Support 1
    /// </summary>
    /// <param name="playerRole">Player role</param>
    /// <returns>Should the entry be displayed?</returns>
    private bool OnDpsSupport1Filter(PlayerRoleDTO playerRole)
    {
        if (SelectedTankRole == null)
        {
            return true;
        }

        if (SelectedTankRole.Role == RaidRole.AlacrityTankHealer)
        {
            return playerRole.Role == RaidRole.QuicknessDamageDealer;
        }

        return playerRole.Role == RaidRole.AlacrityDamageDealer;
    }

    /// <summary>
    /// Filter for DPS Support 2
    /// </summary>
    /// <param name="playerRole">Player role</param>
    /// <returns>Should the entry be displayed?</returns>
    private bool OnDpsSupport2Filter(PlayerRoleDTO playerRole)
    {
        if (SelectedHealerRole == null)
        {
            return true;
        }

        if (SelectedHealerRole.Role == RaidRole.AlacrityHealer)
        {
            return playerRole.Role == RaidRole.QuicknessDamageDealer;
        }

        return playerRole.Role == RaidRole.AlacrityDamageDealer;
    }

    #endregion // Methods

    #region ComponentBase

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        _dpsRoles = GetRoles(RaidRole.DamageDealer).ToList();
        _dpsSupportRoles = GetRoles(RaidRole.AlacrityDamageDealer, RaidRole.QuicknessDamageDealer).ToList();
        _tankRoles = GetRoles(RaidRole.AlacrityTankHealer, RaidRole.QuicknessTankHealer).ToList();
        _healerRoles = GetRoles(RaidRole.AlacrityHealer, RaidRole.QuicknessHealer).ToList();
    }

    #endregion // ComponentBase
}