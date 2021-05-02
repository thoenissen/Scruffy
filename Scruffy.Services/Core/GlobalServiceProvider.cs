using Microsoft.Extensions.DependencyInjection;

using Scruffy.Services.CoreData;
using Scruffy.Services.Fractals;

namespace Scruffy.Services.Core
{
    /// <summary>
    /// Providing a service provider
    /// </summary>
    public class GlobalServiceProvider
    {
        #region Fields

        /// <summary>
        /// Collection of services
        /// </summary>
        private ServiceCollection _serviceCollection;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        private GlobalServiceProvider()
        {
            _serviceCollection = new ServiceCollection();
            _serviceCollection.AddTransient<UserManagementService>();
            _serviceCollection.AddTransient<FractalLfgMessageBuilder>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Current instance
        /// </summary>
        public static GlobalServiceProvider Current { get; } = new GlobalServiceProvider();

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Building a new service provider
        /// </summary>
        /// <returns>The newly created service provider</returns>
        public ServiceProvider GetServiceProvider()
        {
            return _serviceCollection.BuildServiceProvider();
        }

        /// <summary>
        /// Adding a singleton the service collection
        /// </summary>
        /// <typeparam name="T">Type of the service</typeparam>
        /// <param name="singleton">Instance of the service</param>
        public void AddSingleton<T>(T singleton)
            where T : class
        {
            _serviceCollection.AddSingleton(singleton);
        }

        #endregion // Methods
    }
}
