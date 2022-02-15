using Discord;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Services.Core;
using Scruffy.Services.Discord.Attributes;

namespace Scruffy.Services.Discord;

/// <summary>
/// Handling dialog elements
/// </summary>
public sealed class DialogHandler : IAsyncDisposable, IDisposable
{
    #region Fields

    /// <summary>
    /// Command context
    /// </summary>
    private CommandContextContainer _commandContext;

    /// <summary>
    /// Service provider
    /// </summary>
    private ServiceProvider _serviceProvider;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="commandContext">Command context</param>
    public DialogHandler(CommandContextContainer commandContext)
    {
        _commandContext = commandContext;
        _serviceProvider = ServiceProviderContainer.Current.GetServiceProvider();

        DialogContext = new DialogContext();
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Dialog context
    /// </summary>
    public DialogContext DialogContext { get; }

    #endregion // Properties

    #region Static - Methods

    /// <summary>
    /// Execution one dialog element
    /// </summary>
    /// <typeparam name="T">Type of the element</typeparam>
    /// <typeparam name="TData">Type of the element result</typeparam>
    /// <param name="commandContext">Current command context</param>
    /// <param name="onInitialize">Initialization</param>
    /// <returns>Result</returns>
    public static async Task<TData> Run<T, TData>(CommandContextContainer commandContext, Action<DialogContext> onInitialize = null) where T : DialogElementBase<TData>
    {
        var serviceProvider = ServiceProviderContainer.Current.GetServiceProvider();
        await using (serviceProvider.ConfigureAwait(false))
        {
            var service = serviceProvider.GetService<T>();

            var dialogContext = new DialogContext();

            onInitialize?.Invoke(dialogContext);

            service.Initialize(commandContext, serviceProvider, dialogContext);

            return await service.Run()
                                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Execution one dialog element
    /// </summary>
    /// <typeparam name="TData">Type of the element result</typeparam>
    /// <param name="commandContext">Current command context</param>
    /// <param name="deleteMessages">Should the creation message be deleted?</param>
    /// <returns>Result</returns>
    public static async Task<TData> RunForm<TData>(CommandContextContainer commandContext, bool deleteMessages) where TData : new()
    {
        var serviceProvider = ServiceProviderContainer.Current.GetServiceProvider();
        await using (serviceProvider.ConfigureAwait(false))
        {
            var data = new TData();
            var dialogContext = new DialogContext();

            foreach (var property in data.GetType().GetProperties())
            {
                var attribute = property.GetCustomAttributes(typeof(DialogElementAssignmentAttribute), false)
                                        .OfType<DialogElementAssignmentAttribute>()
                                        .FirstOrDefault();
                if (attribute != null)
                {
                    var service = (DialogElementBase)serviceProvider.GetService(attribute.DialogElementType);

                    service.Initialize(commandContext, serviceProvider, dialogContext);

                    property.SetValue(data, await service.InternalRun().ConfigureAwait(false));
                }
            }

            if (deleteMessages)
            {
                dialogContext.Messages.Add(commandContext.Message);

                if (commandContext.Channel is ITextChannel textChannel)
                {
                    await textChannel.DeleteMessagesAsync(dialogContext.Messages)
                                     .ConfigureAwait(false);
                }
            }

            return data;
        }
    }

    #endregion // Methods

    #region Methods

    /// <summary>
    /// Execution one dialog element
    /// </summary>
    /// <typeparam name="T">Type of the element</typeparam>
    /// <typeparam name="TData">Type of the element result</typeparam>
    /// <returns>Result</returns>
    public async Task<TData> Run<T, TData>() where T : DialogElementBase<TData>
    {
        var element = _serviceProvider.GetService<T>();

        element.Initialize(_commandContext, _serviceProvider, DialogContext);

        return await element.Run()
                            .ConfigureAwait(false);
    }

    /// <summary>
    /// Execution one dialog element
    /// </summary>
    /// <typeparam name="T">Type of the element</typeparam>
    /// <typeparam name="TData">Type of the element result</typeparam>
    /// <param name="element">Dialog element</param>
    /// <returns>Result</returns>
    public async Task<TData> Run<T, TData>(T element) where T : DialogElementBase<TData>
    {
        element.Initialize(_commandContext, _serviceProvider, DialogContext);

        return await element.Run()
                            .ConfigureAwait(false);
    }

    /// <summary>
    /// Execution one dialog element
    /// </summary>
    /// <typeparam name="TData">Type of the element result</typeparam>
    /// <returns>Result</returns>
    public async Task<TData> RunForm<TData>() where TData : new()
    {
        var data = new TData();

        foreach (var property in data.GetType()
                                     .GetProperties())
        {
            var attribute = property.GetCustomAttributes(typeof(DialogElementAssignmentAttribute), false)
                                    .OfType<DialogElementAssignmentAttribute>()
                                    .FirstOrDefault();

            if (attribute != null)
            {
                var service = (DialogElementBase)_serviceProvider.GetService(attribute.DialogElementType);

                service.Initialize(_commandContext, _serviceProvider, DialogContext);

                property.SetValue(data,
                                  await service.InternalRun()
                                               .ConfigureAwait(false));
            }
        }

        return data;
    }

    /// <summary>
    /// Deletes all messages
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task DeleteMessages()
    {
        if (_commandContext.Channel is ITextChannel textChannel)
        {
            await textChannel.DeleteMessagesAsync(DialogContext.Messages)
                             .ConfigureAwait(false);
        }
    }

    #endregion // Methods

    #region IAsyncDisposable

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.</summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_serviceProvider != null)
        {
            await _serviceProvider.DisposeAsync()
                                  .ConfigureAwait(false);

            _serviceProvider = null;
        }
    }

    #endregion // IAsyncDisposable

    #region IDisposable

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        _serviceProvider?.Dispose();
        _serviceProvider = null;
    }

    #endregion // IDisposable
}