using Scruffy.Services.Core.Discord.Attributes;

namespace Scruffy.Services.GuildAdministration.DialogElements.Forms
{
    /// <summary>
    /// Create the guild administration
    /// </summary>
    public class CreateGuildAdministrationFormData
    {
        #region Properties

        /// <summary>
        /// Api-Key
        /// </summary>
        [DialogElementAssignment(typeof(GuildAdministrationApiKeyDialogElement))]
        public string ApiKey { get; set; }

        /// <summary>
        /// Id of the Guild
        /// </summary>
        [DialogElementAssignment(typeof(GuildAdministrationGuildDialogElement))]
        public string GuildId { get; set; }

        #endregion // Properties
    }
}
