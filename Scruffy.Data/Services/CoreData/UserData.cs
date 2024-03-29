﻿namespace Scruffy.Data.Services.CoreData;

/// <summary>
/// User data
/// </summary>
public class UserData
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Experience level rank
    /// </summary>
    public  int ExperienceLevelRank { get; set; }

    /// <summary>
    /// Are the data storage terms accepted?
    /// </summary>
    public bool? IsDataStorageAccepted { get; set; }
}