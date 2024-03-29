﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scruffy.Data.Entity.Tables.Developer;

/// <summary>
/// GitHub commit
/// </summary>
[Table("GitHubCommits")]
public class GitHubCommitEntity
{
    /// <summary>
    /// SHA checksum
    /// </summary>
    [StringLength(40)]
    public string Sha {  get; set; }

    /// <summary>
    /// Author
    /// </summary>
    public string Author { get; set; }

    /// <summary>
    /// Committer
    /// </summary>
    public string Committer { get; set; }

    /// <summary>
    /// Timestamp
    /// </summary>
    public DateTime TimeStamp { get; set; }
}