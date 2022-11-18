namespace Scruffy.ServiceHosts.DpsReportAnalyzer.Data.Report
{
    /// <summary>
    /// Class corresponding to a rotation
    /// </summary>
    public class JsonRotation
    {
        /// <summary>
        /// ID of the skill
        /// </summary>
        /// <seealso cref="JsonLog.SkillMap"/>
        public long Id { get; set; }

        /// <summary>
        /// List of casted skills
        /// </summary>
        /// <seealso cref="JsonSkill"/>
        public IReadOnlyList<JsonSkill> Skills { get; set; }
    }
}