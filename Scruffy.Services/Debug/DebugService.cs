using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace Scruffy.Services.Debug
{
    /// <summary>
    /// Debug service
    /// </summary>
    public class DebugService
    {
        #region Methods

        /// <summary>
        /// Dump text of reply
        /// </summary>
        /// <param name="commandContext">Context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        public async Task DumpText(CommandContext commandContext)
        {
            if (commandContext.Message.ReferencedMessage != null)
            {
                await using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(commandContext.Message.ReferencedMessage.Content)))
                {
                    await commandContext.RespondAsync(new DiscordMessageBuilder().WithFile("dump.txt", memoryStream))
                                        .ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// List roles
        /// </summary>
        /// <param name="commandContext">Context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        public async Task ListRoles(CommandContext commandContext)
        {
            await ListEntries(commandContext, "Roles", commandContext.Guild.Roles.Select(obj => obj.Value.Mention).OrderBy(obj => obj)).ConfigureAwait(false);
        }

        /// <summary>
        /// List channels
        /// </summary>
        /// <param name="commandContext">Context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        public async Task ListChannels(CommandContext commandContext)
        {
            await ListEntries(commandContext, "Channels", commandContext.Guild.Channels.Select(obj => obj.Value.Mention).OrderBy(obj => obj)).ConfigureAwait(false);
        }

        /// <summary>
        /// List emojis
        /// </summary>
        /// <param name="commandContext">Context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        public async Task ListEmojis(CommandContext commandContext)
        {
            await ListEntries(commandContext, "Emojis", commandContext.Guild.Emojis.Select(obj => obj.Value.ToString()).OrderBy(obj => obj)).ConfigureAwait(false);
        }

        /// <summary>
        /// List emojis
        /// </summary>
        /// <param name="commandContext">Context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        public async Task ListUsers(CommandContext commandContext)
        {
            try
            {
                var members = await commandContext.Guild
                                                  .GetAllMembersAsync()
                                                  .ConfigureAwait(true);

                await ListEntries(commandContext, "Users", members.Select(obj => obj.Mention).OrderBy(obj => obj).ToList()).ConfigureAwait(false);
            }
            catch
            {
                await ListEntries(commandContext, "Users", commandContext.Guild.Members.Select(obj => obj.Value.Mention).OrderBy(obj => obj).ToList()).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// List entries
        /// </summary>
        /// <param name="commandContext">Context</param>
        /// <param name="description">Description</param>
        /// <param name="entries">Entries</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        private async Task ListEntries(CommandContext commandContext, string description, IEnumerable<string> entries)
        {
            var embedBuilder = new DiscordEmbedBuilder
                               {
                                   Color = DiscordColor.Green,
                                   Description = description
                               };

            var fieldCounter = 1;
            var stringBuilder = new StringBuilder();

            foreach (var entry in entries)
            {
                var currentLine = $"{entry} - {Formatter.InlineCode(entry)}\n";

                if (currentLine.Length + stringBuilder.Length > 1024)
                {
                    embedBuilder.AddField($"#{fieldCounter}", stringBuilder.ToString());

                    if (fieldCounter == 6)
                    {
                        fieldCounter = 1;

                        await commandContext.Channel
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

                    stringBuilder = new StringBuilder();
                }

                stringBuilder.Append(currentLine);
            }

            embedBuilder.AddField($"#{fieldCounter}", stringBuilder.ToString());

            await commandContext.Channel
                                .SendMessageAsync(embedBuilder)
                                .ConfigureAwait(false);
        }

        #endregion // Methods
    }
}
