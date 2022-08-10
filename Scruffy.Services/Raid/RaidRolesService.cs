using Discord.WebSocket;

using Scruffy.Data.Entity.Tables.Raid;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Raid
{
    /// <summary>
    /// Raid roles
    /// </summary>
    public class RaidRolesService : LocatedServiceBase
    {
        #region Fields

        /// <summary>
        /// Discord client
        /// </summary>
        private readonly DiscordSocketClient _discordClient;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        /// <param name="discordClient">Discord client</param>
        public RaidRolesService(LocalizationService localizationService, DiscordSocketClient discordClient)
            : base(localizationService)
        {
            _discordClient = discordClient;
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Returns description
        /// </summary>
        /// <param name="raidRole">Raid role</param>
        /// <returns>Description</returns>
        public string GetDescriptionAsText(RaidRoleEntity raidRole)
        {
            var description = string.Empty;

            if (raidRole.IsTank)
            {
                description = "Tank";
            }

            if (raidRole.IsProvidingAlacrity)
            {
                if (description.Length != 0)
                {
                    description += " | ";
                }

                description += "Alacrity";
            }

            if (raidRole.IsProvidingQuickness)
            {
                if (description.Length != 0)
                {
                    description += " | ";
                }

                description += "Quickness";
            }

            if (raidRole.IsHealer)
            {
                if (description.Length != 0)
                {
                    description += " | ";
                }

                description += "Healer";
            }

            if (raidRole.IsDamageDealer)
            {
                if (description.Length != 0)
                {
                    description += " | ";
                }

                description += "DPS";
            }

            return description;
        }

        /// <summary>
        /// Returns description
        /// </summary>
        /// <param name="raidRole">Raid role</param>
        /// <returns>Description</returns>
        public string GetDescriptionAsEmoji(RaidRoleEntity raidRole)
        {
            var description = string.Empty;

            if (raidRole.IsTank)
            {
                description = DiscordEmoteService.GetTankEmote(_discordClient).ToString();
            }

            if (raidRole.IsProvidingAlacrity)
            {
                description += DiscordEmoteService.GetAlacrityEmote(_discordClient).ToString();
            }

            if (raidRole.IsProvidingQuickness)
            {
                description += DiscordEmoteService.GetQuicknessEmote(_discordClient).ToString();
            }

            if (raidRole.IsHealer)
            {
                description += DiscordEmoteService.GetHealerEmote(_discordClient).ToString();
            }

            if (raidRole.IsDamageDealer)
            {
                description += DiscordEmoteService.GetDamageDealerEmote(_discordClient).ToString();
            }

            return description;
        }

        #endregion // Methods
    }
}