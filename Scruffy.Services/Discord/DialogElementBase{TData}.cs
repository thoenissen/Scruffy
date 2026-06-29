using Microsoft.Extensions.DependencyInjection;

using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Services.Discord;

/// <summary>
/// Dialog element
/// </summary>
/// <typeparam name="TData">Type of the result</typeparam>
public abstract class DialogElementBase<TData> : DialogElementBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    protected DialogElementBase(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Execution of the element
    /// </summary>
    /// <returns>Result</returns>
    public abstract Task<TData> Run();

    /// <summary>
    /// Execution one dialog element
    /// </summary>
    /// <typeparam name="T">Type of the element</typeparam>
    /// <typeparam name="TSubData">Type of the element result</typeparam>
    /// <returns>Result</returns>
    public async Task<TSubData> RunSubElement<T, TSubData>()
        where T : DialogElementBase<TSubData>
    {
        var service = ServiceProvider.GetService<T>();

        service.Initialize(CommandContext, ServiceProvider, DialogContext);

        return await service.Run()
                            .ConfigureAwait(false);
    }

    /// <summary>
    /// Execution one dialog element
    /// </summary>
    /// <typeparam name="T">Type of the element</typeparam>
    /// <typeparam name="TSubData">Type of the element result</typeparam>
    /// <param name="element">Element</param>
    /// <returns>Result</returns>
    public async Task<TSubData> RunSubElement<T, TSubData>(T element)
        where T : DialogElementBase<TSubData>
    {
        element.Initialize(CommandContext, ServiceProvider, DialogContext);

        return await element.Run()
                            .ConfigureAwait(false);
    }

    /// <summary>
    /// Execution one dialog element
    /// </summary>
    /// <typeparam name="TSubData">Type of the element result</typeparam>
    /// <returns>Result</returns>
    public async Task<TSubData> RunSubForm<TSubData>()
        where TSubData : new()
    {
        var data = new TSubData();

        foreach (var property in data.GetType()
                                     .GetProperties())
        {
            var attribute = property.GetCustomAttributes(typeof(DialogElementAssignmentAttribute), false)
                                    .OfType<DialogElementAssignmentAttribute>()
                                    .FirstOrDefault();

            if (attribute != null)
            {
                var service = (DialogElementBase)ServiceProvider.GetService(attribute.DialogElementType);

                service.Initialize(CommandContext, ServiceProvider, DialogContext);

                property.SetValue(data,
                                  await service.InternalRun()
                                               .ConfigureAwait(false));
            }
        }

        return data;
    }

    #endregion // Methods

    #region DialogElementBase

    /// <inheritdoc/>
    internal sealed override async Task<object> InternalRun()
    {
        return await Run().ConfigureAwait(false);
    }

    #endregion // DialogElementBase
}