using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.GuildWars2.Account;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Interfaces;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.GuildWars2;

/// <summary>
/// Guild Wars 2 related commands
/// </summary>
public class GuildWars2CommandHandler : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// Quaggan service
    /// </summary>
    private readonly QuagganService _quagganService;

    /// <summary>
    /// Guild Wars 2 update service
    /// </summary>
    private readonly GuildWarsUpdateService _updateService;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="quagganService">Quaggan service</param>
    /// <param name="updateService">Guild Wars 2 update service</param>
    public GuildWars2CommandHandler(LocalizationService localizationService, QuagganService quagganService, GuildWarsUpdateService updateService)
        : base(localizationService)
    {
        _quagganService = quagganService;
        _updateService = updateService;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Creation of a one time reminder
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public Task Quaggan(IContextContainer context) => _quagganService.PostRandomQuaggan(context);

    /// <summary>
    /// Next update
    /// </summary>
    /// <param name="context">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public Task Update(IContextContainer context) => _updateService.PostUpdateOverview(context);

    /// <summary>
    /// Post raid guides overview
    /// </summary>
    /// <param name="container">Context container</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task PostRaidGuides(IContextContainer container)
    {
        var builder = new EmbedBuilder().WithTitle("Raid guides")
                                        .WithColor(Color.Green)
                                        .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                                        .WithTimestamp(DateTime.Now);

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848910035747864576)} {Format.Url("Vale Guardian", "https://bit.ly/2EevcXD")} ({Format.Url("Mukluk", "https://bit.ly/3i7056E")} - {Format.Url("Tekkit", "https://bit.ly/3Xrru3C")} - {Format.Url("Hardstuck", "https://bit.ly/3EuRmCX")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 743938320459104317)} {Format.Url("Spirit Woods", "https://bit.ly/3ViTUuB")} ({Format.Url("Mukluk", "https://bit.ly/3GDDwRd")} - {Format.Url("Hardstuck", "https://bit.ly/3ViFyKL")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848908993538949131)} {Format.Url("Gorseval the Multifarious", "https://bit.ly/2EhcXkn")} ({Format.Url("Mukluk", "https://bit.ly/3U4IpWx")} - {Format.Url("Tekkit", "https://bit.ly/3i2B8cE")} - {Format.Url("Hardstuck", "https://bit.ly/3EymNwf")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909543915651072)} {Format.Url("Sabetha the Saboteur", "https://bit.ly/2V2GDaE")} ({Format.Url("Mukluk", "https://bit.ly/3tTBG7u")} - {Format.Url("Tekkit", "https://bit.ly/3XrTlAq")} - {Format.Url("Hardstuck", "https://bit.ly/3EvmaU3")})");
        builder.AddField("Wing 1 - Spirit Vale", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909627982610482)} {Format.Url("Slothasor", "https://bit.ly/2trV77X")} ({Format.Url("Mukluk", "https://bit.ly/3AEFFbD")} - {Format.Url("Tekkit", "https://bit.ly/3gqydKv")} - {Format.Url("Hardstuck", "https://bit.ly/3Ev2zDA")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909882115358720)} {Format.Url("Bandit Trio", "https://bit.ly/2BIN4Z3")} ({Format.Url("Mukluk", "https://bit.ly/3V0eRea")} - {Format.Url("Tekkit", "https://bit.ly/3gna87r")} - {Format.Url("Hardstuck", "https://bit.ly/3EuWt67")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909162821845043)} {Format.Url("Matthias Gabrel", "https://bit.ly/2GuID8a")} ({Format.Url("Mukluk", "https://bit.ly/3tUhcLC")} - {Format.Url("Hardstuck", "https://bit.ly/3EpLmvc")})");
        builder.AddField("Wing 2 - Salvation Pass", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 743938372195844117)} {Format.Url("Siege the Stronghold", "https://bit.ly/2SVjCZz")} ({Format.Url("Mukluk", "https://bit.ly/3US6gdr")} - {Format.Url("Hardstuck", "https://bit.ly/3ErY24R")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909049599885322)} {Format.Url("Keep Construct", "https://bit.ly/2UXydRM")} ({Format.Url("Mukluk", "https://bit.ly/3XqplVD")} - {Format.Url("Hardstuck", "https://bit.ly/3XqKmQk")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909953112473622)} {Format.Url("Twisted Castle", "https://bit.ly/3U0bPFl")} ({Format.Url("Mukluk", "https://bit.ly/3hYpD5T")} - {Format.Url("Hardstuck", "https://bit.ly/3ERgDZs")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848910090370940949)} {Format.Url("Xera", "https://bit.ly/2HYHbsr")} ({Format.Url("Mukluk", "https://bit.ly/3VczljK")} - {Format.Url("Hardstuck", "https://bit.ly/3tUi7M4")})");
        builder.AddField("Wing 3 - Stronghold of the Faithful", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848908521680142359)} {Format.Url("Cairn the Indomitable", "https://bit.ly/2SDot2m")} ({Format.Url("Mukluk", "https://bit.ly/3AC2ZXM")} - {Format.Url("Tekkit", "https://bit.ly/3TZ0hlQ")} - {Format.Url("Hardstuck", "https://bit.ly/3Vl51n2")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909340827713557)} {Format.Url("Mursaat Overseer", "https://bit.ly/2NeNsTO")} ({Format.Url("Mukluk", "https://bit.ly/3VmKHBM")} - {Format.Url("Tekkit", "https://bit.ly/3ETo1TZ")} - {Format.Url("Hardstuck", "https://bit.ly/3V1nk0B")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909587938803762)} {Format.Url("Samarog", "https://bit.ly/2V1NWiD")} ({Format.Url("Mukluk", "https://bit.ly/3V3fp34")} - {Format.Url("Tekkit", "https://bit.ly/3GEyNPp")} - {Format.Url("Hardstuck", "https://bit.ly/3EvmKkH")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848908773996101642)} {Format.Url("Deimos", "https://bit.ly/2Ij1Mf6")} ({Format.Url("Mukluk", "https://bit.ly/3V6Rr76")} - {Format.Url("Tekkit", "https://bit.ly/3i2BLD2")} - {Format.Url("Hardstuck", "https://bit.ly/3gz8JKN")})");
        builder.AddField("Wing 4 - Bastion of the Penitent", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848911345964679188)} {Format.Url("Soulless Horror", "https://bit.ly/2S80QtF")} ({Format.Url("Mukluk", "https://bit.ly/3EQSOB1")} - {Format.Url("Tekkit", "https://bit.ly/3Vej3Xe")} - {Format.Url("Hardstuck", "https://bit.ly/3XoJQCc")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 743940484455596064)} {Format.Url("River of Souls", "https://bit.ly/3AyqAIv")} ({Format.Url("Mukluk", "https://bit.ly/3tPIBhS")} - {Format.Url("Tekkit", "https://bit.ly/3ACZr7D")} - {Format.Url("Hardstuck", "https://bit.ly/3GFnNBk")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909739509547058)}{DiscordEmoteService.GetGuildEmote(container.Client, 848908876039585822)}{DiscordEmoteService.GetGuildEmote(container.Client, 848908317832773692)} {Format.Url("Statues of Grenth", "https://bit.ly/3tQqcRU")} ({Format.Url("Mukluk", "https://bit.ly/3i7OHYb")} - {Format.Url("Hardstuck", "https://bit.ly/3tTBRiW")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848908828866379777)} {Format.Url("Dhuum", "https://bit.ly/3GIXAlz")} ({Format.Url("Mukluk", "https://bit.ly/3tOSk8e")} - {Format.Url("Tekkit", "https://bit.ly/3VojvCF")} - {Format.Url("Hardstuck", "https://bit.ly/3i7oDwB")})");
        builder.AddField("Wing 5 - Hall of Chains", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848908712692547614)} {Format.Url("Conjured Amalgamate", "https://bit.ly/2BEAWrX")} ({Format.Url("Mukluk", "https://bit.ly/3i7t76p")} - {Format.Url("Hardstuck", "https://bit.ly/3i4jTI7")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909098619895808)} {Format.Url("Twin Largos", "https://bit.ly/3ACMcU9")} ({Format.Url("Mukluk", "https://bit.ly/3TTta2M")} - {Format.Url("Hardstuck", "https://bit.ly/3gr75eb")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909410691973140)} {Format.Url("Qadim", "https://bit.ly/2V3vAOh")} ({Format.Url("Mukluk", "https://bit.ly/3V1yiTM")} - {Format.Url("Hardstuck", "https://bit.ly/3UVLkCi")})");
        builder.AddField("Wing 6 - Mythwright Gambit", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848908653637533736)} {Format.Url("Cardinal Sabir", "https://bit.ly/3XqnD6L")} ({Format.Url("Mukluk", "https://bit.ly/3V1oIAu")} - {Format.Url("Hardstuck", "https://bit.ly/3tOXiSq")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848908580749049866)} {Format.Url("Cardinal Adina", "https://bit.ly/3XjsB5n")} ({Format.Url("Mukluk", "https://bit.ly/3AyvQvJ")} - {Format.Url("MightyTeapot", "https://bit.ly/3i4OnJX")} - {Format.Url("Hardstuck", "https://bit.ly/3tUj5YI")})");
        stringBuilder.AppendLine($"{DiscordEmoteService.GetGuildEmote(container.Client, 848909465553207296)} {Format.Url("Qadim the Peerless", "https://bit.ly/3XqLiUv")} ({Format.Url("Mukluk", "https://bit.ly/3XqLqDt")} - {Format.Url("Hepha Cinema", "https://bit.ly/3EQGznX")} - {Format.Url("Hardstuck", "https://bit.ly/3VhCT43")})");
        builder.AddField("Wing 7 - The Key of Ahdashim", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.Append($"{Format.Url("Xera", "https://bit.ly/3VmPH9t")}");
        stringBuilder.Append(" - ");
        stringBuilder.Append($"{Format.Url("Dhuum", "https://bit.ly/3gwfWvm")}");
        stringBuilder.Append(" - ");
        stringBuilder.Append($"{Format.Url("Qadim", "https://bit.ly/3tSPLlq")}");
        stringBuilder.Append(" - ");
        stringBuilder.Append($"{Format.Url("Qadim the Peerless", "https://bit.ly/3U0dkDt")}");
        builder.AddField("Markers / Reference sheets", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{Format.Url("Snow Crows", "https://bit.ly/2IJmFLh")} - {Format.Url("Lucky Noobs", "https://bit.ly/3tSh53g")} - {Format.Url("Hardstuck", "https://bit.ly/3V5UXP4")}");
        builder.AddField("Builds", stringBuilder.ToString());

        await container.ReplyAsync(embed: builder.Build())
                       .ConfigureAwait(false);
    }

    /// <summary>
    /// Post fractal guides overview
    /// </summary>
    /// <param name="container">Context container</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task PostFractalGuides(IContextContainer container)
    {
        var builder = new EmbedBuilder().WithTitle("Fractal guides")
                                        .WithColor(Color.Green)
                                        .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                                        .WithTimestamp(DateTime.Now);

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{Format.Url("Aetherblade", "https://bit.ly/3VihmYR")} ({Format.Url("Discretize", "https://bit.ly/3hXprnu")} - {Format.Url("Hardstuck", "https://bit.ly/3EFotUS")})");
        stringBuilder.AppendLine($"{Format.Url("Aquatic Ruins", "https://bit.ly/3EVoepQ")} ({Format.Url("Discretize", "https://bit.ly/3hW5I7F")} - {Format.Url("Hardstuck", "https://bit.ly/3XvasSf")})");
        stringBuilder.AppendLine($"{Format.Url("Captain Mai Trin Boss", "https://bit.ly/3Xrs5lO")} ({Format.Url("Discretize", "https://bit.ly/3OtVyHH")} - {Format.Url("Hardstuck", "https://bit.ly/3Vkpzfo")})");
        stringBuilder.AppendLine($"{Format.Url("Chaos", "https://bit.ly/3OtgHSp")} ({Format.Url("Discretize", "https://bit.ly/3OyfbhD")} - {Format.Url("Hardstuck", "https://bit.ly/3AHDUKR")})");
        stringBuilder.AppendLine($"{Format.Url("Cliffside", "https://bit.ly/3i9ofxx")} ({Format.Url("Discretize", "https://bit.ly/3OvG7i5")} - {Format.Url("Hardstuck", "https://bit.ly/3gypYvR")})");
        stringBuilder.AppendLine($"{Format.Url("Deepstone", "https://bit.ly/3UhxmK5")} ({Format.Url("Discretize", "https://bit.ly/3gwNSaY")} - {Format.Url("Hardstuck", "https://bit.ly/3OxInWe")})");
        stringBuilder.AppendLine($"{Format.Url("Molten Boss", "https://bit.ly/3V3YDAF")} ({Format.Url("Discretize", "https://bit.ly/3i7l8Gp")} - {Format.Url("Hardstuck", "https://bit.ly/3gpmgVu")})");
        stringBuilder.AppendLine($"{Format.Url("Molten Furnace", "https://bit.ly/3XuYlV2")} ({Format.Url("Discretize", "https://bit.ly/3AFzVi5")} - {Format.Url("Hardstuck", "https://bit.ly/3V0xL4L")})");
        stringBuilder.AppendLine($"{Format.Url("Nightmare", "https://bit.ly/3i8gKXG")} ({Format.Url("Hardstuck", "https://bit.ly/3Ezkxoh")})");
        stringBuilder.AppendLine($"{Format.Url("Shattered Observatory", "https://bit.ly/3tWayEG")} ({Format.Url("Hardstuck", "https://bit.ly/3tVfOsj")})");
        stringBuilder.AppendLine($"{Format.Url("Siren's Reef", "https://bit.ly/3i9oDvZ")} ({Format.Url("Discretize", "https://bit.ly/3VgY5ac")} - {Format.Url("Hardstuck", "https://bit.ly/3V40JR3")})");
        stringBuilder.AppendLine($"{Format.Url("Snowblind", "https://bit.ly/3EX1w0z")} ({Format.Url("Discretize", "https://bit.ly/3i2i5zb")} - {Format.Url("Hardstuck", "https://bit.ly/3XrLoLP")})");
        stringBuilder.AppendLine($"{Format.Url("Solid Ocean", "https://bit.ly/3EXnl08")} ({Format.Url("Discretize", "https://bit.ly/3EX0Fgp")} - {Format.Url("Hardstuck", "https://bit.ly/3UazdAk")})");
        stringBuilder.AppendLine($"{Format.Url("Sunqua Peak", "https://bit.ly/3OxZBmn")}");
        stringBuilder.AppendLine($"{Format.Url("Swampland", "https://bit.ly/3EzjdBP")} ({Format.Url("Discretize", "https://bit.ly/3GHVWjY")} - {Format.Url("Hardstuck", "https://bit.ly/3tVXPC2")})");
        stringBuilder.AppendLine($"{Format.Url("Thaumanova Reactor", "https://bit.ly/3U1261F")} ({Format.Url("Discretize", "https://bit.ly/3OE5gr1")} - {Format.Url("Hardstuck", "https://bit.ly/3gwP0LK")})");
        stringBuilder.AppendLine($"{Format.Url("Twilight Oasis", "https://bit.ly/3U4vAf3")} ({Format.Url("Discretize", "https://bit.ly/3V0EKdW")})");
        stringBuilder.AppendLine($"{Format.Url("Uncategorized", "https://bit.ly/3i22EqF")} ({Format.Url("Discretize", "https://bit.ly/3U130LB")} - {Format.Url("Hardstuck", "https://bit.ly/3Oyu6bC")})");
        stringBuilder.AppendLine($"{Format.Url("Underground Facility", "https://bit.ly/3EvdHQJ")} ({Format.Url("Discretize", "https://bit.ly/3Vi7KOx")} - {Format.Url("Hardstuck", "https://bit.ly/3GFSPZK")})");
        stringBuilder.AppendLine($"{Format.Url("Urban Battleground", "https://bit.ly/3iapLiL")} ({Format.Url("Discretize", "https://bit.ly/3XDPWyS")} - {Format.Url("Hardstuck", "https://bit.ly/3GP92Ma")})");
        stringBuilder.AppendLine($"{Format.Url("Volcanic", "https://bit.ly/3U4vR1z")} ({Format.Url("Discretize", "https://bit.ly/3ES9ffl")} - {Format.Url("Hardstuck", "https://bit.ly/3i6SYeL")})");
        builder.WithDescription(stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{Format.Url("Nightmare", "https://bit.ly/3i8gKXG")} ({Format.Url("Discretize", "https://bit.ly/3AJ2hrx")} - {Format.Url("Hardstuck", "https://bit.ly/3EqzSHG")})");
        stringBuilder.AppendLine($"{Format.Url("Shattered Observatory", "https://bit.ly/3tWayEG")} ({Format.Url("Discretize", "https://bit.ly/3hW5OMz")})");
        stringBuilder.AppendLine($"{Format.Url("Sunqua Peak", "https://bit.ly/3OxZBmn")} ({Format.Url("Discretize", "https://bit.ly/3tVWRFU")})");
        builder.AddField("Challenge mode", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{Format.Url("Cheat sheet", "https://bit.ly/3AF42pC")} - {Format.Url("CC", "https://bit.ly/3XuwAfc")}  - {Format.Url("Teamcomps", "https://bit.ly/3GP7lyi")}");
        builder.AddField("Links", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{Format.Url("Discretize", "https://bit.ly/3Vnlfff")} - {Format.Url("Hardstuck", "https://bit.ly/3V5UXP4")}");
        builder.AddField("Builds", stringBuilder.ToString());

        await container.ReplyAsync(embed: builder.Build())
                       .ConfigureAwait(false);
    }

    /// <summary>
    /// Post fractal guides overview
    /// </summary>
    /// <param name="container">Context container</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task PostStrikeMissionGuides(IContextContainer container)
    {
        var builder = new EmbedBuilder().WithTitle("Strike Missions guides")
                                        .WithColor(Color.Green)
                                        .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                                        .WithTimestamp(DateTime.Now);

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{Format.Url("Old Lion's Court", "https://bit.ly/3V0zLtr")} ({Format.Url("Mukluk", "https://bit.ly/3Xt0eBz")} - {Format.Url("Hardstuck", "https://bit.ly/3ETyGhF")})");
        builder.AddField("Living World Season 1", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{Format.Url("Shiverpeaks Pass", "https://bit.ly/3i6N5hE")} ({Format.Url("Hardstuck", "https://bit.ly/3i8ZX6C")})");
        stringBuilder.AppendLine($"{Format.Url("Voice of the Fallen and Claw of the Fallen", "https://bit.ly/3tT9dyr")} ({Format.Url("Hardstuck", "https://bit.ly/3Vh0amw")})");
        stringBuilder.AppendLine($"{Format.Url("Fraenir of Jormag", "https://bit.ly/3tWcY6e")} ({Format.Url("Hardstuck", "https://bit.ly/3VnH7Hu")})");
        stringBuilder.AppendLine($"{Format.Url("Boneskinner", "https://bit.ly/3gCEvGK")} ({Format.Url("Hardstuck", "https://bit.ly/3i5xgaP")})");
        stringBuilder.AppendLine($"{Format.Url("Whisper of Jormag", "https://bit.ly/3gBJeZj")} ({Format.Url("Hardstuck", "https://bit.ly/3AGNsG2")})");
        stringBuilder.AppendLine($"{Format.Url("Forging Steel", "https://bit.ly/3GCMkHj")}");
        stringBuilder.AppendLine($"{Format.Url("Cold War", "https://bit.ly/3OtkfEd")}");
        builder.AddField("The Icebrood Saga", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{Format.Url("Aetherblade Hideout", "https://bit.ly/3gvyyLP")} ({Format.Url("Hardstuck", "https://bit.ly/3U3ZvDY")})");
        stringBuilder.AppendLine($"{Format.Url("Xunlai Jade Junkyard", "https://bit.ly/3GHXHh4")} ({Format.Url("Hardstuck", "https://bit.ly/3U6YxXI")})");
        stringBuilder.AppendLine($"{Format.Url("Kaineng Overlook", "https://bit.ly/3gu0j7s")} ({Format.Url("Hardstuck", "https://bit.ly/3VqW1Nf")})");
        stringBuilder.AppendLine($"{Format.Url(" Harvest Temple", "https://bit.ly/3EwlwG4")} ({Format.Url("Hardstuck", "https://bit.ly/3EZ8Ngx")})");
        builder.AddField("End of Dragons", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"Mukluk: {Format.Url("IBS", "https://bit.ly/3AHFbBD")}, {Format.Url("EoD", "https://bit.ly/3EFpWKS")}");
        builder.AddField("Links", stringBuilder.ToString());

        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{Format.Url("Snow Crows", "https://bit.ly/2IJmFLh")} - {Format.Url("Lucky Noobs", "https://bit.ly/3tSh53g")} - {Format.Url("Hardstuck", "https://bit.ly/3V5UXP4")}");
        builder.AddField("Builds", stringBuilder.ToString());

        await container.ReplyAsync(embed: builder.Build())
                       .ConfigureAwait(false);
    }

    /// <summary>
    /// Lists full material storage
    /// </summary>
    /// <param name="context">Context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task ListFullMaterialStorage(InteractionContextContainer context)
    {
        await context.DeferAsync()
                     .ConfigureAwait(false);

        using (var dbFactory = RepositoryFactory.CreateInstance())
        {
            var accounts = dbFactory.GetRepository<GuildWarsAccountRepository>()
                                    .GetQuery()
                                    .Where(obj => obj.User.DiscordAccounts.Any(discordAccount => discordAccount.Id == context.User.Id))
                                    .Select(obj => new
                                                   {
                                                       obj.Name,
                                                       obj.ApiKey
                                                   })
                                    .ToList();

            foreach (var account in accounts)
            {
                using (var connector = new GuildWars2ApiConnector(account.ApiKey))
                {
                    var materials = await connector.GetAccountMaterials()
                                                   .ConfigureAwait(false);

                    materials = materials.Where(item => item.Count > 0
                                                        && item.Count % 250 == 0)
                                         .ToList();

                    var maxCount = materials.Count > 0
                                      ? materials.Max(item => item.Count)
                                      : 0;

                    var embed = new EmbedBuilder().WithTitle(LocalizationGroup.GetText("FullMaterialTitle", "Full material storage slots"))
                                                  .WithDescription(LocalizationGroup.GetFormattedText("FullMaterialDescription", "**Account:** {0}\n**Assumed Slot count:** {1}", account.Name, maxCount == 0 ? "-" : maxCount.ToString()))
                                                  .WithColor(Color.Green)
                                                  .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                                                  .WithTimestamp(DateTime.Now);

                    if (materials.Count > 0)
                    {
                        var categories = materials.Where(item => item.Count == maxCount)
                                                  .GroupBy(item => item.Category)
                                                  .ToList();

                        var categoryInformation = await connector.GetMaterialCategory(categories.Select(category => category.Key))
                                                                 .ConfigureAwait(false);

                        var items = await connector.GetItems(categories.SelectMany(category => category.Select(item => (int?)item.Id)).ToList())
                                                   .ConfigureAwait(false);

                        foreach (var category in categories)
                        {
                            var categoryName = categoryInformation.FirstOrDefault(categoryInfo => categoryInfo.Id == category.Key)?.Name
                                                   ?? LocalizationGroup.GetText("UnknownMaterialCategory", "Unknown Category");

                            var field = new EmbedFieldBuilder().WithName(categoryName);
                            var fieldValue = new StringBuilder();

                            foreach (var item in category)
                            {
                                var itemName = items.FirstOrDefault(itemInfo => itemInfo.Id == item.Id)?.Name
                                                   ?? LocalizationGroup.GetText("UnknownMaterial", "Unknown Material");

                                fieldValue.Append("> ");
                                fieldValue.AppendLine(itemName);
                            }

                            embed.AddField(field.WithValue(fieldValue.ToString()));
                        }
                    }

                    await context.SendMessageAsync(embed: embed.Build())
                                 .ConfigureAwait(false);
                }
            }
        }
    }

    #endregion // Methods
}