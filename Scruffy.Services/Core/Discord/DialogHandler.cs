using System.Threading.Tasks;

using DSharpPlus.CommandsNext;

using Microsoft.Extensions.DependencyInjection;

namespace Scruffy.Services.Core.Discord
{
    /// <summary>
    /// Handling dialog elements
    /// </summary>
    public static class DialogHandler
    {
        /// <summary>
        /// Execution one dialog element
        /// </summary>
        /// <typeparam name="T">Type of the element</typeparam>
        /// <typeparam name="TData">Type of the element result</typeparam>
        /// <param name="commandContext">Current command context</param>
        /// <returns>Result</returns>
        public static async Task<TData> Run<T, TData>(CommandContext commandContext) where T : DialogElementBase<TData>
        {
            await using (var serviceProvider = GlobalServiceProvider.Current.GetServiceProvider())
            {
                var service = serviceProvider.GetService<T>();

                service.Initialize(commandContext, serviceProvider);

                return await service.Run()
                                    .ConfigureAwait(false);
            }
        }
    }
}