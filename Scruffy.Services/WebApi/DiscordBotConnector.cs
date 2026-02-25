using System.Net.Http;
using System.Text.Json;

namespace Scruffy.Services.WebApi;

/// <summary>
/// Connector for calling the Discord service host REST API
/// </summary>
public sealed class DiscordBotConnector
{
    #region Fields

    /// <summary>
    /// Http client factory
    /// </summary>
    private readonly IHttpClientFactory _httpClientFactory;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="httpClientFactory">Http client factory</param>
    public DiscordBotConnector(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Schedules raid message refresh jobs on the Discord service host
    /// </summary>
    /// <param name="configurationId">ID of the raid configuration</param>
    /// <param name="deadline">Deadline timestamp</param>
    /// <param name="timeStamp">Appointment timestamp</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ScheduleRaidMessageRefreshAsync(long configurationId, DateTime deadline, DateTime timeStamp)
    {
        using (var client = _httpClientFactory.CreateClient("DiscordBot"))
        {
            var payload = new { ConfigurationId = configurationId, Deadline = deadline, TimeStamp = timeStamp };

            using (var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"))
            {
                using (var response = await client.PostAsync("/api/raid/refresh", content).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                }
            }
        }
    }

    #endregion // Methods
}