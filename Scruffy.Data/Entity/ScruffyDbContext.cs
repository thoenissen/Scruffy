using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Keyless;
using Scruffy.Data.Entity.Tables.Calendar;
using Scruffy.Data.Entity.Tables.CoreData;
using Scruffy.Data.Entity.Tables.Discord;
using Scruffy.Data.Entity.Tables.Fractals;
using Scruffy.Data.Entity.Tables.Games;
using Scruffy.Data.Entity.Tables.General;
using Scruffy.Data.Entity.Tables.Guild;
using Scruffy.Data.Entity.Tables.GuildWars2.Account;
using Scruffy.Data.Entity.Tables.GuildWars2.GameData;
using Scruffy.Data.Entity.Tables.GuildWars2.Guild;
using Scruffy.Data.Entity.Tables.Raid;
using Scruffy.Data.Entity.Tables.Reminder;
using Scruffy.Data.Entity.Tables.Statistics;

namespace Scruffy.Data.Entity;

/// <summary>
/// Accessing the database of the discord bot
/// </summary>
public class ScruffyDbContext : DbContext
{
    #region Fields

    /// <summary>
    /// Connection string
    /// </summary>
    private static string _connectionString;

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Connection string
    /// </summary>
    public string ConnectionString => _connectionString;

    /// <summary>
    /// Last error
    /// </summary>
    public Exception LastError { get; set; }

    #endregion // Properties

    #region DbContext

    /// <summary>
    /// <para>
    /// Override this method to configure the database (and other options) to be used for this context.
    /// This method is called for each instance of the context that is created.
    /// The base implementation does nothing.
    /// </para>
    /// <para>
    /// In situations where an instance of <see cref="T:Microsoft.EntityFrameworkCore.DbContextOptions"/> may or may not have been passed
    /// to the constructor, you can use <see cref="P:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.IsConfigured"/> to determine if
    /// the options have already been set, and skip some or all of the logic in
    /// <see cref="M:Microsoft.EntityFrameworkCore.DbContext.OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder)"/>.
    /// </para>
    /// </summary>
    /// <param name="optionsBuilder">
    /// A builder used to create or modify options for this context. Databases (and other extensions)
    /// typically define extension methods on this object that allow you to configure the context.
    /// </param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionStringBuilder = new SqlConnectionStringBuilder
                                      {
                                          ApplicationName = "Scruffy.Bot",
                                          DataSource = Environment.GetEnvironmentVariable("SCRUFFY_DB_DATA_SOURCE"),
                                          InitialCatalog = Environment.GetEnvironmentVariable("SCRUFFY_DB_CATALOG"),
                                          MultipleActiveResultSets = false,
                                          IntegratedSecurity = false,
                                          UserID = Environment.GetEnvironmentVariable("SCRUFFY_DB_USER"),
                                          Password = Environment.GetEnvironmentVariable("SCRUFFY_DB_PASSWORD")
                                      };
        _connectionString = connectionStringBuilder.ConnectionString;

        optionsBuilder.UseSqlServer(ConnectionString);
#if DEBUG
        optionsBuilder.LogTo(s => System.Diagnostics.Debug.WriteLine(s));
#endif
        base.OnConfiguring(optionsBuilder);
    }

    /// <summary>
    /// Override this method to further configure the model that was discovered by convention from the entity types
    /// exposed in <see cref="T:Microsoft.EntityFrameworkCore.DbSet`1"/> properties on your derived context. The resulting model may be cached
    /// and re-used for subsequent instances of your derived context.
    /// </summary>
    /// <remarks>
    /// If a model is explicitly set on the options for this context (via <see cref="M:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel)"/>)
    /// then this method will not be run.
    /// </remarks>
    /// <param name="modelBuilder">
    /// The builder being used to construct the model for this context. Databases (and other extensions) typically
    /// define extension methods on this object that allow you to configure aspects of the model that are specific
    /// to a given database.
    /// </param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // CoreData
        modelBuilder.Entity<UserEntity>();
        modelBuilder.Entity<ServerConfigurationEntity>();

        modelBuilder.Entity<UserEntity>()
                    .HasMany(obj => obj.RaidRegistrations)
                    .WithOne(obj => obj.User)
                    .HasForeignKey(obj => obj.UserId)
                    .IsRequired();

        modelBuilder.Entity<UserEntity>()
                    .HasMany(obj => obj.RaidUserRoles)
                    .WithOne(obj => obj.User)
                    .HasForeignKey(obj => obj.UserId)
                    .IsRequired();

        modelBuilder.Entity<UserEntity>()
                    .HasMany(obj => obj.GuildWarsAccounts)
                    .WithOne(obj => obj.User)
                    .HasForeignKey(obj => obj.UserId)
                    .IsRequired();

        modelBuilder.Entity<UserEntity>()
                    .HasMany(obj => obj.DiscordAccounts)
                    .WithOne(obj => obj.User)
                    .HasForeignKey(obj => obj.UserId)
                    .IsRequired();

        // Discord
        modelBuilder.Entity<DiscordAccountEntity>();
        modelBuilder.Entity<BlockedDiscordChannelEntity>();

        modelBuilder.Entity<DiscordAccountEntity>()
                    .HasMany(obj => obj.OneTimeReminders)
                    .WithOne(obj => obj.DiscordAccount)
                    .HasForeignKey(obj => obj.DiscordAccountId)
                    .IsRequired();

        modelBuilder.Entity<BlockedDiscordChannelEntity>()
                    .HasKey(obj => new
                                   {
                                       obj.ServerId,
                                       obj.ChannelId
                                   });

        // General
        modelBuilder.Entity<LogEntryEntity>();

        // Fractals
        modelBuilder.Entity<FractalLfgConfigurationEntity>();
        modelBuilder.Entity<FractalRegistrationEntity>();
        modelBuilder.Entity<FractalAppointmentEntity>();

        modelBuilder.Entity<FractalLfgConfigurationEntity>()
                    .HasMany(obj => obj.FractalRegistrations)
                    .WithOne(obj => obj.FractalLfgConfiguration)
                    .IsRequired();

        modelBuilder.Entity<FractalRegistrationEntity>()
                    .HasKey(obj => new
                                   {
                                       obj.ConfigurationId,
                                       obj.AppointmentTimeStamp,
                                       obj.UserId
                                   });

        // Raid
        modelBuilder.Entity<RaidAppointmentEntity>();
        modelBuilder.Entity<RaidDayConfigurationEntity>();
        modelBuilder.Entity<RaidExperienceAssignmentEntity>();
        modelBuilder.Entity<RaidExperienceLevelEntity>();
        modelBuilder.Entity<RaidRegistrationEntity>();
        modelBuilder.Entity<RaidRegistrationRoleAssignmentEntity>();
        modelBuilder.Entity<RaidRequiredRoleEntity>();
        modelBuilder.Entity<RaidRoleAliasNameEntity>();
        modelBuilder.Entity<RaidRoleEntity>();
        modelBuilder.Entity<RaidUserRoleEntity>();
        modelBuilder.Entity<RaidDayTemplateEntity>();
        modelBuilder.Entity<RaidRoleLineupAssignmentEntity>();
        modelBuilder.Entity<RaidRoleLineupEntryEntity>();
        modelBuilder.Entity<RaidRoleLineupHeaderEntity>();
        modelBuilder.Entity<RaidCurrentUserPointsEntity>();

        modelBuilder.Entity<RaidExperienceAssignmentEntity>()
                    .HasKey(obj => new
                                   {
                                       ConfigurationId = obj.TemplateId,
                                       obj.ExperienceLevelId
                                   });

        modelBuilder.Entity<RaidRequiredRoleEntity>()
                    .HasKey(obj => new
                                   {
                                       obj.TemplateId,
                                       obj.Index
                                   });

        modelBuilder.Entity<RaidUserRoleEntity>()
                    .HasKey(obj => new
                                   {
                                       obj.UserId,
                                       obj.MainRoleId,
                                       obj.SubRoleId
                                   });

        modelBuilder.Entity<RaidRoleLineupAssignmentEntity>()
                    .HasKey(obj => new
                                   {
                                       obj.TemplateId,
                                       obj.LineupHeaderId
                                   });

        modelBuilder.Entity<RaidRoleLineupEntryEntity>()
                    .HasKey(obj => new
                                   {
                                       obj.LineupHeaderId,
                                       obj.Position,
                                       obj.RoleId
                                   });

        modelBuilder.Entity<RaidAppointmentEntity>()
                    .HasMany(obj => obj.RaidRegistrations)
                    .WithOne(obj => obj.RaidAppointment)
                    .HasForeignKey(obj => obj.AppointmentId)
                    .IsRequired();

        modelBuilder.Entity<RaidDayConfigurationEntity>()
                    .HasMany(obj => obj.RaidAppointments)
                    .WithOne(obj => obj.RaidDayConfiguration)
                    .HasForeignKey(obj => obj.ConfigurationId)
                    .IsRequired();

        modelBuilder.Entity<RaidExperienceLevelEntity>()
                    .HasMany(obj => obj.InferiorRaidExperienceLevels)
                    .WithOne(obj => obj.SuperiorRaidExperienceLevel)
                    .HasForeignKey(obj => obj.SuperiorExperienceLevelId);

        modelBuilder.Entity<RaidExperienceLevelEntity>()
                    .HasMany(obj => obj.RaidExperienceAssignments)
                    .WithOne(obj => obj.RaidExperienceLevel)
                    .HasForeignKey(obj => obj.ExperienceLevelId)
                    .IsRequired();

        modelBuilder.Entity<RaidExperienceLevelEntity>()
                    .HasMany(obj => obj.Users)
                    .WithOne(obj => obj.RaidExperienceLevel)
                    .HasForeignKey(obj => obj.RaidExperienceLevelId);

        modelBuilder.Entity<RaidRegistrationEntity>()
                    .HasMany(obj => obj.RaidRegistrationRoleAssignments)
                    .WithOne(obj => obj.RaidRegistration)
                    .HasForeignKey(obj => obj.RegistrationId)
                    .IsRequired();

        modelBuilder.Entity<RaidRoleEntity>()
                    .HasMany(obj => obj.SubRaidRoles)
                    .WithOne(obj => obj.MainRaidRole)
                    .HasForeignKey(obj => obj.MainRoleId);

        modelBuilder.Entity<RaidRoleEntity>()
                    .HasMany(obj => obj.RaidRoleLineupEntries)
                    .WithOne(obj => obj.RaidRole)
                    .HasForeignKey(obj => obj.RoleId);

        modelBuilder.Entity<RaidDayTemplateEntity>()
                    .HasMany(obj => obj.RaidAppointments)
                    .WithOne(obj => obj.RaidDayTemplate)
                    .HasForeignKey(obj => obj.TemplateId);

        modelBuilder.Entity<RaidDayTemplateEntity>()
                    .HasMany(obj => obj.RaidExperienceAssignments)
                    .WithOne(obj => obj.RaidDayTemplate)
                    .HasForeignKey(obj => obj.TemplateId);

        modelBuilder.Entity<RaidDayTemplateEntity>()
                    .HasMany(obj => obj.RaidRoleLineupAssignments)
                    .WithOne(obj => obj.RaidDayTemplate)
                    .HasForeignKey(obj => obj.TemplateId);

        modelBuilder.Entity<RaidRoleLineupHeaderEntity>()
                    .HasMany(obj => obj.RaidRoleLineupAssignments)
                    .WithOne(obj => obj.RaidRoleLineupHeader)
                    .HasForeignKey(obj => obj.LineupHeaderId);

        modelBuilder.Entity<RaidRoleLineupHeaderEntity>()
                    .HasMany(obj => obj.RaidRoleLineupEntries)
                    .WithOne(obj => obj.RaidRoleLineupHeader)
                    .HasForeignKey(obj => obj.LineupHeaderId);

        // Reminder
        modelBuilder.Entity<OneTimeReminderEntity>();
        modelBuilder.Entity<WeeklyReminderEntity>();

        // Calendar
        modelBuilder.Entity<CalendarAppointmentEntity>();
        modelBuilder.Entity<CalendarAppointmentTemplateEntity>();
        modelBuilder.Entity<CalendarAppointmentScheduleEntity>();
        modelBuilder.Entity<CalendarAppointmentParticipantEntity>();

        modelBuilder.Entity<CalendarAppointmentParticipantEntity>()
                    .HasKey(obj => new
                                   {
                                       obj.AppointmentId,
                                       obj.UserId
                                   });

        modelBuilder.Entity<CalendarAppointmentTemplateEntity>()
                    .HasMany(obj => obj.CalendarAppointments)
                    .WithOne(obj => obj.CalendarAppointmentTemplate)
                    .HasForeignKey(obj => obj.CalendarAppointmentTemplateId);

        modelBuilder.Entity<CalendarAppointmentTemplateEntity>()
                    .HasMany(obj => obj.CalendarAppointmentSchedules)
                    .WithOne(obj => obj.CalendarAppointmentTemplate)
                    .HasForeignKey(obj => obj.CalendarAppointmentTemplateId);

        modelBuilder.Entity<CalendarAppointmentEntity>()
                    .HasMany(obj => obj.CalendarAppointmentParticipants)
                    .WithOne(obj => obj.CalendarAppointment)
                    .HasForeignKey(obj => obj.AppointmentId);

        // Guild
        modelBuilder.Entity<GuildEntity>();
        modelBuilder.Entity<GuildLogEntryEntity>();
        modelBuilder.Entity<GuildSpecialRankConfigurationEntity>();
        modelBuilder.Entity<GuildSpecialRankPointsEntity>();
        modelBuilder.Entity<GuildSpecialRankProtocolEntryEntity>();
        modelBuilder.Entity<GuildSpecialRankRoleAssignmentEntity>();
        modelBuilder.Entity<GuildSpecialRankIgnoreRoleAssignmentEntity>();
        modelBuilder.Entity<GuildChannelConfigurationEntity>();
        modelBuilder.Entity<GuildRankEntity>();
        modelBuilder.Entity<GuildRankCurrentPointsEntity>();

        modelBuilder.Entity<GuildLogEntryEntity>()
                    .HasKey(obj => new
                                   {
                                       obj.GuildId,
                                       obj.Id
                                   });

        modelBuilder.Entity<GuildChannelConfigurationEntity>()
                    .HasKey(obj => new
                                   {
                                       obj.GuildId,
                                       obj.Type
                                   }
                           );

        modelBuilder.Entity<GuildEntity>()
                    .HasMany(obj => obj.GuildLogEntries)
                    .WithOne(obj => obj.Guild)
                    .HasForeignKey(obj => obj.GuildId);

        modelBuilder.Entity<GuildSpecialRankConfigurationEntity>()
                    .HasMany(obj => obj.GuildSpecialRankPoints)
                    .WithOne(obj => obj.GuildSpecialRankConfiguration)
                    .HasForeignKey(obj => obj.ConfigurationId);

        modelBuilder.Entity<GuildSpecialRankConfigurationEntity>()
                    .HasMany(obj => obj.GuildSpecialRankProtocolEntries)
                    .WithOne(obj => obj.GuildSpecialRankConfiguration)
                    .HasForeignKey(obj => obj.ConfigurationId);

        modelBuilder.Entity<GuildSpecialRankConfigurationEntity>()
                    .HasMany(obj => obj.GuildSpecialRankRoleAssignments)
                    .WithOne(obj => obj.GuildSpecialRankConfiguration)
                    .HasForeignKey(obj => obj.ConfigurationId);

        modelBuilder.Entity<GuildSpecialRankConfigurationEntity>()
                    .HasMany(obj => obj.GuildSpecialRankIgnoreRoleAssignments)
                    .WithOne(obj => obj.GuildSpecialRankConfiguration)
                    .HasForeignKey(obj => obj.ConfigurationId);

        modelBuilder.Entity<GuildSpecialRankPointsEntity>()
                    .HasKey(obj => new
                                   {
                                       obj.ConfigurationId,
                                       obj.UserId
                                   });

        modelBuilder.Entity<GuildSpecialRankRoleAssignmentEntity>()
                    .HasKey(obj => new
                                   {
                                       obj.ConfigurationId,
                                       obj.DiscordRoleId
                                   });

        modelBuilder.Entity<GuildSpecialRankIgnoreRoleAssignmentEntity>()
                    .HasKey(obj => new
                                   {
                                       obj.ConfigurationId,
                                       obj.DiscordRoleId
                                   });

        modelBuilder.Entity<GuildRankCurrentPointsEntity>()
                    .HasKey(obj => new
                                   {
                                       obj.GuildId,
                                       obj.UserId,
                                       obj.Date,
                                       obj.Type
                                   });

        // Games
        modelBuilder.Entity<GameChannelEntity>();

        // Guild Wars 2
        modelBuilder.Entity<GuildWarsAccountEntity>();
        modelBuilder.Entity<GuildWarsAccountDailyLoginCheckEntity>();
        modelBuilder.Entity<GuildWarsAccountAchievementEntity>();
        modelBuilder.Entity<GuildWarsAccountAchievementBitEntity>();
        modelBuilder.Entity<GuildWarsWorldEntity>();
        modelBuilder.Entity<GuildWarsItemEntity>();
        modelBuilder.Entity<GuildWarsItemGuildUpgradeConversionEntity>();
        modelBuilder.Entity<GuildWarsAchievementEntity>();
        modelBuilder.Entity<GuildWarsAchievementFlagEntity>();
        modelBuilder.Entity<GuildWarsAchievementPrerequisiteEntity>();
        modelBuilder.Entity<GuildWarsAchievementBitEntity>();
        modelBuilder.Entity<GuildWarsAchievementRewardEntity>();
        modelBuilder.Entity<GuildWarsAchievementTierEntity>();
        modelBuilder.Entity<GuildWarsGuildMemberEntity>();

        modelBuilder.Entity<GuildWarsAccountDailyLoginCheckEntity>()
                    .HasKey(obj => new
                                   {
                                       obj.Name,
                                       obj.Date
                                   });

        modelBuilder.Entity<GuildWarsAccountAchievementEntity>()
                    .HasKey(obj => new
                                   {
                                       obj.AccountName,
                                       obj.AchievementId
                                   });

        modelBuilder.Entity<GuildWarsAccountAchievementEntity>()
                    .HasMany(obj => obj.GuildWarsAccountAchievementBits)
                    .WithOne(obj => obj.GuildWarsAccountAchievement)
                    .HasForeignKey(obj => new { obj.AccountName, obj.AchievementId });

        modelBuilder.Entity<GuildWarsAccountAchievementBitEntity>()
                    .HasKey(obj => new
                                   {
                                       obj.AccountName,
                                       obj.AchievementId,
                                       obj.Bit
                                   });

        modelBuilder.Entity<GuildWarsAccountAchievementBitEntity>()
                    .HasOne(obj => obj.GuildWarsAccountAchievement)
                    .WithMany(obj => obj.GuildWarsAccountAchievementBits)
                    .HasForeignKey(obj => new { obj.AccountName, obj.AchievementId });

        modelBuilder.Entity<GuildWarsItemGuildUpgradeConversionEntity>()
                    .HasKey(obj => new
                                   {
                                       obj.ItemId,
                                       obj.UpgradeId
                                   });

        modelBuilder.Entity<GuildWarsAchievementFlagEntity>()
                    .HasKey(obj => new
                                   {
                                       obj.AchievementId,
                                       obj.Flag
                                   });

        modelBuilder.Entity<GuildWarsAchievementPrerequisiteEntity>()
                    .HasKey(obj => new
                                   {
                                       obj.AchievementId,
                                       obj.Id
                                   });

        modelBuilder.Entity<GuildWarsAchievementBitEntity>()
                    .HasKey(obj => new
                                   {
                                       obj.AchievementId,
                                       obj.Bit
                                   });

        modelBuilder.Entity<GuildWarsAchievementRewardEntity>()
                    .HasKey(obj => new
                                   {
                                       obj.AchievementId,
                                       obj.Counter
                                   });

        modelBuilder.Entity<GuildWarsAchievementTierEntity>()
                    .HasKey(obj => new
                                   {
                                       obj.AchievementId,
                                       obj.Counter
                                   });

        modelBuilder.Entity<GuildWarsGuildMemberEntity>()
                    .HasKey(obj => new
                                   {
                                       obj.GuildId,
                                       obj.Name
                                   });

        // Statistics
        modelBuilder.Entity<DiscordMessageEntity>();
        modelBuilder.Entity<DiscordVoiceTimeSpanEntity>();
        modelBuilder.Entity<DiscordIgnoreChannelEntity>();

        modelBuilder.Entity<DiscordMessageEntity>()
                    .HasKey(obj => new
                                   {
                                       ServerId = obj.DiscordServerId,
                                       ChannelId = obj.DiscordChannelId,
                                       MessageId = obj.DiscordMessageId
                                   });

        modelBuilder.Entity<DiscordVoiceTimeSpanEntity>()
                    .HasKey(obj => new
                                   {
                                       ServerId = obj.DiscordServerId,
                                       ChannelId = obj.DiscordChannelId,
                                       UserId = obj.DiscordAccountId,
                                       obj.StartTimeStamp
                                   });

        modelBuilder.Entity<DiscordIgnoreChannelEntity>()
                    .HasKey(obj => new
                                   {
                                       ServerId = obj.DiscordServerId,
                                       ChannelId = obj.DiscordChannelId
                                   });

        // Keyless
        modelBuilder.Entity<DateValue>(eb =>
                                       {
                                           eb.HasNoKey();
                                           eb.ToTable("__Unmapped_Query_Type_DateValue__",
                                                      t => t.ExcludeFromMigrations());
                                       });

        // Disabling cascade on delete
        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes()
                                               .SelectMany(obj => obj.GetForeignKeys())
                                               .Where(obj => obj.IsOwnership == false && obj.DeleteBehavior == DeleteBehavior.Cascade))
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }

        base.OnModelCreating(modelBuilder);
    }

    #endregion // DbContext
}