using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Entity.Tables.Raid;
using Scruffy.Services.Core;
using Scruffy.Services.Core.Discord;

namespace Scruffy.Services.Raid
{
    /// <summary>
    /// Managing Roles
    /// </summary>
    public class RaidRolesService : LocatedServiceBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public RaidRolesService(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Starting the roles assistant
        /// </summary>
        /// <param name="commandContextContainer">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RunAssistantAsync(CommandContextContainer commandContextContainer)
        {
            var builder = new DiscordEmbedBuilder();
            builder.WithTitle(LocalizationGroup.GetText("AssistantTitle", "Raid role configuration"));
            builder.WithDescription(LocalizationGroup.GetText("AssistantDescription", "With this assistant you are able to configure the raid roles. The following roles are already created:"));

            var areRolesAvailable = false;

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var mainRoles = dbFactory.GetRepository<RaidRoleRepository>()
                                         .GetQuery()
                                         .Where(obj => obj.MainRoleId == null
                                                    && obj.IsDeleted == false)
                                         .Select(obj => new
                                                        {
                                                            obj.DiscordEmojiId,
                                                            obj.Description,
                                                            SubRoles = obj.SubRaidRoles
                                                                          .Where(obj2 => obj2.IsDeleted == false)
                                                                          .Select(obj2 => new
                                                                                          {
                                                                                              obj2.DiscordEmojiId,
                                                                                              obj2.Description
                                                                                          })
                                                        })
                                         .OrderBy(obj => obj.Description)
                                         .ToList();

                if (mainRoles.Count > 0)
                {
                    areRolesAvailable = true;

                    foreach (var role in mainRoles)
                    {
                        var roles = new StringBuilder(1024, 1024);

                        foreach (var subRole in role.SubRoles)
                        {
                            roles.Append("> ");
                            roles.Append(DiscordEmojiService.GetGuildEmoji(commandContextContainer.Client, subRole.DiscordEmojiId));
                            roles.Append(' ');
                            roles.Append(subRole.Description);
                            roles.Append('\n');
                        }

                        roles.Append('\n');

                        builder.AddField($"{DiscordEmojiService.GetGuildEmoji(commandContextContainer.Client, role.DiscordEmojiId)} {role.Description}", roles.ToString());
                    }
                }
            }

            var addEmoji = DiscordEmojiService.GetAddEmoji(commandContextContainer.Client);
            var editEmoji = DiscordEmojiService.GetEditEmoji(commandContextContainer.Client);
            var deleteEmoji = DiscordEmojiService.GetTrashCanEmoji(commandContextContainer.Client);
            var cancelEmoji = DiscordEmojiService.GetCrossEmoji(commandContextContainer.Client);

            var commands = new StringBuilder();
            commands.AppendLine(LocalizationGroup.GetFormattedText("AssistantAddCommand", "{0} Add role", addEmoji));

            if (areRolesAvailable)
            {
                commands.AppendLine(LocalizationGroup.GetFormattedText("AssistantEditCommand", "{0} Edit role", editEmoji));
                commands.AppendLine(LocalizationGroup.GetFormattedText("AssistantDeleteCommand", "{0} Delete role", deleteEmoji));
            }

            commands.AppendLine(LocalizationGroup.GetFormattedText("AssistantCancelCommand", "{0} Cancel", cancelEmoji));

            builder.AddField(LocalizationGroup.GetText("AssistantCommandsField", "Commands"), commands.ToString());

            var message = await commandContextContainer.Channel
                                              .SendMessageAsync(builder)
                                              .ConfigureAwait(false);

            var userReactionTask = commandContextContainer.Client
                                                 .GetInteractivity()
                                                 .WaitForReactionAsync(message, commandContextContainer.User);

            await message.CreateReactionAsync(addEmoji).ConfigureAwait(false);
            await message.CreateReactionAsync(editEmoji).ConfigureAwait(false);

            if (areRolesAvailable)
            {
                await message.CreateReactionAsync(deleteEmoji).ConfigureAwait(false);
                await message.CreateReactionAsync(cancelEmoji).ConfigureAwait(false);
            }

            var userReaction = await userReactionTask.ConfigureAwait(false);

            if (userReaction.TimedOut == false)
            {
                if (userReaction.Result.Emoji.Id == addEmoji.Id)
                {
                    await RunAddAssistantAsync(commandContextContainer).ConfigureAwait(false);
                }
                else if (userReaction.Result.Emoji.Id == editEmoji.Id && areRolesAvailable)
                {
                    await RunEditAssistantAsync(commandContextContainer).ConfigureAwait(false);
                }
                else if (userReaction.Result.Emoji.Id == deleteEmoji.Id)
                {
                    await RunDeleteAssistantAsync(commandContextContainer).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Starting the roles adding assistant
        /// </summary>
        /// <param name="commandContextContainer">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task RunAddAssistantAsync(CommandContextContainer commandContextContainer)
        {
            var currentBotMessage = await commandContextContainer.Channel
                                                        .SendMessageAsync(LocalizationGroup.GetText("ReactWithEmojiPrompt", "Please react with emoji which should be assigned to the role."))
                                                        .ConfigureAwait(false);

            var interactivity = commandContextContainer.Client
                                              .GetInteractivity();

            var reaction = await interactivity.WaitForReactionAsync(currentBotMessage, commandContextContainer.User)
                                              .ConfigureAwait(false);

            if (reaction.TimedOut == false && reaction.Result.Emoji.Id > 0)
            {
                var roleData = new RaidRoleEntity
                                   {
                                       DiscordEmojiId = reaction.Result.Emoji.Id
                                   };

                currentBotMessage = await commandContextContainer.Channel
                                                        .SendMessageAsync(LocalizationGroup.GetText("DescriptionPrompt", "Please enter the description of the role."))
                                                        .ConfigureAwait(false);

                var currentUserResponse = await interactivity.WaitForMessageAsync(obj => obj.Author.Id == commandContextContainer.User.Id
                                                                                      && obj.ChannelId == commandContextContainer.Channel.Id)
                                                             .ConfigureAwait(false);

                if (currentUserResponse.TimedOut == false)
                {
                    roleData.Description = currentUserResponse.Result.Content;

                    var continueCreation = true;
                    var subRolesData = new List<RaidRoleEntity>();
                    var checkEmoji = DiscordEmojiService.GetCheckEmoji(commandContextContainer.Client);
                    var crossEmoji = DiscordEmojiService.GetCrossEmoji(commandContextContainer.Client);

                    var addSubRoles = true;
                    while (addSubRoles)
                    {
                        var promptText = subRolesData.Count == 0
                                             ? LocalizationGroup.GetText("AddSubRolesPrompt", "Do you want to add sub roles?")
                                             : LocalizationGroup.GetText("AddAdditionalSubRolesPrompt", "Do you want to add additional sub roles?");

                        currentBotMessage = await commandContextContainer.Channel
                                                                .SendMessageAsync(promptText)
                                                                .ConfigureAwait(false);

                        var userReactionTask = commandContextContainer.Client
                                                             .GetInteractivity()
                                                             .WaitForReactionAsync(currentBotMessage, commandContextContainer.User);

                        await currentBotMessage.CreateReactionAsync(checkEmoji).ConfigureAwait(false);
                        await currentBotMessage.CreateReactionAsync(crossEmoji).ConfigureAwait(false);

                        var userReaction = await userReactionTask.ConfigureAwait(false);
                        if (userReaction.TimedOut == false)
                        {
                            if (userReaction.Result.Emoji.Id == checkEmoji.Id)
                            {
                                currentBotMessage = await commandContextContainer.Channel
                                                                        .SendMessageAsync(LocalizationGroup.GetText("ReactWithEmojiPrompt", "Please react with emoji which should be assigned to the role."))
                                                                        .ConfigureAwait(false);

                                reaction = await interactivity.WaitForReactionAsync(currentBotMessage, commandContextContainer.User)
                                                              .ConfigureAwait(false);

                                if (reaction.TimedOut == false && reaction.Result.Emoji.Id > 0)
                                {
                                    var subRoleData = new RaidRoleEntity
                                                      {
                                                          DiscordEmojiId = reaction.Result.Emoji.Id
                                                      };

                                    currentBotMessage = await commandContextContainer.Channel
                                                                            .SendMessageAsync(LocalizationGroup.GetText("DescriptionPrompt", "Please enter the description of the role."))
                                                                            .ConfigureAwait(false);

                                    currentUserResponse = await interactivity.WaitForMessageAsync(obj => obj.Author.Id == commandContextContainer.User.Id
                                                                                                      && obj.ChannelId == commandContextContainer.Channel.Id)
                                                                             .ConfigureAwait(false);

                                    if (currentUserResponse.TimedOut == false)
                                    {
                                        subRoleData.Description = currentUserResponse.Result.Content;

                                        subRolesData.Add(subRoleData);
                                    }
                                    else
                                    {
                                        continueCreation = false;
                                    }
                                }
                                else
                                {
                                    continueCreation = false;
                                }
                            }
                            else
                            {
                                addSubRoles = false;
                            }
                        }
                        else
                        {
                            continueCreation = false;
                        }
                    }

                    if (continueCreation)
                    {
                        using (var dbFactory = RepositoryFactory.CreateInstance())
                        {
                            if (dbFactory.GetRepository<RaidRoleRepository>()
                                         .Add(roleData))
                            {
                                foreach (var subRole in subRolesData)
                                {
                                    subRole.MainRoleId = roleData.Id;

                                    dbFactory.GetRepository<RaidRoleRepository>()
                                             .Add(subRole);
                                }
                            }
                        }

                        await commandContextContainer.Channel
                                            .SendMessageAsync(LocalizationGroup.GetText("CreationCompletedMessage", "The creation is completed."))
                                            .ConfigureAwait(false);
                    }
                }
            }
        }

        /// <summary>
        /// Starting the role editing assistant
        /// </summary>
        /// <param name="commandContextContainer">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task RunEditAssistantAsync(CommandContextContainer commandContextContainer)
        {
            var roleId = await SelectRoleAsync(commandContextContainer, null).ConfigureAwait(false);
            if (roleId != null)
            {
                var builder = new DiscordEmbedBuilder();
                builder.WithTitle(LocalizationGroup.GetText("RoleEditTitle", "Raid role configuration"));

                var areRolesAvailable = false;

                using (var dbFactory = RepositoryFactory.CreateInstance())
                {
                    var rolesFieldText = new StringBuilder();
                    var roles = dbFactory.GetRepository<RaidRoleRepository>()
                                         .GetQuery()
                                         .Where(obj => obj.Id == roleId.Value
                                                    && obj.IsDeleted == false)
                                         .Select(obj => new
                                                        {
                                                            obj.DiscordEmojiId,
                                                            obj.Description,
                                                            SubRoles = obj.SubRaidRoles
                                                                          .Where(obj2 => obj2.IsDeleted == false)
                                                                          .Select(obj2 => new
                                                                                          {
                                                                                              obj2.DiscordEmojiId,
                                                                                              obj2.Description
                                                                                          })
                                                        })
                                         .OrderBy(obj => obj.Description)
                                         .First();

                    builder.WithDescription($"{DiscordEmoji.FromGuildEmote(commandContextContainer.Client, roles.DiscordEmojiId)} - {roles.Description}");

                    if (roles.SubRoles.Any())
                    {
                        areRolesAvailable = true;

                        foreach (var role in roles.SubRoles)
                        {
                            rolesFieldText.Append(DiscordEmoji.FromGuildEmote(commandContextContainer.Client, role.DiscordEmojiId));
                            rolesFieldText.Append(" - ");
                            rolesFieldText.Append(role.Description);
                            rolesFieldText.Append('\n');
                        }

                        builder.AddField(LocalizationGroup.GetText("AssistantRolesField", "Roles"), rolesFieldText.ToString());
                    }
                }

                var addSubRoleEmoji = DiscordEmojiService.GetAddEmoji(commandContextContainer.Client);
                var descriptionEmoji = DiscordEmojiService.GetEditEmoji(commandContextContainer.Client);
                var editSubRoleEmoji = DiscordEmojiService.GetEdit2Emoji(commandContextContainer.Client);
                var emojiEmoji = DiscordEmojiService.GetEmojiEmoji(commandContextContainer.Client);
                var deleteSubRoleEmoji = DiscordEmojiService.GetTrashCanEmoji(commandContextContainer.Client);
                var cancelEmoji = DiscordEmojiService.GetCrossEmoji(commandContextContainer.Client);

                var commands = new StringBuilder();
                commands.AppendLine(LocalizationGroup.GetFormattedText("RoleEditEditDescriptionCommand", "{0} Edit description", descriptionEmoji));
                commands.AppendLine(LocalizationGroup.GetFormattedText("RoleEditEditEmojiCommand", "{0} Edit emoji", emojiEmoji));
                commands.AppendLine(LocalizationGroup.GetFormattedText("RoleEditAddSubRoleCommand", "{0} Add sub role", addSubRoleEmoji));

                if (areRolesAvailable)
                {
                    commands.AppendLine(LocalizationGroup.GetFormattedText("RoleEditEditSubRoleCommand", "{0} Edit sub role", editSubRoleEmoji));
                    commands.AppendLine(LocalizationGroup.GetFormattedText("RoleEditDeleteSubRoleCommand", "{0} Delete sub role", deleteSubRoleEmoji));
                }

                commands.AppendLine(LocalizationGroup.GetFormattedText("AssistantCancelCommand", "{0} Cancel", cancelEmoji));

                builder.AddField(LocalizationGroup.GetText("AssistantCommandsField", "Commands"), commands.ToString());

                var message = await commandContextContainer.Channel
                                                  .SendMessageAsync(builder)
                                                  .ConfigureAwait(false);

                var userReactionTask = commandContextContainer.Client
                                                     .GetInteractivity()
                                                     .WaitForReactionAsync(message, commandContextContainer.User);

                await message.CreateReactionAsync(descriptionEmoji).ConfigureAwait(false);
                await message.CreateReactionAsync(emojiEmoji).ConfigureAwait(false);
                await message.CreateReactionAsync(addSubRoleEmoji).ConfigureAwait(false);

                if (areRolesAvailable)
                {
                    await message.CreateReactionAsync(editSubRoleEmoji).ConfigureAwait(false);
                    await message.CreateReactionAsync(deleteSubRoleEmoji).ConfigureAwait(false);
                }

                await message.CreateReactionAsync(cancelEmoji).ConfigureAwait(false);

                var userReaction = await userReactionTask.ConfigureAwait(false);
                if (userReaction.TimedOut == false)
                {
                    if (userReaction.Result.Emoji.Id == addSubRoleEmoji.Id)
                    {
                        await RunAddSubRoleAssistantAsync(commandContextContainer, roleId.Value).ConfigureAwait(false);
                    }
                    else if (userReaction.Result.Emoji.Id == descriptionEmoji.Id)
                    {
                        await RunEditDescriptionAssistantAsync(commandContextContainer, roleId.Value).ConfigureAwait(false);
                    }
                    else if (userReaction.Result.Emoji.Id == editSubRoleEmoji.Id)
                    {
                        await RunEditSubRoleAssistantAsync(commandContextContainer, roleId.Value).ConfigureAwait(false);
                    }
                    else if (userReaction.Result.Emoji.Id == emojiEmoji.Id)
                    {
                        await RunEditEmojiAssistantAsync(commandContextContainer, roleId.Value).ConfigureAwait(false);
                    }
                    else if (userReaction.Result.Emoji.Id == deleteSubRoleEmoji.Id)
                    {
                        await RunDeleteSubRoleAssistantAsync(commandContextContainer, roleId.Value).ConfigureAwait(false);
                    }
                }
            }
        }

        /// <summary>
        /// Editing the description of the role
        /// </summary>
        /// <param name="commandContextContainer">Current command context</param>
        /// <param name="roleId">Id of the role</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task RunEditDescriptionAssistantAsync(CommandContextContainer commandContextContainer, long roleId)
        {
            await commandContextContainer.Channel
                                .SendMessageAsync(LocalizationGroup.GetText("DescriptionPrompt", "Please enter the description of the role."))
                                .ConfigureAwait(false);

            var response = await commandContextContainer.Client
                                               .GetInteractivity()
                                               .WaitForMessageAsync(obj => obj.ChannelId == commandContextContainer.Channel.Id
                                                                        && obj.Author.Id == commandContextContainer.Member.Id)
                                               .ConfigureAwait(false);

            if (response.TimedOut == false)
            {
                using (var dbFactory = RepositoryFactory.CreateInstance())
                {
                    dbFactory.GetRepository<RaidRoleRepository>()
                             .Refresh(obj => obj.Id == roleId,
                                      obj => obj.Description = response.Result.Content);
                }
            }
        }

        /// <summary>
        /// Editing the emoji of the role
        /// </summary>
        /// <param name="commandContextContainer">Current command context</param>
        /// <param name="roleId">Id of the role</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task RunEditEmojiAssistantAsync(CommandContextContainer commandContextContainer, long roleId)
        {
            var message = await commandContextContainer.Channel
                                              .SendMessageAsync(LocalizationGroup.GetText("ReactWithEmojiPrompt", "Please react with emoji which should be assigned to the role."))
                                              .ConfigureAwait(false);

            var response = await commandContextContainer.Client
                                               .GetInteractivity()
                                               .WaitForReactionAsync(message, commandContextContainer.Member)
                                               .ConfigureAwait(false);

            if (response.TimedOut == false)
            {
                using (var dbFactory = RepositoryFactory.CreateInstance())
                {
                    dbFactory.GetRepository<RaidRoleRepository>()
                             .Refresh(obj => obj.Id == roleId,
                                      obj => obj.DiscordEmojiId = response.Result.Emoji.Id);
                }
            }
        }

        /// <summary>
        /// Editing a new sub role
        /// </summary>
        /// <param name="commandContextContainer">Current command context</param>
        /// <param name="mainRoleId">Id of the role</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task RunAddSubRoleAssistantAsync(CommandContextContainer commandContextContainer, long mainRoleId)
        {
            var interactivity = commandContextContainer.Client.GetInteractivity();

            var currentBotMessage = await commandContextContainer.Channel
                                        .SendMessageAsync(LocalizationGroup.GetText("ReactWithEmojiPrompt", "Please react with emoji which should be assigned to the role."))
                                        .ConfigureAwait(false);

            var reaction = await interactivity.WaitForReactionAsync(currentBotMessage, commandContextContainer.User)
                                              .ConfigureAwait(false);

            if (reaction.TimedOut == false && reaction.Result.Emoji.Id > 0)
            {
                var subRoleData = new RaidRoleEntity
                                  {
                                      MainRoleId = mainRoleId,
                                      DiscordEmojiId = reaction.Result.Emoji.Id
                                  };

                currentBotMessage = await commandContextContainer.Channel
                                                        .SendMessageAsync(LocalizationGroup.GetText("DescriptionPrompt", "Please enter the description of the role."))
                                                        .ConfigureAwait(false);

                var currentUserResponse = await interactivity.WaitForMessageAsync(obj => obj.Author.Id == commandContextContainer.User.Id
                                                                                  && obj.ChannelId == commandContextContainer.Channel.Id)
                                                         .ConfigureAwait(false);

                if (currentUserResponse.TimedOut == false)
                {
                    subRoleData.Description = currentUserResponse.Result.Content;

                    using (var dbFactory = RepositoryFactory.CreateInstance())
                    {
                        dbFactory.GetRepository<RaidRoleRepository>()
                                 .Add(subRoleData);
                    }
                }
            }
        }

        /// <summary>
        /// Editing a sub role
        /// </summary>
        /// <param name="commandContextContainer">Current command context</param>
        /// <param name="mainRoleId">Id of the role</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task RunEditSubRoleAssistantAsync(CommandContextContainer commandContextContainer, long mainRoleId)
        {
            var roleId = await SelectRoleAsync(commandContextContainer, mainRoleId).ConfigureAwait(false);
            if (roleId != null)
            {
                var builder = new DiscordEmbedBuilder();
                builder.WithTitle(LocalizationGroup.GetText("RoleEditTitle", "Raid role configuration"));

                var descriptionEmoji = DiscordEmojiService.GetEditEmoji(commandContextContainer.Client);
                var emojiEmoji = DiscordEmojiService.GetEmojiEmoji(commandContextContainer.Client);
                var cancelEmoji = DiscordEmojiService.GetCrossEmoji(commandContextContainer.Client);

                var commands = new StringBuilder();
                commands.AppendLine(LocalizationGroup.GetFormattedText("RoleEditEditDescriptionCommand", "{0} Edit description", descriptionEmoji));
                commands.AppendLine(LocalizationGroup.GetFormattedText("RoleEditEditEmojiCommand", "{0} Edit emoji", emojiEmoji));
                commands.AppendLine(LocalizationGroup.GetFormattedText("AssistantCancelCommand", "{0} Cancel", cancelEmoji));

                builder.AddField(LocalizationGroup.GetText("AssistantCommandsField", "Commands"), commands.ToString());

                var message = await commandContextContainer.Channel
                                                  .SendMessageAsync(builder)
                                                  .ConfigureAwait(false);

                var userReactionTask = commandContextContainer.Client
                                                     .GetInteractivity()
                                                     .WaitForReactionAsync(message, commandContextContainer.User);

                await message.CreateReactionAsync(descriptionEmoji).ConfigureAwait(false);
                await message.CreateReactionAsync(emojiEmoji).ConfigureAwait(false);
                await message.CreateReactionAsync(cancelEmoji).ConfigureAwait(false);

                var userReaction = await userReactionTask.ConfigureAwait(false);
                if (userReaction.TimedOut == false)
                {
                    if (userReaction.Result.Emoji.Id == descriptionEmoji.Id)
                    {
                        await RunEditDescriptionAssistantAsync(commandContextContainer, roleId.Value).ConfigureAwait(false);
                    }
                    else if (userReaction.Result.Emoji.Id == emojiEmoji.Id)
                    {
                        await RunEditEmojiAssistantAsync(commandContextContainer, roleId.Value).ConfigureAwait(false);
                    }
                }
            }
        }

        /// <summary>
        /// Deletion of a sub role
        /// </summary>
        /// <param name="commandContextContainer">Current command context</param>
        /// <param name="mainRoleId">Id of the role</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task RunDeleteSubRoleAssistantAsync(CommandContextContainer commandContextContainer, long mainRoleId)
        {
            var roleId = await SelectRoleAsync(commandContextContainer, mainRoleId).ConfigureAwait(false);
            if (roleId != null)
            {
                var checkEmoji = DiscordEmojiService.GetCheckEmoji(commandContextContainer.Client);
                var crossEmoji = DiscordEmojiService.GetCrossEmoji(commandContextContainer.Client);

                var message = await commandContextContainer.Channel
                                                  .SendMessageAsync(LocalizationGroup.GetText("DeleteRolePrompt", "Are you sure you want to delete the role?"))
                                                  .ConfigureAwait(false);

                var userReactionTask = commandContextContainer.Client
                                                     .GetInteractivity()
                                                     .WaitForReactionAsync(message, commandContextContainer.User);

                await message.CreateReactionAsync(checkEmoji).ConfigureAwait(false);
                await message.CreateReactionAsync(crossEmoji).ConfigureAwait(false);

                var userReaction = await userReactionTask.ConfigureAwait(false);
                if (userReaction.TimedOut == false)
                {
                    using (var dbFactory = RepositoryFactory.CreateInstance())
                    {
                        dbFactory.GetRepository<RaidRoleRepository>()
                                 .Refresh(obj => obj.Id == roleId.Value,
                                          obj => obj.IsDeleted = true);
                    }
                }
            }
        }

        /// <summary>
        /// Starting the role deletion assistant
        /// </summary>
        /// <param name="commandContextContainer">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task RunDeleteAssistantAsync(CommandContextContainer commandContextContainer)
        {
            var roleId = await SelectRoleAsync(commandContextContainer, null).ConfigureAwait(false);
            if (roleId != null)
            {
                var checkEmoji = DiscordEmojiService.GetCheckEmoji(commandContextContainer.Client);
                var crossEmoji = DiscordEmojiService.GetCrossEmoji(commandContextContainer.Client);

                var message = await commandContextContainer.Channel
                                                  .SendMessageAsync(LocalizationGroup.GetText("DeleteRolePrompt", "Are you sure you want to delete the role?"))
                                                  .ConfigureAwait(false);

                var userReactionTask = commandContextContainer.Client
                                                     .GetInteractivity()
                                                     .WaitForReactionAsync(message, commandContextContainer.User);

                await message.CreateReactionAsync(checkEmoji).ConfigureAwait(false);
                await message.CreateReactionAsync(crossEmoji).ConfigureAwait(false);

                var userReaction = await userReactionTask.ConfigureAwait(false);
                if (userReaction.TimedOut == false)
                {
                    using (var dbFactory = RepositoryFactory.CreateInstance())
                    {
                        dbFactory.GetRepository<RaidRoleRepository>()
                                 .Refresh(obj => obj.Id == roleId.Value,
                                          obj => obj.IsDeleted = true);
                    }
                }
            }
        }

        /// <summary>
        /// Selection a role
        /// </summary>
        /// <param name="commandContextContainer">Current command context</param>
        /// <param name="mainRoleId">Id of the main role</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task<long?> SelectRoleAsync(CommandContextContainer commandContextContainer, long? mainRoleId)
        {
            long? roleId = null;

            var builder = new DiscordEmbedBuilder();
            builder.WithTitle(LocalizationGroup.GetText("ChooseRoleTitle", "Raid role selection"));
            builder.WithDescription(LocalizationGroup.GetText("ChooseRoleDescription", "Please choose one of the following roles:"));

            var roles = new Dictionary<int, long>();
            var rolesFieldText = new StringBuilder();

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                var mainRoles = dbFactory.GetRepository<RaidRoleRepository>()
                                         .GetQuery()
                                         .Where(obj => obj.MainRoleId == mainRoleId
                                                    && obj.IsDeleted == false)
                                         .Select(obj => new
                                                        {
                                                            obj.Id,
                                                            obj.DiscordEmojiId,
                                                            obj.Description
                                                        })
                                         .OrderBy(obj => obj.Description)
                                         .ToList();

                var i = 1;
                foreach (var role in mainRoles)
                {
                    rolesFieldText.Append('`');
                    rolesFieldText.Append(i);
                    rolesFieldText.Append("` - ");
                    rolesFieldText.Append(DiscordEmoji.FromGuildEmote(commandContextContainer.Client, role.DiscordEmojiId));
                    rolesFieldText.Append(' ');
                    rolesFieldText.Append(role.Description);
                    rolesFieldText.Append('\n');

                    roles[i] = role.Id;

                    i++;
                }

                builder.AddField(LocalizationGroup.GetText("AssistantRolesField", "Roles"), rolesFieldText.ToString());
            }

            await commandContextContainer.Channel
                                .SendMessageAsync(builder)
                                .ConfigureAwait(false);

            var currentUserResponse = await commandContextContainer.Client
                                                          .GetInteractivity()
                                                          .WaitForMessageAsync(obj => obj.Author.Id == commandContextContainer.User.Id
                                                                                   && obj.ChannelId == commandContextContainer.Channel.Id)
                                                          .ConfigureAwait(false);

            if (currentUserResponse.TimedOut == false
             && int.TryParse(currentUserResponse.Result.Content, out var index)
             && roles.TryGetValue(index, out var selectedRoleId))
            {
                roleId = selectedRoleId;
            }

            return roleId;
        }

        #endregion // Methods
    }
}
