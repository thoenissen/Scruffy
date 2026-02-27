namespace Scruffy.Data.Enumerations.Raid;

/// <summary>
/// Registration state
/// </summary>
public enum RegistrationState
{
    /// <summary>
    /// Registered
    /// </summary>
    Registered,

    /// <summary>
    /// The user participated in the raid
    /// </summary>
    Played,

    /// <summary>
    /// The user was on the substitute bench
    /// </summary>
    Substitute,

    /// <summary>
    /// The user did not show up and receives penalty points
    /// </summary>
    NoShow,

    /// <summary>
    /// The user registered after the registration deadline
    /// </summary>
    LateRegistration
}