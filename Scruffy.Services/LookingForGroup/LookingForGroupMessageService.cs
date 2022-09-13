﻿using Discord;
using Discord.WebSocket;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;
using Scruffy.Data.Entity.Repositories.LookingForGroup;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.LookingForGroup
{
    /// <summary>
    /// Looking for group message service
    /// </summary>
    public class LookingForGroupMessageService : LocatedServiceBase
    {
        #region Fields

        /// <summary>
        /// Discord client
        /// </summary>
        private readonly DiscordSocketClient _discordClient;

        /// <summary>
        /// Repository factory
        /// </summary>
        private readonly RepositoryFactory _repositoryFactory;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        /// <param name="discordClient">Discord client</param>
        /// <param name="repositoryFactory">Repository factory</param>
        public LookingForGroupMessageService(LocalizationService localizationService,
                                             DiscordSocketClient discordClient,
                                             RepositoryFactory repositoryFactory)
            : base(localizationService)
        {
            _discordClient = discordClient;
            _repositoryFactory = repositoryFactory;
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Refresh message
        /// </summary>
        /// <param name="appointmentId">Id of the appointment</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        public async Task RefreshMessage(int appointmentId)
        {
            var discordAccountQuery = _repositoryFactory.GetRepository<DiscordAccountRepository>()
                                                        .GetQuery()
                                                        .Select(obj => obj);

            var appointmentData = _repositoryFactory.GetRepository<LookingForGroupAppointmentRepository>()
                                                    .GetQuery()
                                                    .Where(obj => obj.Id == appointmentId)
                                                    .Select(obj => new
                                                                   {
                                                                       obj.ChannelId,
                                                                       obj.MessageId,
                                                                       obj.Title,
                                                                       obj.Description,
                                                                       obj.ThreadId,
                                                                       Participants = obj.Participants
                                                                                         .Select(obj2 => new
                                                                                                         {
                                                                                                             obj2.RegistrationTimeStamp,
                                                                                                             UserId = discordAccountQuery.Where(obj3 => obj3.UserId == obj2.UserId)
                                                                                                                                         .Select(obj3 => obj3.Id)
                                                                                                                                         .FirstOrDefault()
                                                                                                         })
                                                                   })
                                                    .FirstOrDefault();

            if (appointmentData != null)
            {
                var channel = await _discordClient.GetChannelAsync(appointmentData.ChannelId)
                                                  .ConfigureAwait(false);

                if (channel is ITextChannel textChannel)
                {
                    var message = await textChannel.GetMessageAsync(appointmentData.MessageId)
                                                   .ConfigureAwait(false);

                    if (message is IUserMessage userMessage)
                    {
                        var embedBuilder = new EmbedBuilder().WithTitle("LFG: " + appointmentData.Title)
                                                             .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                                                             .WithColor(Color.Green)
                                                             .WithTimestamp(DateTime.Now)
                                                             .WithThumbnailUrl("https://cdn.discordapp.com/attachments/982003562462724186/1018981799759716432/unknown.png");

                        if (string.IsNullOrWhiteSpace(appointmentData.Description) == false)
                        {
                            embedBuilder.WithDescription(appointmentData.Description);
                        }

                        var participantsBuilder = new StringBuilder();

                        foreach (var participant in appointmentData.Participants.OrderBy(obj => obj.RegistrationTimeStamp))
                        {
                            participantsBuilder.AppendLine("> " + _discordClient.GetUser(participant.UserId).Mention);
                        }

                        if (participantsBuilder.Length == 0)
                        {
                            participantsBuilder.Append(">  \u200b");
                        }

                        embedBuilder.AddField(LocalizationGroup.GetText("Participants", "Participants"), participantsBuilder.ToString());

                        var componentsBuilder = new ComponentBuilder();

                        componentsBuilder.WithButton(LocalizationGroup.GetText("Join", "Join"),
                                                     InteractivityService.GetPermanentCustomId("lfg", "join", appointmentId.ToString()),
                                                     ButtonStyle.Secondary,
                                                     DiscordEmoteService.GetCheckEmote(_discordClient));
                        componentsBuilder.WithButton(LocalizationGroup.GetText("Leave", "Leave"),
                                                     InteractivityService.GetPermanentCustomId("lfg", "leave", appointmentId.ToString()),
                                                     ButtonStyle.Secondary,
                                                     DiscordEmoteService.GetCrossEmote(_discordClient));
                        componentsBuilder.WithButton(LocalizationGroup.GetText("Thread", "Thread"),
                                                     style: ButtonStyle.Link,
                                                     url: $"https://discord.com/channels/{textChannel.GuildId}/{appointmentData.ThreadId}/");
                        componentsBuilder.WithButton(null,
                                                     InteractivityService.GetPermanentCustomId("lfg", "configuration", appointmentId.ToString()),
                                                     ButtonStyle.Secondary,
                                                     new Emoji("⚙️"));

                        await userMessage.ModifyAsync(obj =>
                                                      {
                                                          obj.Content = string.Empty;
                                                          obj.Embed = embedBuilder.Build();
                                                          obj.Components = componentsBuilder.Build();
                                                      })
                                         .ConfigureAwait(false);
                    }
                }
            }
        }

        #endregion // Methods
    }
}