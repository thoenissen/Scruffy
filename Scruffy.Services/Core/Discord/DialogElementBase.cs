using System;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;

using Microsoft.Extensions.DependencyInjection;

namespace Scruffy.Services.Core.Discord
{
    /// <summary>
    /// Dialog element
    /// </summary>
    /// <typeparam name="TData">Type of the result</typeparam>
    public abstract class DialogElementBase<TData>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        protected DialogElementBase(LocalizationService localizationService)
        {
            LocalizationGroup = localizationService.GetGroup(GetType().Name);
        }

        #endregion // Constructor

        #region Properties

        /// <summary>
        /// Localization group
        /// </summary>
        public LocalizationGroup LocalizationGroup { get; }

        /// <summary>
        /// Command context
        /// </summary>
        public CommandContext CommandContext { get; private set; }

        /// <summary>
        /// Service provider
        /// </summary>
        public IServiceProvider ServiceProvider { get; private set; }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Initializing
        /// </summary>
        /// <param name="commandContext">Command context</param>
        /// <param name="serviceProvider">Service provider</param>
        internal void Initialize(CommandContext commandContext, IServiceProvider serviceProvider)
        {
            CommandContext = commandContext;
            ServiceProvider = serviceProvider;
        }

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
        public async Task<TSubData> RunSubElement<T, TSubData>() where T : DialogElementBase<TSubData>
        {
            await using (var serviceProvider = GlobalServiceProvider.Current.GetServiceProvider())
            {
                var service = serviceProvider.GetService<T>();

                service.Initialize(CommandContext, serviceProvider);

                return await service.Run()
                                    .ConfigureAwait(false);
            }
        }

        #endregion // Methods
    }
}
