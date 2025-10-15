using Discord;

using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Discord;

/// <summary>
/// Dialog element with buttons
/// </summary>
/// <typeparam name="TData">Type of the result</typeparam>
public abstract class DialogButtonElementBase<TData> : DialogElementBase<TData>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    protected DialogButtonElementBase(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Returns the buttons which should be added to the message
    /// </summary>
    /// <returns>Reactions</returns>
    public virtual IReadOnlyList<ButtonData<TData>> GetButtons() => null;

    /// <summary>
    /// Returning the message
    /// </summary>
    /// <returns>Message</returns>
    public abstract string GetMessage();

    /// <summary>
    /// Default case if none of the given buttons is used
    /// </summary>
    /// <returns>Result</returns>
    protected abstract TData DefaultFunc();

    /// <inheritdoc/>
    public override async Task<TData> Run()
    {
        var components = CommandContext.Interactivity.CreateTemporaryComponentContainer<int>(obj => obj.User.Id == CommandContext.User.Id);

        await using (components.ConfigureAwait(false))
        {
            var componentsBuilder = new ComponentBuilder();

            var buttons = GetButtons();

            if (buttons?.Count > 0)
            {
                var i = 1;

                foreach (var button in buttons)
                {
                    componentsBuilder.WithButton(button.CommandText, components.AddButton(i), ButtonStyle.Secondary, button.Emote);

                    i++;
                }
            }

            var message = await CommandContext.SendMessageAsync(GetMessage(), components: componentsBuilder.Build())
                                              .ConfigureAwait(false);

            DialogContext.Messages.Add(message);

            components.StartTimeout();

            var (component, identification) = await components.Task
                                                              .ConfigureAwait(false);

            await component.DeferAsync()
                           .ConfigureAwait(false);

            var disabledComponentBuilder = new ComponentBuilder();

            foreach (var buttonComponent in componentsBuilder.ActionRows.SelectMany(obj => obj.Components).OfType<ButtonComponent>())
            {
                disabledComponentBuilder.WithButton(buttonComponent.ToBuilder().WithDisabled(true));
            }

            await message.ModifyAsync(obj => obj.Components = disabledComponentBuilder.Build())
                         .ConfigureAwait(false);

            var executedButton = buttons?.Take(identification).LastOrDefault();

            return executedButton != null
                       ? await executedButton.Func()
                                             .ConfigureAwait(false)
                       : DefaultFunc();
        }
    }

    #endregion // Methods
}