using Discord;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Raid;
using Scruffy.Data.Entity.Tables.Raid;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Raid;

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
        var builder = new EmbedBuilder();
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
                        roles.Append(DiscordEmoteService.GetGuildEmote(commandContextContainer.Client, subRole.DiscordEmojiId));
                        roles.Append(' ');
                        roles.Append(subRole.Description);
                        roles.Append('\n');
                    }

                    roles.Append('\n');

                    builder.AddField($"{DiscordEmoteService.GetGuildEmote(commandContextContainer.Client, role.DiscordEmojiId)} {role.Description}", roles.ToString());
                }
            }
        }

        var addEmote = DiscordEmoteService.GetAddEmote(commandContextContainer.Client);
        var editEmote = DiscordEmoteService.GetEditEmote(commandContextContainer.Client);
        var deleteEmote = DiscordEmoteService.GetTrashCanEmote(commandContextContainer.Client);
        var cancelEmote = DiscordEmoteService.GetCrossEmote(commandContextContainer.Client);

        var commands = new StringBuilder();
        commands.AppendLine(LocalizationGroup.GetFormattedText("AssistantAddCommand", "{0} Add role", addEmote));

        if (areRolesAvailable)
        {
            commands.AppendLine(LocalizationGroup.GetFormattedText("AssistantEditCommand", "{0} Edit role", editEmote));
            commands.AppendLine(LocalizationGroup.GetFormattedText("AssistantDeleteCommand", "{0} Delete role", deleteEmote));
        }

        commands.AppendLine(LocalizationGroup.GetFormattedText("AssistantCancelCommand", "{0} Cancel", cancelEmote));

        builder.AddField(LocalizationGroup.GetText("AssistantCommandsField", "Commands"), commands.ToString());

        var message = await commandContextContainer.Channel
                                                   .SendMessageAsync(embed: builder.Build())
                                                   .ConfigureAwait(false);

        var userReactionTask = commandContextContainer.Interactivity
                                                      .WaitForReactionAsync(message, commandContextContainer.User);

        await message.AddReactionAsync(addEmote).ConfigureAwait(false);
        await message.AddReactionAsync(editEmote).ConfigureAwait(false);

        if (areRolesAvailable)
        {
            await message.AddReactionAsync(deleteEmote).ConfigureAwait(false);
            await message.AddReactionAsync(cancelEmote).ConfigureAwait(false);
        }

        var userReaction = await userReactionTask.ConfigureAwait(false);

        if (userReaction?.Emote is GuildEmote emote)
        {
            if (emote.Name == addEmote.Name)
            {
                await RunAddAssistantAsync(commandContextContainer).ConfigureAwait(false);
            }
            else if (emote.Name == editEmote.Name && areRolesAvailable)
            {
                await RunEditAssistantAsync(commandContextContainer).ConfigureAwait(false);
            }
            else if (emote.Name == deleteEmote.Name)
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

        var reaction = await commandContextContainer.Interactivity
                                                    .WaitForReactionAsync(currentBotMessage, commandContextContainer.User)
                                                    .ConfigureAwait(false);

        if (reaction?.Emote is GuildEmote guildEmote)
        {
            var roleData = new RaidRoleEntity
                           {
                               DiscordEmojiId = guildEmote.Id
                           };

            currentBotMessage = await commandContextContainer.Channel
                                                             .SendMessageAsync(LocalizationGroup.GetText("DescriptionPrompt", "Please enter the description of the role."))
                                                             .ConfigureAwait(false);

            var currentUserResponse = await commandContextContainer.Interactivity
                                                                   .WaitForMessageAsync(obj => obj.Author.Id == commandContextContainer.User.Id
                                                                                       && obj.Channel.Id == commandContextContainer.Channel.Id)
                                                                   .ConfigureAwait(false);

            if (currentUserResponse != null)
            {
                roleData.Description = currentUserResponse.Content;

                var continueCreation = true;
                var subRolesData = new List<RaidRoleEntity>();
                var checkEmote = DiscordEmoteService.GetCheckEmote(commandContextContainer.Client);
                var crossEmote = DiscordEmoteService.GetCrossEmote(commandContextContainer.Client);

                var addSubRoles = true;
                while (addSubRoles)
                {
                    var promptText = subRolesData.Count == 0
                                         ? LocalizationGroup.GetText("AddSubRolesPrompt", "Do you want to add sub roles?")
                                         : LocalizationGroup.GetText("AddAdditionalSubRolesPrompt", "Do you want to add additional sub roles?");

                    currentBotMessage = await commandContextContainer.Channel
                                                                     .SendMessageAsync(promptText)
                                                                     .ConfigureAwait(false);

                    var userReactionTask = commandContextContainer.Interactivity
                                                                  .WaitForReactionAsync(currentBotMessage, commandContextContainer.User);

                    await currentBotMessage.AddReactionAsync(checkEmote).ConfigureAwait(false);
                    await currentBotMessage.AddReactionAsync(crossEmote).ConfigureAwait(false);

                    var userReaction = await userReactionTask.ConfigureAwait(false);
                    if (userReaction != null)
                    {
                        if (userReaction.Emote.Name == checkEmote.Name)
                        {
                            currentBotMessage = await commandContextContainer.Channel
                                                                             .SendMessageAsync(LocalizationGroup.GetText("ReactWithEmojiPrompt", "Please react with emoji which should be assigned to the role."))
                                                                             .ConfigureAwait(false);

                            reaction = await commandContextContainer.Interactivity
                                                                    .WaitForReactionAsync(currentBotMessage, commandContextContainer.User)
                                                                    .ConfigureAwait(false);

                            if (reaction?.Emote is GuildEmote roleEmote)
                            {
                                var subRoleData = new RaidRoleEntity
                                                  {
                                                      DiscordEmojiId = roleEmote.Id
                                                  };

                                currentBotMessage = await commandContextContainer.Channel
                                                                                 .SendMessageAsync(LocalizationGroup.GetText("DescriptionPrompt", "Please enter the description of the role."))
                                                                                 .ConfigureAwait(false);

                                currentUserResponse = await commandContextContainer.Interactivity
                                                                                   .WaitForMessageAsync(obj => obj.Author.Id == commandContextContainer.User.Id
                                                                                                       && obj.Channel.Id == commandContextContainer.Channel.Id)
                                                                                   .ConfigureAwait(false);

                                if (currentUserResponse != null)
                                {
                                    subRoleData.Description = currentUserResponse.Content;

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
            var builder = new EmbedBuilder();
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

                builder.WithDescription($"{DiscordEmoteService.GetGuildEmote(commandContextContainer.Client, roles.DiscordEmojiId)} - {roles.Description}");

                if (roles.SubRoles.Any())
                {
                    areRolesAvailable = true;

                    foreach (var role in roles.SubRoles)
                    {
                        rolesFieldText.Append(DiscordEmoteService.GetGuildEmote(commandContextContainer.Client, role.DiscordEmojiId));
                        rolesFieldText.Append(" - ");
                        rolesFieldText.Append(role.Description);
                        rolesFieldText.Append('\n');
                    }

                    builder.AddField(LocalizationGroup.GetText("AssistantRolesField", "Roles"), rolesFieldText.ToString());
                }
            }

            var addSubRoleEmote = DiscordEmoteService.GetAddEmote(commandContextContainer.Client);
            var descriptionEmote = DiscordEmoteService.GetEditEmote(commandContextContainer.Client);
            var editSubRoleEmote = DiscordEmoteService.GetEdit2Emote(commandContextContainer.Client);
            var emojiEmote = DiscordEmoteService.GetEmojiEmote(commandContextContainer.Client);
            var deleteSubRoleEmote = DiscordEmoteService.GetTrashCanEmote(commandContextContainer.Client);
            var cancelEmote = DiscordEmoteService.GetCrossEmote(commandContextContainer.Client);

            var commands = new StringBuilder();
            commands.AppendLine(LocalizationGroup.GetFormattedText("RoleEditEditDescriptionCommand", "{0} Edit description", descriptionEmote));
            commands.AppendLine(LocalizationGroup.GetFormattedText("RoleEditEditEmojiCommand", "{0} Edit emoji", emojiEmote));
            commands.AppendLine(LocalizationGroup.GetFormattedText("RoleEditAddSubRoleCommand", "{0} Add sub role", addSubRoleEmote));

            if (areRolesAvailable)
            {
                commands.AppendLine(LocalizationGroup.GetFormattedText("RoleEditEditSubRoleCommand", "{0} Edit sub role", editSubRoleEmote));
                commands.AppendLine(LocalizationGroup.GetFormattedText("RoleEditDeleteSubRoleCommand", "{0} Delete sub role", deleteSubRoleEmote));
            }

            commands.AppendLine(LocalizationGroup.GetFormattedText("AssistantCancelCommand", "{0} Cancel", cancelEmote));

            builder.AddField(LocalizationGroup.GetText("AssistantCommandsField", "Commands"), commands.ToString());

            var message = await commandContextContainer.Channel
                                                       .SendMessageAsync(embed: builder.Build())
                                                       .ConfigureAwait(false);

            var userReactionTask = commandContextContainer.Interactivity
                                                          .WaitForReactionAsync(message, commandContextContainer.User);

            await message.AddReactionAsync(descriptionEmote).ConfigureAwait(false);
            await message.AddReactionAsync(emojiEmote).ConfigureAwait(false);
            await message.AddReactionAsync(addSubRoleEmote).ConfigureAwait(false);

            if (areRolesAvailable)
            {
                await message.AddReactionAsync(editSubRoleEmote).ConfigureAwait(false);
                await message.AddReactionAsync(deleteSubRoleEmote).ConfigureAwait(false);
            }

            await message.AddReactionAsync(cancelEmote).ConfigureAwait(false);

            var userReaction = await userReactionTask.ConfigureAwait(false);
            if (userReaction != null)
            {
                if (userReaction.Emote.Name == addSubRoleEmote.Name)
                {
                    await RunAddSubRoleAssistantAsync(commandContextContainer, roleId.Value).ConfigureAwait(false);
                }
                else if (userReaction.Emote.Name == descriptionEmote.Name)
                {
                    await RunEditDescriptionAssistantAsync(commandContextContainer, roleId.Value).ConfigureAwait(false);
                }
                else if (userReaction.Emote.Name == editSubRoleEmote.Name)
                {
                    await RunEditSubRoleAssistantAsync(commandContextContainer, roleId.Value).ConfigureAwait(false);
                }
                else if (userReaction.Emote.Name == emojiEmote.Name)
                {
                    await RunEditEmojiAssistantAsync(commandContextContainer, roleId.Value).ConfigureAwait(false);
                }
                else if (userReaction.Emote.Name == deleteSubRoleEmote.Name)
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

        var response = await commandContextContainer.Interactivity
                                                    .WaitForMessageAsync(obj => obj.Channel.Id == commandContextContainer.Channel.Id
                                                                        && obj.Author.Id == commandContextContainer.Member.Id)
                                                    .ConfigureAwait(false);

        if (response != null)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                dbFactory.GetRepository<RaidRoleRepository>()
                         .Refresh(obj => obj.Id == roleId,
                                  obj => obj.Description = response.Content);
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

        var response = await commandContextContainer.Interactivity
                                                    .WaitForReactionAsync(message, commandContextContainer.Member)
                                                    .ConfigureAwait(false);

        if (response?.Emote is GuildEmote guildEmote)
        {
            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                dbFactory.GetRepository<RaidRoleRepository>()
                         .Refresh(obj => obj.Id == roleId,
                                  obj => obj.DiscordEmojiId = guildEmote.Id);
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
        var currentBotMessage = await commandContextContainer.Channel
                                                             .SendMessageAsync(LocalizationGroup.GetText("ReactWithEmojiPrompt", "Please react with emoji which should be assigned to the role."))
                                                             .ConfigureAwait(false);

        var reaction = await commandContextContainer.Interactivity
                                                    .WaitForReactionAsync(currentBotMessage, commandContextContainer.User)
                                                    .ConfigureAwait(false);

        if (reaction?.Emote is GuildEmote guildEmote)
        {
            var subRoleData = new RaidRoleEntity
                              {
                                  MainRoleId = mainRoleId,
                                  DiscordEmojiId = guildEmote.Id
                              };

            currentBotMessage = await commandContextContainer.Channel
                                                             .SendMessageAsync(LocalizationGroup.GetText("DescriptionPrompt", "Please enter the description of the role."))
                                                             .ConfigureAwait(false);

            var currentUserResponse = await commandContextContainer.Interactivity
                                                                   .WaitForMessageAsync(obj => obj.Author.Id == commandContextContainer.User.Id
                                                                                       && obj.Channel.Id == commandContextContainer.Channel.Id)
                                                         .ConfigureAwait(false);

            if (currentUserResponse != null)
            {
                subRoleData.Description = currentUserResponse.Content;

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
            var builder = new EmbedBuilder();
            builder.WithTitle(LocalizationGroup.GetText("RoleEditTitle", "Raid role configuration"));

            var descriptionEmote = DiscordEmoteService.GetEditEmote(commandContextContainer.Client);
            var emojiEmote = DiscordEmoteService.GetEmojiEmote(commandContextContainer.Client);
            var cancelEmote = DiscordEmoteService.GetCrossEmote(commandContextContainer.Client);

            var commands = new StringBuilder();
            commands.AppendLine(LocalizationGroup.GetFormattedText("RoleEditEditDescriptionCommand", "{0} Edit description", descriptionEmote));
            commands.AppendLine(LocalizationGroup.GetFormattedText("RoleEditEditEmojiCommand", "{0} Edit emoji", emojiEmote));
            commands.AppendLine(LocalizationGroup.GetFormattedText("AssistantCancelCommand", "{0} Cancel", cancelEmote));

            builder.AddField(LocalizationGroup.GetText("AssistantCommandsField", "Commands"), commands.ToString());

            var message = await commandContextContainer.Channel
                                                       .SendMessageAsync(embed: builder.Build())
                                                       .ConfigureAwait(false);

            var userReactionTask = commandContextContainer.Interactivity
                                                          .WaitForReactionAsync(message, commandContextContainer.User);

            await message.AddReactionAsync(descriptionEmote).ConfigureAwait(false);
            await message.AddReactionAsync(emojiEmote).ConfigureAwait(false);
            await message.AddReactionAsync(cancelEmote).ConfigureAwait(false);

            var userReaction = await userReactionTask.ConfigureAwait(false);
            if (userReaction != null)
            {
                if (userReaction.Emote.Name == descriptionEmote.Name)
                {
                    await RunEditDescriptionAssistantAsync(commandContextContainer, roleId.Value).ConfigureAwait(false);
                }
                else if (userReaction.Emote.Name == emojiEmote.Name)
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
            var checkEmote = DiscordEmoteService.GetCheckEmote(commandContextContainer.Client);
            var crossEmote = DiscordEmoteService.GetCrossEmote(commandContextContainer.Client);

            var message = await commandContextContainer.Channel
                                                       .SendMessageAsync(LocalizationGroup.GetText("DeleteRolePrompt", "Are you sure you want to delete the role?"))
                                                       .ConfigureAwait(false);

            var userReactionTask = commandContextContainer.Interactivity
                                                          .WaitForReactionAsync(message, commandContextContainer.User);

            await message.AddReactionAsync(checkEmote).ConfigureAwait(false);
            await message.AddReactionAsync(crossEmote).ConfigureAwait(false);

            var userReaction = await userReactionTask.ConfigureAwait(false);
            if (userReaction != null)
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
            var checkEmote = DiscordEmoteService.GetCheckEmote(commandContextContainer.Client);
            var crossEmote = DiscordEmoteService.GetCrossEmote(commandContextContainer.Client);

            var message = await commandContextContainer.Channel
                                                       .SendMessageAsync(LocalizationGroup.GetText("DeleteRolePrompt", "Are you sure you want to delete the role?"))
                                                       .ConfigureAwait(false);

            var userReactionTask = commandContextContainer.Interactivity
                                                          .WaitForReactionAsync(message, commandContextContainer.User);

            await message.AddReactionAsync(checkEmote).ConfigureAwait(false);
            await message.AddReactionAsync(crossEmote).ConfigureAwait(false);

            var userReaction = await userReactionTask.ConfigureAwait(false);
            if (userReaction != null)
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

        var builder = new EmbedBuilder();
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
                rolesFieldText.Append(DiscordEmoteService.GetGuildEmote(commandContextContainer.Client, role.DiscordEmojiId));
                rolesFieldText.Append(' ');
                rolesFieldText.Append(role.Description);
                rolesFieldText.Append('\n');

                roles[i] = role.Id;

                i++;
            }

            builder.AddField(LocalizationGroup.GetText("AssistantRolesField", "Roles"), rolesFieldText.ToString());
        }

        await commandContextContainer.Channel
                                     .SendMessageAsync(embed: builder.Build())
                                     .ConfigureAwait(false);

        var currentUserResponse = await commandContextContainer.Interactivity
                                                               .WaitForMessageAsync(obj => obj.Author.Id == commandContextContainer.User.Id
                                                                                   && obj.Channel.Id == commandContextContainer.Channel.Id)
                                                               .ConfigureAwait(false);

        if (currentUserResponse != null
         && int.TryParse(currentUserResponse.Content, out var index)
         && roles.TryGetValue(index, out var selectedRoleId))
        {
            roleId = selectedRoleId;
        }

        return roleId;
    }

    #endregion // Methods
}