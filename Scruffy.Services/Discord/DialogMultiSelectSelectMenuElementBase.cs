using Discord;

using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Discord;

/// <summary>
/// Dialog element with select menu (multiple selections)
/// </summary>
/// <typeparam name="TData">Type of the result</typeparam>
public abstract class DialogMultiSelectSelectMenuElementBase<TData> : InteractionDialogElementBase<List<TData>>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    protected DialogMultiSelectSelectMenuElementBase(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Min values
    /// </summary>
    protected virtual int MinValues => 1;

    /// <summary>
    /// Max values
    /// </summary>
    protected virtual int MaxValues => 1;

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Returns the select menu entries which should be added to the message
    /// </summary>
    /// <returns>Reactions</returns>
    public virtual IReadOnlyList<SelectMenuOptionData> GetEntries() => null;

    /// <summary>
    /// Return the message of element
    /// </summary>
    /// <returns>Message</returns>
    public abstract Task<string> GetMessage();

    /// <summary>
    /// Returning the placeholder
    /// </summary>
    /// <returns>Placeholder</returns>
    public virtual string GetPlaceholder() => null;

    /// <summary>
    /// Execute the dialog element
    /// </summary>
    /// <returns>Result</returns>
    public override async Task<List<TData>> Run()
    {
        var components = CommandContext.Interactivity.CreateTemporaryComponentContainer<int>(obj => obj.User.Id == CommandContext.User.Id);
        await using (components.ConfigureAwait(false))
        {
            var componentsBuilder = new ComponentBuilder();

            var selectMenu = new SelectMenuBuilder().WithCustomId(components.AddSelectMenu(0))
                                                    .WithPlaceholder(GetPlaceholder())
                                                    .WithMinValues(MinValues)
                                                    .WithMaxValues(MaxValues);

            var entries = GetEntries();
            if (entries?.Count > 0)
            {
                foreach (var entry in entries.Take(25))
                {
                    selectMenu.AddOption(entry.Label, entry.Value, entry.Description);
                }
            }

            componentsBuilder.WithSelectMenu(selectMenu);

            var message = await CommandContext.SendMessageAsync(await GetMessage().ConfigureAwait(false), components: componentsBuilder.Build())
                                              .ConfigureAwait(false);

            DialogContext.Messages.Add(message);

            components.StartTimeout();

            var (component, _) = await components.Task
                                                 .ConfigureAwait(false);

            await component.DeferAsync()
                           .ConfigureAwait(false);

            await message.ModifyAsync(obj => obj.Components = new ComponentBuilder().Build())
                         .ConfigureAwait(false);

            var selectedEntries = new List<TData>();

            foreach (var selectedValue in component.Data.Values)
            {
                selectedEntries.Add(ConvertSelectedValue(selectedValue));
            }

            return selectedEntries;
        }
    }

    /// <summary>
    /// Convert the selected value
    /// </summary>
    /// <param name="selectedValue">Selected value</param>
    /// <returns>Converted value</returns>
    protected virtual TData ConvertSelectedValue(string selectedValue) => (TData)Convert.ChangeType(selectedValue, typeof(TData));

    #endregion // Methods
}