using Newtonsoft.Json;

namespace Scruffy.Data.Converter;

/// <summary>
/// Converter to convert seconds (double) to a <see cref="TimeSpan"/>.
/// </summary>
public class SecondsTimeSpanConverter : JsonConverter<TimeSpan>
{
    /// <inheritdoc/>
    public override TimeSpan ReadJson(JsonReader reader, Type objectType, TimeSpan existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return reader.Value != null
            ? reader.ValueType == typeof(long)
                ? TimeSpan.FromSeconds((long)reader.Value)
                : TimeSpan.FromSeconds((double)reader.Value)
            : TimeSpan.Zero;
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, TimeSpan value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}