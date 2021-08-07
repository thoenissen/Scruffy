using System.Collections.Generic;

using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.Core
{
    /// <summary>
    /// Token Information
    /// </summary>
    public class TokenInformation
    {
        #region Permissions

        /// <summary>
        /// Permissions
        /// </summary>
        public static class Permission
        {
            /// <summary>
            /// Account
            /// </summary>
            public const string Account = "account";

            /// <summary>
            /// Characters
            /// </summary>
            public const string Characters = "characters";
        }

        #endregion // Permissions

        #region Properties

        /// <summary>
        /// Id
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Array of strings describing which permissions the API key has. The array can contain any of:
        ///  - account - Grants access to the /v2/account endpoint(This permission is required for all API keys).
        ///  - builds - Grants access to view each character's equipped specializations and gear.
        ///  - characters - Grants access to the /v2/characters endpoint.
        ///  - guilds - Grants access to guild info under the /v2/guild/:id/ sub-endpoints.
        ///  - inventories - Grants access to inventories in the /v2/characters, /v2/account/bank, and /v2/account/materials endpoints.
        ///  - progression - Grants access to achievements, dungeon unlock status, mastery point assignments, and general PvE progress.
        ///  - pvp - Grants access to the /v2/pvp sub-endpoints. (i.e. /v2/pvp/games, /v2/pvp/stats)
        ///  - tradingpost - Grants access to the /v2/commerce/transactions endpoint.
        ///  - unlocks - Grants access to the /v2/account/skins and /v2/account/dyes endpoints.
        ///  - wallet - Grants access to the /v2/account/wallet endpoint.
        /// </summary>
        [JsonProperty("permissions")]
        public List<string> Permissions { get; set; }

        #endregion // Properties
    }
}
