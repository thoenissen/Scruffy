using Newtonsoft.Json;

namespace Scruffy.Data.Json.QuickChart;

/// <summary>
/// Ticks
/// </summary>
/// <typeparam name="T">Type</typeparam>
public class AxisTicks<T> : AxisTicks
    where T : struct
{
    #region Properties

    /// <summary>
    /// Minimum value
    /// </summary>
    [JsonProperty("min")]
    public T? MinValue { get; set; }

    /// <summary>
    /// Maximum value
    /// </summary>
    [JsonProperty("max")]
    public T? MaxValue { get; set; }

    #endregion // Properties
}