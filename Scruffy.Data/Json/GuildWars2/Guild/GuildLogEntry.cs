using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Scruffy.Data.Json.GuildWars2.Guild
{
    /// <summary>
    /// Guild log entry
    /// </summary>
    public class GuildLogEntry
    {
        /// <summary>
        /// An ID to uniquely identify the log entry within the scope of the guild. Not globally unique.
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// ISO-8601 standard timestamp for when the log entry was created.
        /// </summary>
        [JsonProperty("time")]
        public DateTime Time { get; set; }

        /// <summary>
        /// The type of log entry. Additional fields are given depending on the type. Possible values:
        ///      - joined - user has joined the guild
        ///      - invited - user has been invited to the guild.Additional fields include:
        ///            invited_by(string) - Account name of the guild member which invited the player.
        ///      - kick - user has been kicked from the guild.Additional fields include:
        ///            kicked_by (string) - Account name of the guild member which kicked the player.
        ///      - rank_change - Rank for user has been changed. Additional fields include:
        ///            changed_by (string) - Account name of the guild member which changed the player rank.
        ///                old_rank (string) - Old rank name.
        ///                new_rank (string) - New rank name.
        ///      - treasury - A guild member has deposited an item into the guild's treasury. Additional fields include:
        ///            item_id (number) - The item ID that was deposited into the treasury.
        ///            count (number) - How many of the specified item was deposited.
        ///      - stash - A guild member has deposited/withdrawn an item into the guild stash.Additional fields include:
        ///            operation (string) - Possible values:
        ///                - deposit
        ///                -     withdraw
        ///                - move
        ///                -     item_id (number) - The item ID that was deposited into the treasury.
        ///                -     count (number) - How many of the specified item was deposited.
        ///                -     coins (number) - How many coins (in copper) were deposited.
        ///      - motd - A guild member has changed the guild's MOTD. Additional fields include:
        ///            motd (string) - The new MOTD.
        ///      - upgrade - A guild member has interacted with a guild upgrade.Additional fields include:
        ///            action(string) - The type of interaction had.Possible values:
        ///                queued
        ///                cancelled
        ///                completed - Having this action will also generate a new count field indicating how many upgrades were added.
        ///                sped_up
        ///            upgrade_id (number) - The upgrade ID which was completed.
        ///            recipe_id (number)(optional) - May be added if the upgrade was created through a scribe station by a scribe.
        ///      - influence — Additional fields include:
        ///            activity(string) — Activity that generated influence for the guild.Possible values: daily_login, gifted
        ///            total_participants (number) — Number of members.
        ///            participants (array of strings) — An array of account names of participants.Strings may be null.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// The account name of the guild member who generated this log entry.
        /// </summary>
        [JsonProperty("user")]
        public string User { get; set; }

        /// <summary>
        ///  Account name of the guild member which kicked the player.
        /// </summary>
        [JsonProperty("kicked_by")]
        public string KickedBy { get; set; }

        /// <summary>
        /// Account name of the guild member which invited the player.
        /// </summary>
        [JsonProperty("invited_by")]
        public string InvitedBy { get; set; }

        /// <summary>
        /// Possible values:
        ///  - deposit
        ///  - withdraw
        ///  - move
        /// </summary>
        [JsonProperty("operation")]
        public string Operation { get; set; }

        /// <summary>
        /// The item ID that was deposited into the treasury.
        /// </summary>
        [JsonProperty("item_id")]
        public int? ItemId { get; set; }

        /// <summary>
        /// How many of the specified item was deposited.
        /// </summary>
        [JsonProperty("count")]
        public int? Count { get; set; }

        /// <summary>
        /// How many coins (in copper) were deposited.
        /// </summary>
        [JsonProperty("coins")]
        public int? Coins { get; set; }

        /// <summary>
        /// Account name of the guild member which changed the player rank.
        /// </summary>
        [JsonProperty("changed_by")]
        public string ChangedBy { get; set; }

        /// <summary>
        /// Old rank name.
        /// </summary>
        [JsonProperty("old_rank")]
        public string OldRank { get; set; }

        /// <summary>
        /// New rank name.
        /// </summary>
        [JsonProperty("new_rank")]
        public string NewRank { get; set; }

        /// <summary>
        /// The upgrade ID which was completed.
        /// </summary>
        [JsonProperty("upgrade_id")]
        public int? UpgradeId { get; set; }

        /// <summary>
        /// May be added if the upgrade was created through a scribe station by a scribe.
        /// </summary>
        [JsonProperty("recipe_id ")]
        public int? RecipeId { get; set; }

        /// <summary>
        /// The type of interaction had. Possible values:
        ///  - queued
        ///  - cancelled
        ///  - completed - Having this action will also generate a new count field indicating how many upgrades were added.
        ///  - sped_up
        /// </summary>
        [JsonProperty("action")]
        public string Action { get; set; }

        /// <summary>
        /// Activity that generated influence for the guild. Possible values: daily_login, gifted
        /// </summary>
        [JsonProperty("activity")]
        public string Activity { get; set; }

        /// <summary>
        /// Number of members.
        /// </summary>
        [JsonProperty("total_participants")]
        public int? TotalParticipants { get; set; }

        /// <summary>
        /// An array of account names of participants. Strings may be null.
        /// </summary>
        [JsonProperty("participants")]
        public List<string> Participants { get; set; }

        /// <summary>
        /// The new MOTD.
        /// </summary>
        [JsonProperty("motd")]
        public string MessageOfTheDay { get; set; }
    }
}
