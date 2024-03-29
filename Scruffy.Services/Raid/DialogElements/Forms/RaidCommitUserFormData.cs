﻿using Discord;

using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Services.Raid.DialogElements.Forms;

/// <summary>
/// Raid commit user and points
/// </summary>
public class RaidCommitUserFormData
{
    /// <summary>
    /// User
    /// </summary>
    [DialogElementAssignment(typeof(RaidCommitUserDialogElement))]
    public IUser User { get; set; }

    /// <summary>
    /// Points
    /// </summary>
    [DialogElementAssignment(typeof(RaidCommitPointDialogElement))]
    public double Points { get; set; }
}