using System.Net.Http;

using Newtonsoft.Json;

using Scruffy.Data.Json.DpsReport;

namespace Scruffy.Services.WebApi;

/// <summary>
/// DPS-Report Connector
/// </summary>
public class DpsReportConnector
{
    #region Fields

    /// <summary>
    /// Client factory
    /// </summary>
    private readonly IHttpClientFactory _clientFactory;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="clientFactory">Client factory</param>
    public DpsReportConnector(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Request  dps reports
    /// </summary>
    /// <param name="userToken">User token</param>
    /// <param name="page">Page</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<Page> GetUploads(string userToken, int page)
    {
        var client = _clientFactory.CreateClient();

        using (var response = await client.GetAsync($"https://dps.report/getUploads?userToken={userToken}&page={page}")
                                          .ConfigureAwait(false))
        {
            var jsonResult = await response.Content
                                           .ReadAsStringAsync()
                                           .ConfigureAwait(false);

            return JsonConvert.DeserializeObject<Page>(jsonResult);
        }
    }

    /// <summary>
    /// Get boss description
    /// </summary>
    /// <param name="bossId">Boss ID</param>
    /// <returns>Boss description</returns>
    public string GetBossDescription(int bossId)
    {
        var description = bossId switch
        {
            15438 => "Vale Guardian",
            15429 => "Gorseval",
            15375 => "Sabetha the Saboteur",
            16123 => "Slothasor",
            16115 => "Matthias Gabrel",
            16235 => "Keep Construct",
            16246 => "Xera",
            17194 => "Cairn the Indomitable",
            17172 => "Mursaat Overseer",
            17188 => "Samarog",
            17154 => "Deimos",
            19767 => "Soulless Horror",
            19450 => "Dhuum",
            43974 => "Conjured Amalgamate",
            21105 => "Twin Largos",
            21089 => "Twin Largos",
            20934 => "Qadim",
            22006 => "Cardinal Adina",
            21964 => "Cardinal Sabir",
            22000 => "Qadim the Peerless",
            16088 => "Bandit Trio",
            16137 => "Bandit Trio",
            16125 => "Bandit Trio",
            16253 => "McLeod the Silent",
            16247 => "Twisted Castle",
            19828 => "Desmina Escort",
            19691 => "Broken King",
            19536 => "Soul Eater",
            19651 => "Eye of Judgement",
            19844 => "Eye of Fate",
            17021 => "M A M A",
            17028 => "Siax the Corrupted",
            16948 => "Ensolyss of the Endless Torment",
            17632 => "Skorvald the Shattered",
            17949 => "Artsariiv",
            17759 => "Arkk",
            23254 => "Ai, Keeper of the Peak",
            22154 => "Icebrood Construct",
            22343 => "The Voice and The Claw",
            22481 => "The Voice and The Claw",
            22315 => "The Voice and the Claw",
            22492 => "Fraenir of Jormag",
            22436 => "Fraenir of Jormag",
            22521 => "Boneskinner",
            22711 => "Whisper of Jormag",
            22836 => "Varinia Stormsounder",
            21333 => "Freezie",
            16199 => "Standard Kitty Golem",
            19645 => "Medium Kitty Golem",
            19676 => "Large Kitty Golem",
            16202 => "Massive Kitty Golem",
            16177 => "Average Kitty Golem",
            16198 => "Vital Kitty Golem",
            _ => "Unknown"
        };

        return description;
    }

    /// <summary>
    /// Get raid wing description
    /// </summary>
    /// <param name="bossId">Boss ID</param>
    /// <returns>Raid wing description</returns>
    public string GetRaidWingDescription(int bossId)
    {
        var description = "Other";

        switch (bossId)
        {
            case 15438:
            case 15429:
            case 15375:
                {
                    description = "Spirit Vale";
                }
                break;

            case 16123:
            case 16088:
            case 16137:
            case 16125:
            case 16115:
                {
                    description = "Salvation Pass";
                }
                break;

            case 16235:
            case 16247:
            case 16253:
            case 16246:
                {
                    description = "Stronghold of the Faithful";
                }
                break;

            case 17194:
            case 17172:
            case 17188:
            case 17154:
                {
                    description = "Bastion of the Penitent";
                }
                break;

            case 19767:
            case 19828:
            case 19691:
            case 19536:
            case 19651:
            case 19844:
            case 19450:
                {
                    description = "Hall of Chains";
                }
                break;

            case 43974:
            case 21105:
            case 21089:
            case 20934:
                {
                    description = "Mythwright Gambit";
                }
                break;

            case 22006:
            case 21964:
            case 22000:
                {
                    description = "Key of Ahdashim ";
                }
                break;

            case 17021:
            case 17028:
            case 16948:
            case 17632:
            case 17949:
            case 17759:
            case 23254:
                {
                    description = "Fractals";
                }
                break;

            case 22154:
            case 22343:
            case 22481:
            case 22315:
            case 22492:
            case 22436:
            case 22521:
            case 22711:
                {
                    description = "Strike missions";
                }
                break;
        }

        return description;
    }

    /// <summary>
    /// Get sort number
    /// </summary>
    /// <param name="bossId">Boss ID</param>
    /// <returns>Sort number</returns>
    public int GetRaidSortNumber(int bossId)
    {
        var number = bossId switch
        {
            15438 => 101,
            15429 => 102,
            15375 => 103,
            16123 => 201,
            16088 => 202,
            16137 => 202,
            16125 => 202,
            16115 => 203,
            16253 => 301,
            16235 => 302,
            16247 => 303,
            16246 => 304,
            17194 => 401,
            17172 => 402,
            17188 => 403,
            17154 => 404,
            19767 => 501,
            19828 => 502,
            19651 => 503,
            19844 => 503,
            19536 => 504,
            19691 => 505,
            19450 => 506,
            43974 => 601,
            21105 => 602,
            21089 => 602,
            20934 => 603,
            22006 => 701,
            21964 => 702,
            22000 => 703,
            _ => 0
        };

        return number;
    }

    /// <summary>
    /// Get boss icon ID
    /// </summary>
    /// <param name="bossId">Boss ID</param>
    /// <returns>Icon ID</returns>
    public ulong GetRaidBossIconId(int bossId)
    {
        ulong iconId = bossId switch
        {
            15438 => 848910035747864576,
            15429 => 848908993538949131,
            15375 => 848909543915651072,
            16123 => 848909627982610482,
            16088 => 848909882115358720,
            16137 => 848909882115358720,
            16125 => 848909882115358720,
            16115 => 848909162821845043,
            16253 => 743938372195844117,
            16235 => 848909049599885322,
            16247 => 848909953112473622,
            16246 => 848910090370940949,
            17194 => 848908521680142359,
            17172 => 848909340827713557,
            17188 => 848909587938803762,
            17154 => 848908773996101642,
            19767 => 848911345964679188,
            19828 => 743940484455596064,
            19651 => 848909739509547058,
            19844 => 848909739509547058,
            19536 => 848908876039585822,
            19691 => 848908317832773692,
            19450 => 848908828866379777,
            43974 => 848908712692547614,
            21105 => 848909098619895808,
            21089 => 848909098619895808,
            20934 => 848909410691973140,
            22006 => 848908580749049866,
            21964 => 848908653637533736,
            22000 => 848909465553207296,
            _ => 0ul
        };

        return iconId;
    }

    #endregion // Methods
}