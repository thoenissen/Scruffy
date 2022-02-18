using Discord;

using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Discord;

/// <summary>
/// Dialog element with select menu
/// </summary>
/// <typeparam name="TData">Type of the result</typeparam>
public abstract class DialogSelectMenuElementBase<TData> : DialogElementBase<TData>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    protected DialogSelectMenuElementBase(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Returns the select menu entries which should be added to the message
    /// </summary>
    /// <returns>Reactions</returns>
    public virtual IReadOnlyList<SelectMenuEntryData<TData>> GetEntries() => null;

    /// <summary>
    /// Returning the message
    /// </summary>
    /// <returns>Message</returns>
    public abstract string GetMessage();

    /// <summary>
    /// Returning the placeholder
    /// </summary>
    /// <returns>Placeholder</returns>
    public virtual string GetPlaceholder() => null;

    /// <summary>
    /// Default case if none of the given buttons is used
    /// </summary>
    /// <returns>Result</returns>
    protected abstract TData DefaultFunc();

    /// <summary>
    /// Execute the dialog element
    /// </summary>
    /// <returns>Result</returns>
    public override async Task<TData> Run()
    {
        var components = CommandContext.Interactivity.CreateTemporaryComponentContainer<int>(obj => obj.User.Id == CommandContext.User.Id);
        await using (components.ConfigureAwait(false))
        {
            var componentsBuilder = new ComponentBuilder();

            var entries = GetEntries();
            if (entries?.Count > 0)
            {
                var selectMenu = new SelectMenuBuilder().WithCustomId(components.AddSelectMenu(0))
                                                        .WithPlaceholder(GetPlaceholder());

                var i = 1;
                foreach (var entry in entries.Take(25))
                {
                    selectMenu.AddOption(entry.CommandText, i.ToString(), null, entry.Emote);

                    i++;
                }

                componentsBuilder.WithSelectMenu(selectMenu);
            }

            var message = await CommandContext.SendMessageAsync(GetMessage(), components: componentsBuilder.Build())
                                              .ConfigureAwait(false);

            DialogContext.Messages.Add(message);

            components.StartTimeout();

            var (component, _) = await components.Task
                                                 .ConfigureAwait(false);

            var selectedValue = component.Data.Values.FirstOrDefault();

            await component.DeferAsync()
                           .ConfigureAwait(false);

            var disabledComponentBuilder = new ComponentBuilder();

            foreach (var selectMenuComponent in componentsBuilder.ActionRows.SelectMany(obj => obj.Components).OfType<SelectMenuComponent>())
            {
                disabledComponentBuilder.WithSelectMenu(selectMenuComponent.ToBuilder()
                                                                           .WithOptions(selectMenuComponent.Options
                                                                                                           .Where(obj => obj.Value == selectedValue)
                                                                                                           .Select(obj => new SelectMenuOptionBuilder().WithLabel(obj.Label)
                                                                                                                                                       .WithValue(obj.Label)
                                                                                                                                                       .WithEmote(obj.Emote)
                                                                                                                                                       .WithDefault(true))
                                                                                                           .Take(1)
                                                                                                           .ToList())
                                                                           .WithDisabled(true));
            }

            await message.ModifyAsync(obj => obj.Components = disabledComponentBuilder.Build())
                         .ConfigureAwait(false);

            var executedButton = entries?.Take(Convert.ToInt32(selectedValue)).LastOrDefault();

            return executedButton != null
                       ? await executedButton.Func()
                                             .ConfigureAwait(false)
                       : DefaultFunc();
        }
    }

    #endregion // Methods
}