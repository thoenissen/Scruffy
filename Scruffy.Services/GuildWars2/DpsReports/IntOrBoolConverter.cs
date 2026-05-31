using GW2EIDPSReport.DPSReportJsons;

using Newtonsoft.Json;

namespace Scruffy.Services.GuildWars2.DpsReports;

/// <summary>
/// Converter for int? that can also handle boolean values
/// </summary>
/// <remarks>The data of <see cref="DPSReportGetUploadsObject"/> somites uses a <see langword="bool"/> for <see cref="DPSReportGetUploadsObject.FoundUploads"/></remarks>
internal sealed class IntOrBoolConverter : JsonConverter<int?>
{
    #region JsonConverter

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, int? value, JsonSerializer serializer)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc/>
    public override int? ReadJson(JsonReader reader, Type objectType, int? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return reader.TokenType switch
               {
                   JsonToken.Boolean => reader.Value is true ? 0 : -1,
                   JsonToken.Integer => Convert.ToInt32(reader.Value),
                   JsonToken.Null => null,
                   _ => throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing int?")
               };
    }

    #endregion // JsonConverter
}