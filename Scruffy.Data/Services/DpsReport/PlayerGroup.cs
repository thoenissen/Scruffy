namespace Scruffy.Data.Services.DpsReport;

/// <summary>
/// A grouping of players in a DPS report
/// </summary>
public class PlayerGroup
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id">ID of this group</param>
    /// <param name="date">Date of the group</param>
    /// <param name="players">Players in this group</param>
    /// <param name="withStats">Whether to add stats</param>
    public PlayerGroup(int id, DateOnly date, HashSet<string> players, bool withStats)
    {
        Id = id;
        Date = date;
        Players = players;

        if (withStats)
        {
            Stats = new PlayerGroupStats();
        }
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Id of this group
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Day of the week this group exists
    /// </summary>
    public DateOnly Date { get; private set; }

    /// <summary>
    /// Players in this group
    /// </summary>
    public HashSet<string> Players { get; private set; }

    /// <summary>
    /// Stats of this group
    /// </summary>
    public PlayerGroupStats Stats { get; private set; }

    #endregion // Properties

    #region Object

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (obj is PlayerGroup other)
        {
            if (Date == other.Date)
            {
                var group = new HashSet<string>(Players);

                group.ExceptWith(other.Players);

                return group.Count < Players.Count * 0.4;
            }
        }

        return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        // Always use equals
        return Date.GetHashCode();
    }

    #endregion // Object
}