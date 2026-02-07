namespace Scruffy.Services.GuildWars2.Data;

/// <summary>
/// Upload check
/// </summary>
internal record UploadCheckData
{
    /// <summary>
    /// Id
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Permalink
    /// </summary>
    public required string Permalink { get; init; }

    /// <summary>
    /// Is the log valid?
    /// </summary>
    public bool IsValid { get; set; }
}