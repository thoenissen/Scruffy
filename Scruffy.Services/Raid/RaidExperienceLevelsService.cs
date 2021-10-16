﻿using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.Entities;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Raid
{
    /// <summary>
    /// Raid experience levels
    /// </summary>
    public class RaidExperienceLevelsService : LocatedServiceBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public RaidExperienceLevelsService(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Post overview of experience roles
        /// </summary>
        /// <param name="commandContextContainer">Command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task PostExperienceLevelOverview(CommandContextContainer commandContextContainer)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var levels = await dbFactory.GetRepository<RaidExperienceLevelRepository>()
                                            .GetQuery()
                                            .OrderBy(obj => obj.Description)
                                            .Select(obj => new
                                            {
                                                obj.DiscordEmoji,
                                                obj.Description,

                                                Users = obj.Users.SelectMany(obj2 => obj2.DiscordAccounts)
                                                                 .OrderBy(obj2 => obj2.Id)
                                                                 .Select(obj2 => obj2.Id)
                                            })
                                            .ToListAsync()
                                            .ConfigureAwait(false);

                var embedBuilder = new DiscordEmbedBuilder
                                   {
                                       Color = DiscordColor.Green,
                                       Description = LocalizationGroup.GetText("ExperienceLevels", "Experience levels")
                                   };

                var stringBuilder = new StringBuilder();
                var fieldCounter = 1;
                var currentFieldTitle = string.Empty;

                foreach (var level in levels)
                {
                    var levelFieldCounter = 1;

                    currentFieldTitle = $"{DiscordEmojiService.GetGuildEmoji(commandContextContainer.Client, level.DiscordEmoji)} {level.Description} #{levelFieldCounter}";

                    foreach (var entry in level.Users)
                    {
                        var user = await commandContextContainer.Client
                                                       .GetUserAsync(entry)
                                                       .ConfigureAwait(false);

                        var currentLine = $"{user.Mention}\n";

                        if (currentLine.Length + stringBuilder.Length > 1024)
                        {
                            stringBuilder.Append('\u200B');
                            embedBuilder.AddField(currentFieldTitle, stringBuilder.ToString());

                            if (fieldCounter == 6)
                            {
                                fieldCounter = 1;

                                await commandContextContainer.Channel
                                                    .SendMessageAsync(embedBuilder)
                                                    .ConfigureAwait(false);

                                embedBuilder = new DiscordEmbedBuilder
                                               {
                                                   Color = DiscordColor.Green
                                               };
                            }
                            else
                            {
                                fieldCounter++;
                            }

                            levelFieldCounter++;

                            currentFieldTitle = $"{DiscordEmojiService.GetGuildEmoji(commandContextContainer.Client, level.DiscordEmoji)} {level.Description} #{levelFieldCounter}";

                            stringBuilder = new StringBuilder();
                        }

                        stringBuilder.Append(currentLine);
                    }

                    stringBuilder.Append('\u200B');

                    embedBuilder.AddField(currentFieldTitle, stringBuilder.ToString());

                    stringBuilder = new StringBuilder();
                }

                await commandContextContainer.Channel
                                    .SendMessageAsync(embedBuilder)
                                    .ConfigureAwait(false);
            }
        }

        #endregion // Methods
    }
}
