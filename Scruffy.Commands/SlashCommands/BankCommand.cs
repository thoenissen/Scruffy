using Discord.Interactions;

namespace Scruffy.Commands.SlashCommands;

/// <summary>
/// Bank commands
/// </summary>
public enum BankCommand
{
    /// <summary>
    /// Lists all slots of the material storage with has the maximum amount of items
    /// </summary>
    [ChoiceDisplay("List full material storage slots")]
    ListFullMaterialStorage,
}