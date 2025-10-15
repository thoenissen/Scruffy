using Discord.Commands;

namespace Scruffy.Services.Discord.Extensions;

/// <summary>
/// Extension of <see cref="CommandInfo"/>
/// </summary>
public static class CommandInfoExtensions
{
    /// <summary>
    /// Return the full command name
    /// </summary>
    /// <param name="commandInfo">Command info</param>
    /// <returns>Full name</returns>
    public static string GetFullName(this CommandInfo commandInfo)
    {
        var builder = new StringBuilder();

        if (commandInfo.Name != null
         && (commandInfo.Aliases == null || commandInfo.Aliases[0] != commandInfo.Module.Name))
        {
            builder.Insert(0, commandInfo.Name);
        }

        var module = commandInfo.Module;

        while (module != null)
        {
            if (builder.Length != 0)
            {
                builder.Insert(0, ' ');
            }

            builder.Insert(0, module.Name);

            module = module.Parent;
        }

        return builder.ToString();
    }

    /// <summary>
    /// Returns the main group
    /// </summary>
    /// <param name="commandInfo">Command info</param>
    /// <returns>Group name</returns>
    public static string GetMainGroup(this CommandInfo commandInfo)
    {
        var module = commandInfo.Module;

        while (module.Parent != null)
        {
            module = module.Parent;
        }

        return module.Name;
    }
}