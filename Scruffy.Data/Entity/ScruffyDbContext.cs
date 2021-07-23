using System;
using System.Diagnostics;
using System.Linq;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity.Tables.Calendar;
using Scruffy.Data.Entity.Tables.CoreData;
using Scruffy.Data.Entity.Tables.Fractals;
using Scruffy.Data.Entity.Tables.General;
using Scruffy.Data.Entity.Tables.GuildAdministration;
using Scruffy.Data.Entity.Tables.Raid;
using Scruffy.Data.Entity.Tables.Reminder;

namespace Scruffy.Data.Entity
{
    /// <summary>
    /// Accessing the database of the discord bot
    /// </summary>
    public class ScruffyDbContext : DbContext
    {
        #region Properties

        /// <summary>
        /// Last error
        /// </summary>
        public Exception LastError { get; set; }

        #endregion // Properties

        #region DbContext

        /// <summary>
        ///     <para>
        ///         Override this method to configure the database (and other options) to be used for this context.
        ///         This method is called for each instance of the context that is created.
        ///         The base implementation does nothing.
        ///     </para>
        ///     <para>
        ///         In situations where an instance of <see cref="T:Microsoft.EntityFrameworkCore.DbContextOptions" /> may or may not have been passed
        ///         to the constructor, you can use <see cref="P:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.IsConfigured" /> to determine if
        ///         the options have already been set, and skip some or all of the logic in
        ///         <see cref="M:Microsoft.EntityFrameworkCore.DbContext.OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder)" />.
        ///     </para>
        /// </summary>
        /// <param name="optionsBuilder">
        ///     A builder used to create or modify options for this context. Databases (and other extensions)
        ///     typically define extension methods on this object that allow you to configure the context.
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

            optionsBuilder.UseSqlServer(connectionStringBuilder.ConnectionString);

#if DEBUG
            optionsBuilder.LogTo(s => Debug.WriteLine(s));
#endif

            base.OnConfiguring(optionsBuilder);
        }

        /// <summary>
        ///     Override this method to further configure the model that was discovered by convention from the entity types
        ///     exposed in <see cref="T:Microsoft.EntityFrameworkCore.DbSet`1" /> properties on your derived context. The resulting model may be cached
        ///     and re-used for subsequent instances of your derived context.
        /// </summary>
        /// <remarks>
        ///     If a model is explicitly set on the options for this context (via <see cref="M:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel)" />)
        ///     then this method will not be run.
        /// </remarks>
        /// <param name="modelBuilder">
        ///     The builder being used to construct the model for this context. Databases (and other extensions) typically
        ///     define extension methods on this object that allow you to configure aspects of the model that are specific
        ///     to a given database.
        /// </param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // CoreData
            modelBuilder.Entity<UserEntity>();
            modelBuilder.Entity<ServerConfigurationEntity>();

            modelBuilder.Entity<UserEntity>()
                        .HasMany(obj => obj.OneTimeReminders)
                        .WithOne(obj => obj.User)
                        .HasForeignKey(obj => obj.UserId)
                        .IsRequired();

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
                                           obj.UserId,
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

            modelBuilder.Entity<RaidDayTemplateEntity>()
                        .HasMany(obj => obj.RaidAppointments)
                        .WithOne(obj => obj.RaidDayTemplateEntity)
                        .HasForeignKey(obj => obj.TemplateId);

            modelBuilder.Entity<RaidDayTemplateEntity>()
                        .HasMany(obj => obj.RaidExperienceAssignments)
                        .WithOne(obj => obj.RaidDayTemplateEntity)
                        .HasForeignKey(obj => obj.TemplateId);

            // Reminder
            modelBuilder.Entity<OneTimeReminderEntity>();
            modelBuilder.Entity<WeeklyReminderEntity>();

            // Calendar
            modelBuilder.Entity<CalendarAppointmentEntity>();
            modelBuilder.Entity<CalendarAppointmentTemplateEntity>();
            modelBuilder.Entity<CalendarAppointmentScheduleEntity>();

            modelBuilder.Entity<CalendarAppointmentTemplateEntity>()
                        .HasMany(obj => obj.CalendarAppointments)
                        .WithOne(obj => obj.CalendarAppointmentTemplate)
                        .HasForeignKey(obj => obj.CalendarAppointmentTemplateId);

            modelBuilder.Entity<CalendarAppointmentTemplateEntity>()
                        .HasMany(obj => obj.CalendarAppointmentSchedules)
                        .WithOne(obj => obj.CalendarAppointmentTemplate)
                        .HasForeignKey(obj => obj.CalendarAppointmentTemplateId);

            // Guild
            modelBuilder.Entity<GuildEntity>();
            modelBuilder.Entity<GuildLogEntryEntity>();

            modelBuilder.Entity<GuildLogEntryEntity>()
                        .HasKey(obj => new
                                       {
                                           obj.GuildId,
                                           obj.Id
                                       });

            modelBuilder.Entity<GuildEntity>()
                        .HasMany(obj => obj.GuildLogEntries)
                        .WithOne(obj => obj.Guild)
                        .HasForeignKey(obj => obj.GuildId);

            // Disabling cascade on delete
            foreach (var foreignKey in modelBuilder.Model
                                                   .GetEntityTypes()
                                                   .SelectMany(obj => obj.GetForeignKeys())
                                                   .Where(obj => obj.IsOwnership == false
                                                              && obj.DeleteBehavior == DeleteBehavior.Cascade))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }

            base.OnModelCreating(modelBuilder);
        }

        #endregion // DbContext
    }
}
