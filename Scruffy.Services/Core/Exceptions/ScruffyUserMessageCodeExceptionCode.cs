namespace Scruffy.Services.Core.Exceptions;

/// <summary>
/// Exception codes
/// </summary>
internal enum ScruffyUserMessageCodeExceptionCode
{
    /// <summary>
    /// You have no user token configured for dps.report
    /// </summary>
    NoDpsReportTokenConfiguration,

    /// <summary>
    /// Import of logs failed
    /// </summary>
    DpsReportImportFailed
}