using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Scruffy.Data.Json.ThatShaman;

/// <summary>
/// Next update
/// </summary>
public class NextUpdateData
{
    /// <summary>
    /// Is the update confirmed?
    /// </summary>
    [JsonProperty("confirmed")]
    public bool Confirmed { get; set; }

    /// <summary>
    /// Urls
    /// </summary>
    [JsonProperty("urls")]
    public List<string> Urls { get; set; }

    /// <summary>
    /// Update date
    /// </summary>
    [JsonProperty("when")]
    public DateTime When { get; set; }
}