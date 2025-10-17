using System.Net;
using System.Net.Http;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Scruffy.Data.Entity;
using Scruffy.Services.Core.JobScheduler;
using Scruffy.Services.CoreData;
using Scruffy.Services.Debug;
using Scruffy.Services.Discord;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.Core;

/// <summary>
/// Providing a service provider
/// </summary>
public sealed class ServiceProviderContainer : IAsyncDisposable
{
    #region Fields

    /// <summary>
    /// Collection of services
    /// </summary>
    private ServiceCollection _serviceCollection;

    /// <summary>
    /// Service provider
    /// </summary>
    private IAsyncDisposable _serviceProvider;

    /// <summary>
    /// Service scope factory
    /// </summary>
    private IServiceScopeFactory _serviceScopeFactory;

    /// <summary>
    /// Singleton scope
    /// </summary>
    private IServiceScope _singletonScope;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public ServiceProviderContainer()
    {
        Current = this;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Current instance
    /// </summary>
    public static ServiceProviderContainer Current { get; private set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Initialize
    /// </summary>
    /// <param name="onInitialize">Additional initialization</param>
    public void Initialize(Action<IServiceCollection> onInitialize)
    {
        _serviceCollection = [];

        var singletons = new List<SingletonLocatedServiceBase>();

        foreach (var type in Assembly.Load("Scruffy.Services")
                                     .GetTypes()
                                     .Where(obj => typeof(SingletonLocatedServiceBase).IsAssignableFrom(obj)
                                                && obj.IsAbstract == false))
        {
            var instance = (SingletonLocatedServiceBase)Activator.CreateInstance(type);

            if (instance != null)
            {
                _serviceCollection.AddSingleton(type, instance);

                singletons.Add(instance);
            }
        }

        _serviceCollection.AddTransient<UserManagementService>();
        _serviceCollection.AddTransient<DebugService>();
        _serviceCollection.AddTransient<ThatShamanConnector>();
        _serviceCollection.AddTransient<QuickChartConnector>();
        _serviceCollection.AddTransient<DpsReportConnector>();
        _serviceCollection.AddTransient<GitHubConnector>();

        _serviceCollection.AddScoped<RepositoryFactory>();

        foreach (var type in Assembly.Load("Scruffy.Services")
                                     .GetTypes()
                                     .Where(obj => (typeof(DialogElementBase).IsAssignableFrom(obj)
                                                 || typeof(LocatedServiceBase).IsAssignableFrom(obj))
                                                && obj.IsAbstract == false))
        {
            _serviceCollection.AddTransient(type);
        }

        foreach (var type in Assembly.Load("Scruffy.Services")
                                     .GetTypes()
                                     .Where(obj => typeof(LocatedAsyncJob).IsAssignableFrom(obj)
                                                && obj.IsAbstract == false))
        {
            _serviceCollection.AddTransient(type);
        }

        _serviceCollection.AddHttpClient();
        _serviceCollection.AddHttpClient("GitHub",
                                         obj =>
                                         {
                                             obj.DefaultRequestHeaders.Add("Accept", "application/json");
                                             obj.DefaultRequestHeaders.Add("User-Agent", "Scruffy");
                                         })
                          .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                                                                    {
                                                                        UseDefaultCredentials = true,
                                                                        Credentials = new NetworkCredential(Environment.GetEnvironmentVariable("SCRUFFY_GITHUB_USER"),
                                                                                                            Environment.GetEnvironmentVariable("SCRUFFY_GITHUB_TOKEN")),
                                                                    });
        _serviceCollection.AddHttpClient("GW2Wiki",
                                         obj => obj.DefaultRequestHeaders.Add("User-Agent", "Scruffy"));

        onInitialize?.Invoke(_serviceCollection);

        var serviceProvider = _serviceCollection.BuildServiceProvider();

        _serviceProvider = serviceProvider;

        _serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        _singletonScope = _serviceScopeFactory.CreateScope();

        foreach (var singleton in singletons)
        {
            singleton.Initialize(_singletonScope.ServiceProvider);
        }
    }

    /// <summary>
    /// Building a new service provider
    /// </summary>
    /// <returns>The newly created service provider</returns>
    public ServiceProvider GetServiceProvider()
    {
        return _serviceCollection.BuildServiceProvider();
    }

    /// <summary>
    /// Creates a new service scope
    /// </summary>
    /// <returns>Newly created scope.</returns>
    public IServiceScope CreateScope() => _serviceScopeFactory.CreateScope();

    #endregion // Methods

    #region IDisposable

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async ValueTask DisposeAsync()
    {
        _singletonScope.Dispose();

        await _serviceProvider.DisposeAsync()
                              .ConfigureAwait(false);
    }

    #endregion // IDisposable
}