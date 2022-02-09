namespace Scruffy.Services.Discord.Attributes
{
    /// <summary>
    /// Message component command group
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageComponentCommandGroupAttribute : Attribute
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="group">Group</param>
        public MessageComponentCommandGroupAttribute(string group)
        {
            Group = group;
        }

        #endregion // Constructor

        #region Properties

        /// <summary>
        /// Group
        /// </summary>
        public string Group { get; }

        #endregion // Properties
    }
}
