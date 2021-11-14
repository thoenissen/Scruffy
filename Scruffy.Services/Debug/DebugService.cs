using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Account;
using Scruffy.Data.Entity.Repositories.General;
using Scruffy.Data.Enumerations.GuildWars2;
using Scruffy.Services.GuildWars2;
using Scruffy.Services.WebApi;

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
                var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(commandContext.Message.ReferencedMessage.Content));
                await using (memoryStream.ConfigureAwait(false))
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
        /// List commands
        /// </summary>
        /// <param name="commandContext">Context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        public async Task ListCommands(CommandContext commandContext)
        {
            IEnumerable<Command> GetCommands(IEnumerable<Command> commands)
            {
                foreach (var command in commands)
                {
                    yield return command;

                    if (command is CommandGroup commandGroup)
                    {
                        foreach (var subCommand in GetCommands(commandGroup.Children))
                        {
                            yield return subCommand;
                        }
                    }
                }
            }

            var commands = new List<string>();

            foreach (var command in GetCommands(commandContext.Client.GetCommandsNext().RegisteredCommands.Values))
            {
                if (commands.Contains(command.QualifiedName) == false
                 && (command is not CommandGroup
                  || command is CommandGroup { IsExecutableWithoutSubcommands: true })
                 && command.QualifiedName.StartsWith("debug") == false)
                {
                    commands.Add(command.QualifiedName);
                }
            }

            await ListEntries(commandContext, "Commands", commands, false).ConfigureAwait(false);
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
        /// <param name="isAddInline">Adding inline code</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        private async Task ListEntries(CommandContext commandContext, string description, IEnumerable<string> entries, bool isAddInline = true)
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
                var currentLine = isAddInline
                    ? $"{entry} - {Formatter.InlineCode(entry)}\n"
                    : $"{entry}\n";

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

        /// <summary>
        /// Posting a specific log entry
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <param name="id">Id</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task PostLogEntry(CommandContext commandContext, int id)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var logEntry = dbFactory.GetRepository<LogEntryRepository>()
                                        .GetQuery()
                                        .FirstOrDefault(obj => obj.Id == id);

                if (logEntry != null)
                {
                    var memoryStream = new MemoryStream();
                    await using (memoryStream.ConfigureAwait(false))
                    {
                        var writer = new StreamWriter(memoryStream);
                        await using (writer.ConfigureAwait(false))
                        {
                            await writer.WriteAsync(nameof(logEntry.Id))
                                        .ConfigureAwait(false);
                            await writer.WriteAsync(": ")
                                        .ConfigureAwait(false);
                            await writer.WriteAsync(logEntry.Id.ToString())
                                        .ConfigureAwait(false);
                            await writer.WriteAsync(Environment.NewLine)
                                        .ConfigureAwait(false);

                            await writer.WriteAsync(nameof(logEntry.TimeStamp))
                                        .ConfigureAwait(false);
                            await writer.WriteAsync(": ")
                                        .ConfigureAwait(false);
                            await writer.WriteAsync(logEntry.TimeStamp.ToString("G"))
                                        .ConfigureAwait(false);
                            await writer.WriteAsync(Environment.NewLine)
                                        .ConfigureAwait(false);

                            await writer.WriteAsync(nameof(logEntry.Type))
                                        .ConfigureAwait(false);
                            await writer.WriteAsync(": ")
                                        .ConfigureAwait(false);
                            await writer.WriteAsync(logEntry.Type.ToString())
                                        .ConfigureAwait(false);
                            await writer.WriteAsync(Environment.NewLine)
                                        .ConfigureAwait(false);

                            await writer.WriteAsync(nameof(logEntry.Source))
                                        .ConfigureAwait(false);
                            await writer.WriteAsync(": ")
                                        .ConfigureAwait(false);
                            await writer.WriteAsync(logEntry.Source)
                                        .ConfigureAwait(false);
                            await writer.WriteAsync(Environment.NewLine)
                                        .ConfigureAwait(false);

                            await writer.WriteAsync(nameof(logEntry.SubSource))
                                        .ConfigureAwait(false);
                            await writer.WriteAsync(": ")
                                        .ConfigureAwait(false);
                            await writer.WriteAsync(logEntry.SubSource)
                                        .ConfigureAwait(false);
                            await writer.WriteAsync(Environment.NewLine)
                                        .ConfigureAwait(false);

                            await writer.WriteAsync(nameof(logEntry.Message))
                                        .ConfigureAwait(false);
                            await writer.WriteAsync(": ")
                                        .ConfigureAwait(false);
                            await writer.WriteAsync(logEntry.Message)
                                        .ConfigureAwait(false);
                            await writer.WriteAsync(Environment.NewLine)
                                        .ConfigureAwait(false);

                            await writer.WriteAsync(nameof(logEntry.AdditionalInformation))
                                        .ConfigureAwait(false);
                            await writer.WriteAsync(": ")
                                        .ConfigureAwait(false);
                            await writer.WriteAsync(logEntry.AdditionalInformation)
                                        .ConfigureAwait(false);
                            await writer.WriteAsync(Environment.NewLine)
                                        .ConfigureAwait(false);

                            await writer.FlushAsync()
                                        .ConfigureAwait(false);

                            memoryStream.Position = 0;

                            await commandContext.Channel
                                                .SendMessageAsync(new DiscordMessageBuilder().WithFile("entry.txt", memoryStream))
                                                .ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    await commandContext.Message
                                        .RespondAsync("Unknown log entry")
                                        .ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Posting log overview
        /// </summary>
        /// <param name="channel">Channel</param>
        /// <param name="date">Date</param>
        /// <param name="suppressEmpty">Suppress empty overview</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task PostLogOverview(DiscordChannel channel, DateTime date, bool suppressEmpty)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var dayStart = date.Date;
                var dayEnd = date.Date.AddDays(1);

                var logEntries = dbFactory.GetRepository<LogEntryRepository>()
                                          .GetQuery()
                                          .Where(obj => obj.TimeStamp >= dayStart
                                                        && obj.TimeStamp < dayEnd)
                                          .ToList();

                if (suppressEmpty == false
                    || logEntries.Count > 0)
                {
                    var builder = new DiscordEmbedBuilder().WithTitle($"Log entries {date:yyyy-MM-dd}").WithColor(DiscordColor.Green)
                                                           .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/ef1f3e1f3f40100fb3750f8d7d25c657.png?size=64")
                                                           .WithTimestamp(DateTime.Now);

                    var types = logEntries.GroupBy(obj => obj.Type)
                                          .Select(obj => new
                                          {
                                              Type = obj.Key,
                                              Count = obj.Count()
                                          });

                    var stringBuilder = new StringBuilder();

                    foreach (var type in types.OrderBy(obj => obj.Type))
                    {
                        stringBuilder.AppendLine($"{type.Type}: {type.Count}");
                    }

                    if (stringBuilder.Length == 0)
                    {
                        stringBuilder.Append("\u200b");
                    }

                    builder.AddField("Types", stringBuilder.ToString());

                    var levels = logEntries.GroupBy(obj => obj.Level)
                                          .Select(obj => new
                                                         {
                                                             Level = obj.Key,
                                                             Count = obj.Count()
                                                         });

                    stringBuilder = new StringBuilder();

                    foreach (var level in levels.OrderBy(obj => obj.Level))
                    {
                        stringBuilder.AppendLine($"{level.Level}: {level.Count}");
                    }

                    if (stringBuilder.Length == 0)
                    {
                        stringBuilder.Append("\u200b");
                    }

                    builder.AddField("Levels", stringBuilder.ToString());

                    if (logEntries.Count > 0)
                    {
                        builder.AddField("IDs", $"{logEntries.Min(obj => obj.Id)} -> {logEntries.Max(obj => obj.Id)}");
                    }

                    await channel.SendMessageAsync(builder)
                                 .ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Refresh accounts
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RefreshAccount()
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                await dbFactory.GetRepository<AccountRepository>()
                               .RefreshRangeAsync(obj => obj.Permissions == GuildWars2ApiPermission.None,
                                             async obj =>
                                             {
                                                 var connector = new GuidWars2ApiConnector(obj.ApiKey);
                                                 await using (connector.ConfigureAwait(false))
                                                 {
                                                     var tokenInfo = await connector.GetTokenInformationAsync()
                                                                                    .ConfigureAwait(false);

                                                     obj.Permissions = GuildWars2ApiPermissionConverter.ToPermission(tokenInfo.Permissions);
                                                 }
                                             })
                               .ConfigureAwait(false);
            }
        }

        #endregion // Methods
    }
}
