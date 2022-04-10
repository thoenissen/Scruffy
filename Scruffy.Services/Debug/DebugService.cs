using System.IO;

using Discord;

using Scruffy.Data.Converter;
using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Account;
using Scruffy.Data.Entity.Repositories.General;
using Scruffy.Data.Entity.Repositories.Guild;
using Scruffy.Data.Entity.Tables.Guild;
using Scruffy.Data.Enumerations.GuildWars2;
using Scruffy.Services.Discord;
using Scruffy.Services.GuildWars2;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.Debug;

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
    public async Task DumpText(CommandContextContainer commandContext)
    {
        if (commandContext.Message.ReferencedMessage != null)
        {
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(commandContext.Message.ReferencedMessage.Content));
            await using (memoryStream.ConfigureAwait(false))
            {
                await commandContext.Channel
                                    .SendFileAsync(new FileAttachment(memoryStream, "dump.txt"), messageReference: new MessageReference(commandContext.Message.Id, commandContext.Channel?.Id, commandContext.Guild?.Id))
                                    .ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// List roles
    /// </summary>
    /// <param name="commandContext">Context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task ListRoles(CommandContextContainer commandContext)
    {
        await ListEntries(commandContext, "Roles", commandContext.Guild.Roles.Select(obj => obj.Mention).OrderBy(obj => obj)).ConfigureAwait(false);
    }

    /// <summary>
    /// List channels
    /// </summary>
    /// <param name="commandContext">Context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task ListChannels(CommandContextContainer commandContext)
    {
        var channels = await commandContext.Guild
                                           .GetChannelsAsync()
                                           .ConfigureAwait(false);

        await ListEntries(commandContext, "Channels", channels.Select(obj => MentionUtils.MentionChannel(obj.Id)).OrderBy(obj => obj)).ConfigureAwait(false);
    }

    /// <summary>
    /// List emojis
    /// </summary>
    /// <param name="commandContext">Context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task ListEmojis(CommandContextContainer commandContext)
    {
        await ListEntries(commandContext, "Emojis", commandContext.Guild.Emotes.Select(obj => obj.ToString()).OrderBy(obj => obj)).ConfigureAwait(false);
    }

    /// <summary>
    /// List emojis
    /// </summary>
    /// <param name="commandContext">Context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task ListUsers(CommandContextContainer commandContext)
    {
        var members = await commandContext.Guild
                                          .GetUsersAsync()
                                          .ConfigureAwait(true);

        await ListEntries(commandContext,
                          "Users",
                          members.Select(obj => obj.Mention)
                                 .OrderBy(obj => obj)
                                 .ToList()).ConfigureAwait(false);
    }

    /// <summary>
    /// List entries
    /// </summary>
    /// <param name="commandContext">Context</param>
    /// <param name="description">Description</param>
    /// <param name="entries">Entries</param>
    /// <param name="isAddInline">Adding inline code</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task ListEntries(CommandContextContainer commandContext, string description, IEnumerable<string> entries, bool isAddInline = true)
    {
        var embedBuilder = new EmbedBuilder
                           {
                               Color = Color.Green,
                               Description = description
                           };

        var fieldCounter = 1;
        var stringBuilder = new StringBuilder();

        foreach (var entry in entries)
        {
            var currentLine = isAddInline
                                  ? $"{entry} - {Format.Code(entry)}\n"
                                  : $"{entry}\n";

            if (currentLine.Length + stringBuilder.Length > 1024)
            {
                embedBuilder.AddField($"#{fieldCounter}", stringBuilder.ToString());

                if (fieldCounter == 5)
                {
                    fieldCounter = 1;

                    await commandContext.Channel
                                        .SendMessageAsync(embed: embedBuilder.Build())
                                        .ConfigureAwait(false);

                    embedBuilder = new EmbedBuilder
                                   {
                                       Color = Color.Green
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
                            .SendMessageAsync(embed: embedBuilder.Build())
                            .ConfigureAwait(false);
    }

    /// <summary>
    /// Posting a specific log entry
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <param name="id">Id</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task PostLogEntry(CommandContextContainer commandContext, int id)
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
                                            .SendFileAsync(new FileAttachment(memoryStream, "entry.txt"))
                                            .ConfigureAwait(false);
                    }
                }
            }
            else
            {
                await commandContext.Message
                                    .ReplyAsync("Unknown log entry")
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
    public async Task PostLogOverview(IMessageChannel channel, DateTime date, bool suppressEmpty)
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
                var builder = new EmbedBuilder().WithTitle($"Bot state report")
                                                .WithColor(Color.Green)
                                                .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                                                .WithTimestamp(DateTime.Now);

                var types = logEntries.GroupBy(obj => new
                                                      {
                                                          obj.Type,
                                                          obj.Level
                                                      })
                                      .Select(obj => new
                                                     {
                                                         GroupKey = obj.Key,
                                                         Count = obj.Count()
                                                     });

                var stringBuilder = new StringBuilder();

                foreach (var type in types.GroupBy(obj => obj.GroupKey.Type))
                {
                    stringBuilder.AppendLine(type.Key.ToString());

                    foreach (var level in type)
                    {
                        stringBuilder.AppendLine($" - {level.GroupKey.Level}: {level.Count}");
                    }
                }

                if (stringBuilder.Length > 0)
                {
                    builder.AddField($"Log entries  {date:yyyy-MM-dd}", $"```{Environment.NewLine}{stringBuilder}```");
                }

                stringBuilder = new StringBuilder();

                var today = DateTime.Today;

                foreach (var type in dbFactory.GetRepository<GuildLogEntryRepository>()
                                              .GetQuery()
                                              .Where(obj => obj.IsProcessed == false
                                                         && (obj.Type == GuildLogEntryEntity.Types.Stash
                                                          || obj.Type == GuildLogEntryEntity.Types.Upgrade)
                                                         && obj.Time < today)
                                              .GroupBy(obj => obj.Type)
                                              .Select(obj => new
                                                             {
                                                                 Type = obj.Key,
                                                                 Count = obj.Count()
                                                             }))
                {
                    stringBuilder.AppendLine($"{type.Type}: {type.Count}");
                }

                if (stringBuilder.Length > 0)
                {
                    builder.AddField("Unprocessed guild log entries", $"```{Environment.NewLine}{stringBuilder}```");
                }

                await channel.SendMessageAsync(embed: builder.Build())
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

                                                      obj.Permissions = GuildWars2ApiDataConverter.ToPermission(tokenInfo.Permissions);
                                                  }
                                              })
                           .ConfigureAwait(false);
        }
    }

    #endregion // Methods
}