using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Entity.Tables.Raid;
using Scruffy.Services.Core;

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
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RunAssistantAsync(CommandContext commandContext)
        {
            var builder = new DiscordEmbedBuilder();
            builder.WithTitle(LocalizationGroup.GetText("AssistantTitle", "Raid role configuration"));
            builder.WithDescription(LocalizationGroup.GetText("AssistantDescription", "With this assistant you are able to configure the raid roles. The following roles are already created:"));

            var roles = new StringBuilder();
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
                        roles.Append(DiscordEmoji.FromGuildEmote(commandContext.Client, role.DiscordEmojiId));
                        roles.Append(" - ");
                        roles.Append(role.Description);
                        roles.Append('\n');

                        foreach (var subRole in role.SubRoles)
                        {
                            roles.Append(" ● ");
                            roles.Append(DiscordEmoji.FromGuildEmote(commandContext.Client, subRole.DiscordEmojiId));
                            roles.Append(" - ");
                            roles.Append(subRole.Description);
                            roles.Append('\n');
                        }
                    }
                    roles.Append('\u200B');

                    builder.AddField(LocalizationGroup.GetText("AssistantRolesField", "Roles"), roles.ToString());
                }
            }

            var addEmoji = DiscordEmojiService.GetAddEmoji(commandContext.Client);
            var editEmoji = DiscordEmojiService.GetEditEmoji(commandContext.Client);
            var deleteEmoji = DiscordEmojiService.GetTrashCanEmoji(commandContext.Client);
            var cancelEmoji = DiscordEmojiService.GetCrossEmoji(commandContext.Client);

            var commands = new StringBuilder();
            commands.AppendLine(LocalizationGroup.GetFormattedText("AssistantAddCommand", "{0} Add role", addEmoji));

            if (areRolesAvailable)
            {
                commands.AppendLine(LocalizationGroup.GetFormattedText("AssistantEditCommand", "{0} Edit role", editEmoji));
                commands.AppendLine(LocalizationGroup.GetFormattedText("AssistantDeleteCommand", "{0} Delete role", deleteEmoji));
            }

            commands.AppendLine(LocalizationGroup.GetFormattedText("AssistantCancelCommand", "{0} Cancel", cancelEmoji));

            builder.AddField(LocalizationGroup.GetText("AssistantCommandsField", "Commands"), commands.ToString());

            var message = await commandContext.Channel
                                              .SendMessageAsync(builder)
                                              .ConfigureAwait(false);

            var userReactionTask = commandContext.Client
                                                 .GetInteractivity()
                                                 .WaitForReactionAsync(message, commandContext.User);

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
                    await RunAddAssistantAsync(commandContext).ConfigureAwait(false);
                }
                else if (userReaction.Result.Emoji.Id == editEmoji.Id && areRolesAvailable)
                {
                    await RunEditAssistantAsync(commandContext).ConfigureAwait(false);
                }
                else if (userReaction.Result.Emoji.Id == deleteEmoji.Id)
                {
                    await RunDeleteAssistantAsync(commandContext).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Starting the roles adding assistant
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task RunAddAssistantAsync(CommandContext commandContext)
        {
            var currentBotMessage = await commandContext.Channel
                                                        .SendMessageAsync(LocalizationGroup.GetText("ReactWithEmojiPrompt", "Please react with emoji which should be assigned to the role."))
                                                        .ConfigureAwait(false);

            var interactivity = commandContext.Client
                                              .GetInteractivity();

            var reaction = await interactivity.WaitForReactionAsync(currentBotMessage, commandContext.User)
                                              .ConfigureAwait(false);

            if (reaction.TimedOut == false && reaction.Result.Emoji.Id > 0)
            {
                var roleData = new RaidRoleEntity
                                   {
                                       DiscordEmojiId = reaction.Result.Emoji.Id
                                   };

                currentBotMessage = await commandContext.Channel
                                                        .SendMessageAsync(LocalizationGroup.GetText("DescriptionPrompt", "Please enter the description of the role."))
                                                        .ConfigureAwait(false);

                var currentUserResponse = await interactivity.WaitForMessageAsync(obj => obj.Author.Id == commandContext.User.Id
                                                                                      && obj.ChannelId == commandContext.Channel.Id)
                                                             .ConfigureAwait(false);

                if (currentUserResponse.TimedOut == false)
                {
                    roleData.Description = currentUserResponse.Result.Content;

                    var continueCreation = true;
                    var subRolesData = new List<RaidRoleEntity>();
                    var checkEmoji = DiscordEmojiService.GetCheckEmoji(commandContext.Client);
                    var crossEmoji = DiscordEmojiService.GetCrossEmoji(commandContext.Client);

                    var addSubRoles = true;
                    while (addSubRoles)
                    {
                        var promptText = subRolesData.Count == 0
                                             ? LocalizationGroup.GetText("AddSubRolesPrompt", "Do you want to add sub roles?")
                                             : LocalizationGroup.GetText("AddAdditionalSubRolesPrompt", "Do you want to add additional sub roles?");

                        currentBotMessage = await commandContext.Channel
                                                                .SendMessageAsync(promptText)
                                                                .ConfigureAwait(false);

                        var userReactionTask = commandContext.Client
                                                             .GetInteractivity()
                                                             .WaitForReactionAsync(currentBotMessage, commandContext.User);

                        await currentBotMessage.CreateReactionAsync(checkEmoji).ConfigureAwait(false);
                        await currentBotMessage.CreateReactionAsync(crossEmoji).ConfigureAwait(false);

                        var userReaction = await userReactionTask.ConfigureAwait(false);
                        if (userReaction.TimedOut == false)
                        {
                            if (userReaction.Result.Emoji.Id == checkEmoji.Id)
                            {
                                currentBotMessage = await commandContext.Channel
                                                                        .SendMessageAsync(LocalizationGroup.GetText("ReactWithEmojiPrompt", "Please react with emoji which should be assigned to the role."))
                                                                        .ConfigureAwait(false);

                                reaction = await interactivity.WaitForReactionAsync(currentBotMessage, commandContext.User)
                                                              .ConfigureAwait(false);

                                if (reaction.TimedOut == false && reaction.Result.Emoji.Id > 0)
                                {
                                    var subRoleData = new RaidRoleEntity
                                                      {
                                                          DiscordEmojiId = reaction.Result.Emoji.Id
                                                      };

                                    currentBotMessage = await commandContext.Channel
                                                                            .SendMessageAsync(LocalizationGroup.GetText("DescriptionPrompt", "Please enter the description of the role."))
                                                                            .ConfigureAwait(false);

                                    currentUserResponse = await interactivity.WaitForMessageAsync(obj => obj.Author.Id == commandContext.User.Id
                                                                                                      && obj.ChannelId == commandContext.Channel.Id)
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

                        await commandContext.Channel
                                            .SendMessageAsync(LocalizationGroup.GetText("CreationCompletedMessage", "The creation is completed."))
                                            .ConfigureAwait(false);
                    }
                }
            }
        }

        /// <summary>
        /// Starting the role editing assistant
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task RunEditAssistantAsync(CommandContext commandContext)
        {
            var roleId = await SelectRoleAsync(commandContext, null).ConfigureAwait(false);
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

                    builder.WithDescription($"{DiscordEmoji.FromGuildEmote(commandContext.Client, roles.DiscordEmojiId)} - {roles.Description}");

                    if (roles.SubRoles.Any())
                    {
                        areRolesAvailable = true;

                        foreach (var role in roles.SubRoles)
                        {
                            rolesFieldText.Append(DiscordEmoji.FromGuildEmote(commandContext.Client, role.DiscordEmojiId));
                            rolesFieldText.Append(" - ");
                            rolesFieldText.Append(role.Description);
                            rolesFieldText.Append('\n');
                        }

                        builder.AddField(LocalizationGroup.GetText("AssistantRolesField", "Roles"), rolesFieldText.ToString());
                    }
                }

                var addSubRoleEmoji = DiscordEmojiService.GetAddEmoji(commandContext.Client);
                var descriptionEmoji = DiscordEmojiService.GetEditEmoji(commandContext.Client);
                var editSubRoleEmoji = DiscordEmojiService.GetEdit2Emoji(commandContext.Client);
                var emojiEmoji = DiscordEmojiService.GetEmojiEmoji(commandContext.Client);
                var deleteSubRoleEmoji = DiscordEmojiService.GetTrashCanEmoji(commandContext.Client);
                var cancelEmoji = DiscordEmojiService.GetCrossEmoji(commandContext.Client);

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

                var message = await commandContext.Channel
                                                  .SendMessageAsync(builder)
                                                  .ConfigureAwait(false);

                var userReactionTask = commandContext.Client
                                                     .GetInteractivity()
                                                     .WaitForReactionAsync(message, commandContext.User);

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
                        await RunAddSubRoleAssistantAsync(commandContext, roleId.Value).ConfigureAwait(false);
                    }
                    else if (userReaction.Result.Emoji.Id == descriptionEmoji.Id)
                    {
                        await RunEditDescriptionAssistantAsync(commandContext, roleId.Value).ConfigureAwait(false);
                    }
                    else if (userReaction.Result.Emoji.Id == editSubRoleEmoji.Id)
                    {
                        await RunEditSubRoleAssistantAsync(commandContext, roleId.Value).ConfigureAwait(false);
                    }
                    else if (userReaction.Result.Emoji.Id == emojiEmoji.Id)
                    {
                        await RunEditEmojiAssistantAsync(commandContext, roleId.Value).ConfigureAwait(false);
                    }
                    else if (userReaction.Result.Emoji.Id == deleteSubRoleEmoji.Id)
                    {
                        await RunDeleteSubRoleAssistantAsync(commandContext, roleId.Value).ConfigureAwait(false);
                    }
                }
            }
        }

        /// <summary>
        /// Editing the description of the role
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <param name="roleId">Id of the role</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task RunEditDescriptionAssistantAsync(CommandContext commandContext, long roleId)
        {
            await commandContext.Channel
                                .SendMessageAsync(LocalizationGroup.GetText("DescriptionPrompt", "Please enter the description of the role."))
                                .ConfigureAwait(false);

            var response = await commandContext.Client
                                               .GetInteractivity()
                                               .WaitForMessageAsync(obj => obj.ChannelId == commandContext.Channel.Id
                                                                        && obj.Author.Id == commandContext.Member.Id)
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
        /// <param name="commandContext">Current command context</param>
        /// <param name="roleId">Id of the role</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task RunEditEmojiAssistantAsync(CommandContext commandContext, long roleId)
        {
            var message = await commandContext.Channel
                                              .SendMessageAsync(LocalizationGroup.GetText("ReactWithEmojiPrompt", "Please react with emoji which should be assigned to the role."))
                                              .ConfigureAwait(false);

            var response = await commandContext.Client
                                               .GetInteractivity()
                                               .WaitForReactionAsync(message, commandContext.Member)
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
        /// <param name="commandContext">Current command context</param>
        /// <param name="mainRoleId">Id of the role</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task RunAddSubRoleAssistantAsync(CommandContext commandContext, long mainRoleId)
        {
            var interactivity = commandContext.Client.GetInteractivity();

            var currentBotMessage = await commandContext.Channel
                                        .SendMessageAsync(LocalizationGroup.GetText("ReactWithEmojiPrompt", "Please react with emoji which should be assigned to the role."))
                                        .ConfigureAwait(false);

            var reaction = await interactivity.WaitForReactionAsync(currentBotMessage, commandContext.User)
                                              .ConfigureAwait(false);

            if (reaction.TimedOut == false && reaction.Result.Emoji.Id > 0)
            {
                var subRoleData = new RaidRoleEntity
                                  {
                                      MainRoleId = mainRoleId,
                                      DiscordEmojiId = reaction.Result.Emoji.Id
                                  };

                currentBotMessage = await commandContext.Channel
                                                        .SendMessageAsync(LocalizationGroup.GetText("DescriptionPrompt", "Please enter the description of the role."))
                                                        .ConfigureAwait(false);

                var currentUserResponse = await interactivity.WaitForMessageAsync(obj => obj.Author.Id == commandContext.User.Id
                                                                                  && obj.ChannelId == commandContext.Channel.Id)
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
        /// <param name="commandContext">Current command context</param>
        /// <param name="mainRoleId">Id of the role</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task RunEditSubRoleAssistantAsync(CommandContext commandContext, long mainRoleId)
        {
            var roleId = await SelectRoleAsync(commandContext, mainRoleId).ConfigureAwait(false);
            if (roleId != null)
            {
                var builder = new DiscordEmbedBuilder();
                builder.WithTitle(LocalizationGroup.GetText("RoleEditTitle", "Raid role configuration"));

                var descriptionEmoji = DiscordEmojiService.GetEditEmoji(commandContext.Client);
                var emojiEmoji = DiscordEmojiService.GetEmojiEmoji(commandContext.Client);
                var cancelEmoji = DiscordEmojiService.GetCrossEmoji(commandContext.Client);

                var commands = new StringBuilder();
                commands.AppendLine(LocalizationGroup.GetFormattedText("RoleEditEditDescriptionCommand", "{0} Edit description", descriptionEmoji));
                commands.AppendLine(LocalizationGroup.GetFormattedText("RoleEditEditEmojiCommand", "{0} Edit emoji", emojiEmoji));
                commands.AppendLine(LocalizationGroup.GetFormattedText("AssistantCancelCommand", "{0} Cancel", cancelEmoji));

                builder.AddField(LocalizationGroup.GetText("AssistantCommandsField", "Commands"), commands.ToString());

                var message = await commandContext.Channel
                                                  .SendMessageAsync(builder)
                                                  .ConfigureAwait(false);

                var userReactionTask = commandContext.Client
                                                     .GetInteractivity()
                                                     .WaitForReactionAsync(message, commandContext.User);

                await message.CreateReactionAsync(descriptionEmoji).ConfigureAwait(false);
                await message.CreateReactionAsync(emojiEmoji).ConfigureAwait(false);
                await message.CreateReactionAsync(cancelEmoji).ConfigureAwait(false);

                var userReaction = await userReactionTask.ConfigureAwait(false);
                if (userReaction.TimedOut == false)
                {
                    if (userReaction.Result.Emoji.Id == descriptionEmoji.Id)
                    {
                        await RunEditDescriptionAssistantAsync(commandContext, roleId.Value).ConfigureAwait(false);
                    }
                    else if (userReaction.Result.Emoji.Id == emojiEmoji.Id)
                    {
                        await RunEditEmojiAssistantAsync(commandContext, roleId.Value).ConfigureAwait(false);
                    }
                }
            }
        }

        /// <summary>
        /// Deletion of a sub role
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <param name="mainRoleId">Id of the role</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task RunDeleteSubRoleAssistantAsync(CommandContext commandContext, long mainRoleId)
        {
            var roleId = await SelectRoleAsync(commandContext, mainRoleId).ConfigureAwait(false);
            if (roleId != null)
            {
                var checkEmoji = DiscordEmojiService.GetCheckEmoji(commandContext.Client);
                var crossEmoji = DiscordEmojiService.GetCrossEmoji(commandContext.Client);

                var message = await commandContext.Channel
                                                  .SendMessageAsync(LocalizationGroup.GetText("DeleteRolePrompt", "Are you sure you want to delete the role?"))
                                                  .ConfigureAwait(false);

                var userReactionTask = commandContext.Client
                                                     .GetInteractivity()
                                                     .WaitForReactionAsync(message, commandContext.User);

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
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task RunDeleteAssistantAsync(CommandContext commandContext)
        {
            var roleId = await SelectRoleAsync(commandContext, null)
                             .ConfigureAwait(false);
            if (roleId != null)
            {
                var checkEmoji = DiscordEmojiService.GetCheckEmoji(commandContext.Client);
                var crossEmoji = DiscordEmojiService.GetCrossEmoji(commandContext.Client);

                var message = await commandContext.Channel
                                                  .SendMessageAsync(LocalizationGroup.GetText("DeleteRolePrompt", "Are you sure you want to delete the role?"))
                                                  .ConfigureAwait(false);

                var userReactionTask = commandContext.Client
                                                     .GetInteractivity()
                                                     .WaitForReactionAsync(message, commandContext.User);

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
        /// <param name="commandContext">Current command context</param>
        /// <param name="mainRoleId">Id of the main role</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task<long?> SelectRoleAsync(CommandContext commandContext, long? mainRoleId)
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
                    rolesFieldText.Append(DiscordEmoji.FromGuildEmote(commandContext.Client, role.DiscordEmojiId));
                    rolesFieldText.Append(' ');
                    rolesFieldText.Append(role.Description);
                    rolesFieldText.Append('\n');

                    roles[i] = role.Id;

                    i++;
                }

                builder.AddField(LocalizationGroup.GetText("AssistantRolesField", "Roles"), rolesFieldText.ToString());
            }

            await commandContext.Channel
                                .SendMessageAsync(builder)
                                .ConfigureAwait(false);

            var currentUserResponse = await commandContext.Client
                                                          .GetInteractivity()
                                                          .WaitForMessageAsync(obj => obj.Author.Id == commandContext.User.Id
                                                                                   && obj.ChannelId == commandContext.Channel.Id)
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
