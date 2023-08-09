using Discord;

using Microsoft.AspNetCore.Mvc;

using Scruffy.Services.Discord;

namespace Scruffy.ServiceHosts.WebApi.Controllers;

/// <summary>
/// State controller
/// </summary>
[ApiController]
[Route("resources")]
#if !DEBUG
[Microsoft.AspNetCore.Authorization.Authorize]
#endif
public class ResourcesController : ControllerBase
{
    #region Fields

    /// <summary>
    /// Discord client
    /// </summary>
    private readonly IDiscordClient _discordClient;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="discordClient">Discord client</param>
    public ResourcesController(IDiscordClient discordClient)
    {
        _discordClient = discordClient;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Get emotes
    /// </summary>
    /// <returns>Action result</returns>
    [HttpGet]
    [Route("emotes")]
    public IActionResult GetEmotes()
    {
        return Ok(new
                  {
                      DamageDealer = (DiscordEmoteService.GetDamageDealerEmote(_discordClient) as GuildEmote)?.Url,
                      Alacrity = (DiscordEmoteService.GetAlacrityEmote(_discordClient) as GuildEmote)?.Url,
                      Quickness = (DiscordEmoteService.GetQuicknessEmote(_discordClient) as GuildEmote)?.Url,
                      Tank = (DiscordEmoteService.GetTankEmote(_discordClient) as GuildEmote)?.Url,
                      Healer = (DiscordEmoteService.GetQuicknessEmote(_discordClient) as GuildEmote)?.Url
                  });
    }

    #endregion // Methods
}