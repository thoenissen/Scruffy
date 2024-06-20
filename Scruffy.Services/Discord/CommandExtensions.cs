using Discord;
using Discord.Interactions;

namespace Scruffy.Services.Discord;

/// <summary>
/// Discord command extensions
/// </summary>
internal static class CommandExtensions
{
    /// <summary>
    /// Convert to <see cref="ApplicationCommandOptionProperties"/>
    /// </summary>
    /// <param name="parameterInfo">Parameter information</param>
    /// <returns>Converter object</returns>
    public static ApplicationCommandOptionProperties ToApplicationCommandOptionProps(this SlashCommandParameterInfo parameterInfo)
    {
        var props = new ApplicationCommandOptionProperties
                    {
                        Name = parameterInfo.Name,
                        Description = parameterInfo.Description,
                        Type = parameterInfo.DiscordOptionType ?? ApplicationCommandOptionType.String,
                        IsRequired = parameterInfo.IsRequired,
                        Choices = parameterInfo.Choices?
                                               .Select(obj => new ApplicationCommandOptionChoiceProperties
                                                              {
                                                                  Name = obj.Name,
                                                                  Value = obj.Value
                                                              })
                                               .ToList(),
                        ChannelTypes = parameterInfo.ChannelTypes?.ToList(),
                        IsAutocomplete = parameterInfo.IsAutocomplete,
                        MaxValue = parameterInfo.MaxValue,
                        MinValue = parameterInfo.MinValue
                    };

        parameterInfo.TypeConverter.Write(props, parameterInfo);

        return props;
    }

    /// <summary>
    /// Convert to <see cref="SlashCommandProperties"/>
    /// </summary>
    /// <param name="commandInfo">Command info</param>
    /// <returns>Converter object</returns>
    public static SlashCommandProperties ToApplicationCommandProps(this SlashCommandInfo commandInfo)
    {
        var props = new SlashCommandBuilder().WithName(commandInfo.Name)
                                             .WithDescription(commandInfo.Description)
                                             .WithDMPermission(commandInfo.IsEnabledInDm)
                                             .WithDefaultMemberPermissions((commandInfo.DefaultMemberPermissions ?? 0) | (commandInfo.Module.DefaultMemberPermissions ?? 0))
                                             .WithNameLocalizations(new Dictionary<string, string>())
                                             .WithDescriptionLocalizations(new Dictionary<string, string>())
                                             .Build();

        if (commandInfo.Parameters.Count > SlashCommandBuilder.MaxOptionsCount)
        {
            throw new InvalidOperationException($"Slash Commands cannot have more than {SlashCommandBuilder.MaxOptionsCount} command parameters");
        }

        props.Options = commandInfo.FlattenedParameters
                                   ?.Select(x => x.ToApplicationCommandOptionProps())
                                   .ToList()
                     ?? Optional<List<ApplicationCommandOptionProperties>>.Unspecified;

        return props;
    }

    /// <summary>
    /// Convert to <see cref="ApplicationCommandOptionProperties"/>
    /// </summary>
    /// <param name="commandInfo">Command info</param>
    /// <returns>Converter object</returns>
    public static ApplicationCommandOptionProperties ToApplicationCommandOptionProps(this SlashCommandInfo commandInfo) =>
        new()
        {
            Name = commandInfo.Name,
            Description = commandInfo.Description,
            Type = ApplicationCommandOptionType.SubCommand,
            IsRequired = false,
            Options = commandInfo.FlattenedParameters?.Select(x => x.ToApplicationCommandOptionProps()).ToList()
        };

    /// <summary>
    /// Convert to <see cref="ApplicationCommandProperties"/>
    /// </summary>
    /// <param name="commandInfo">Command information</param>
    /// <returns>Converted object</returns>
    public static ApplicationCommandProperties ToApplicationCommandProps(this ContextCommandInfo commandInfo)
        => commandInfo.CommandType switch
        {
            ApplicationCommandType.Message => new MessageCommandBuilder
                                              {
                                                  Name = commandInfo.Name,
                                                  IsDefaultPermission = commandInfo.DefaultPermission,
                                                  DefaultMemberPermissions = (commandInfo.DefaultMemberPermissions ?? 0) | (commandInfo.Module.DefaultMemberPermissions ?? 0),
                                                  IsDMEnabled = commandInfo.IsEnabledInDm
                                              }.Build(),
            ApplicationCommandType.User => new UserCommandBuilder
                                           {
                                               Name = commandInfo.Name,
                                               IsDefaultPermission = commandInfo.DefaultPermission,
                                               DefaultMemberPermissions = (commandInfo.DefaultMemberPermissions ?? 0) | (commandInfo.Module.DefaultMemberPermissions ?? 0),
                                               IsDMEnabled = commandInfo.IsEnabledInDm
                                           }.Build(),
            _ => throw new InvalidOperationException($"{commandInfo.CommandType} isn't a supported command type.")
        };

    /// <summary>
    /// Convert to <see cref="ApplicationCommandProperties"/>
    /// </summary>
    /// <param name="moduleInfo">Module information</param>
    /// <returns>Converted object</returns>
    public static IReadOnlyCollection<ApplicationCommandProperties> ToApplicationCommandProps(this ModuleInfo moduleInfo)
    {
        var args = new List<ApplicationCommandProperties>();

        moduleInfo.ParseModuleModel(args);

        return args;
    }

    /// <summary>
    /// Parse module
    /// </summary>
    /// <param name="moduleInfo">Module information</param>
    /// <param name="args">Arguments</param>
    private static void ParseModuleModel(this ModuleInfo moduleInfo, List<ApplicationCommandProperties> args)
    {
        if (moduleInfo.DontAutoRegister == false)
        {
            args.AddRange(moduleInfo.ContextCommands?.Select(x => x.ToApplicationCommandProps()));

            if (moduleInfo.IsSlashGroup == false)
            {
                args.AddRange(moduleInfo.SlashCommands?.Select(x => x.ToApplicationCommandProps()));

                foreach (var submodule in moduleInfo.SubModules)
                {
                    submodule.ParseModuleModel(args);
                }
            }
            else
            {
                var options = new List<ApplicationCommandOptionProperties>();

                foreach (var command in moduleInfo.SlashCommands)
                {
                    if (command.IgnoreGroupNames)
                    {
                        args.Add(command.ToApplicationCommandProps());
                    }
                    else
                    {
                        options.Add(command.ToApplicationCommandOptionProps());
                    }
                }

                options.AddRange(moduleInfo.SubModules?.SelectMany(x => x.ParseSubModule(args)));

                var props = new SlashCommandBuilder().WithName(moduleInfo.SlashGroupName)
                                                     .WithDescription(moduleInfo.Description)
                                                     .WithDMPermission(moduleInfo.IsEnabledInDm)
                                                     .WithDefaultMemberPermissions(moduleInfo.DefaultMemberPermissions)
                                                     .WithNameLocalizations(new Dictionary<string, string>())
                                                     .WithDescriptionLocalizations(new Dictionary<string, string>())
                                                     .Build();

                if (options.Count > SlashCommandBuilder.MaxOptionsCount)
                {
                    throw new InvalidOperationException($"Slash Commands cannot have more than {SlashCommandBuilder.MaxOptionsCount} command parameters");
                }

                props.Options = options;

                args.Add(props);
            }
        }
    }

    /// <summary>
    /// Parse sub modules
    /// </summary>
    /// <param name="moduleInfo">Module information</param>
    /// <param name="args">Arguments</param>
    /// <returns>Properties</returns>
    private static IEnumerable<ApplicationCommandOptionProperties> ParseSubModule(this ModuleInfo moduleInfo, List<ApplicationCommandProperties> args)
    {
        if (moduleInfo.DontAutoRegister)
        {
            return Array.Empty<ApplicationCommandOptionProperties>();
        }

        args.AddRange(moduleInfo.ContextCommands?.Select(x => x.ToApplicationCommandProps()));

        var options = new List<ApplicationCommandOptionProperties>();
        options.AddRange(moduleInfo.SubModules?.SelectMany(x => x.ParseSubModule(args)));

        foreach (var command in moduleInfo.SlashCommands)
        {
            if (command.IgnoreGroupNames)
            {
                args.Add(command.ToApplicationCommandProps());
            }
            else
            {
                options.Add(command.ToApplicationCommandOptionProps());
            }
        }

        return moduleInfo.IsSlashGroup
                   ? new List<ApplicationCommandOptionProperties>
                     {
                         new()
                         {
                             Name = moduleInfo.SlashGroupName.ToLower(),
                             Description = moduleInfo.Description,
                             Type = ApplicationCommandOptionType.SubCommandGroup,
                             Options = options
                         }
                     }
                   : options;
    }
}