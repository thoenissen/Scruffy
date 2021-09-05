using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Services.Core.Discord.Attributes;

namespace Scruffy.Services.Core.Discord
{
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
        /// Dialog context
        /// </summary>
        private DialogContext _dialogContext;

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
            _dialogContext = new DialogContext();
            _serviceProvider = GlobalServiceProvider.Current.GetServiceProvider();
        }

        #endregion // Constructor

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
            await using (var serviceProvider = GlobalServiceProvider.Current.GetServiceProvider())
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
            await using (var serviceProvider = GlobalServiceProvider.Current.GetServiceProvider())
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

                    await commandContext.Channel
                                        .DeleteMessagesAsync(dialogContext.Messages)
                                        .ConfigureAwait(false);
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
        /// <param name="element">Dialog element</param>
        /// <returns>Result</returns>
        public async Task<TData> Run<T, TData>(T element) where T : DialogElementBase<TData>
        {
            element.Initialize(_commandContext, _serviceProvider, _dialogContext);

            return await element.Run()
                                .ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes all messages
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteMessages()
        {
            foreach (var message in _dialogContext.Messages)
            {
                await message.DeleteAsync()
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
}