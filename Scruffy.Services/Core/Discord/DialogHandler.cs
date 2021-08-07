using System;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Services.Core.Discord.Attributes;
using Scruffy.Services.Core.Discord.Interfaces;

namespace Scruffy.Services.Core.Discord
{
    /// <summary>
    /// Handling dialog elements
    /// </summary>
    public static class DialogHandler
    {
        #region Methods

        /// <summary>
        /// Execution one dialog element
        /// </summary>
        /// <typeparam name="T">Type of the element</typeparam>
        /// <typeparam name="TData">Type of the element result</typeparam>
        /// <param name="commandContext">Current command context</param>
        /// <param name="onInitialize">Initialization</param>
        /// <returns>Result</returns>
        public static async Task<TData> Run<T, TData>(ICommandContext commandContext, Action<DialogContext> onInitialize = null) where T : DialogElementBase<TData>
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
        public static async Task<TData> RunForm<TData>(ICommandContext commandContext, bool deleteMessages) where TData : new()
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
    }
}