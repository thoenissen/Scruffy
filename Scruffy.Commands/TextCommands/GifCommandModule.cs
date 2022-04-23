using System.Net.Http;

using Discord.Commands;

using Microsoft.AspNetCore.WebUtilities;

using Newtonsoft.Json;

using Scruffy.Data.Enumerations.General;
using Scruffy.Data.Json.Tenor;
using Scruffy.Services.Core;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Attributes;
using Scruffy.Services.Discord.Extensions;

namespace Scruffy.Commands.TextCommands;

/// <summary>
/// GIF commands
/// </summary>
[Group("gif")]
[Alias("gi")]
[BlockedChannelCheck]
public class GifCommandModule : LocatedTextCommandModuleBase
{
    #region Methods

    /// <summary>
    /// GIF related to a string
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Command]
    public Task GroupCommand([Remainder] string searchTerm) => ShowMigrationMessage("gif");

    #endregion // Methods
}