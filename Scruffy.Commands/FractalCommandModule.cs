using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

using Microsoft.EntityFrameworkCore;

using Scruffy.Commands.Base;
using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Fractals;
using Scruffy.Data.Entity.Tables.Fractals;
using Scruffy.Services.Core;
using Scruffy.Services.CoreData;
using Scruffy.Services.Fractals;

namespace Scruffy.Commands.Fractals
{
    /// <summary>
    /// Fractal lfg setup commands
    /// </summary>
    [Group("fractal")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class FractalCommandModule : LocatedCommandModuleBase
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public FractalCommandModule(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region Properties

        /// <summary>
        /// User management service
        /// </summary>
        public UserManagementService UserManagementService { get; set; }

        /// <summary>
        /// Message builder
        /// </summary>
        public FractalLfgMessageBuilder MessageBuilder { get; set; }

        /// <summary>
        /// Fractal reminder service
        /// </summary>
        public FractalReminderService FractalReminderService { get; set; }

        #endregion // Properties

        #region Command methods

        /// <summary>
        /// Creation of a new lfg entry
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("setup")]
        public async Task Setup(CommandContext commandContext)
        {
            var messagesToBeDeleted = new List<DiscordMessage>
                                      {
                                          commandContext.Message
                                      };

            string title = null;
            string description = null;

            var interactivity = commandContext.Client.GetInteractivity();

            var currentBotMessage = await commandContext.RespondAsync(LocalizationGroup.GetText("TitlePrompt", "Please enter a title.")).ConfigureAwait(false);

            messagesToBeDeleted.Add(currentBotMessage);

            var responseMessage = await interactivity.WaitForMessageAsync(obj => obj.Author.Id == commandContext.Message.Author.Id).ConfigureAwait(false);

            if (responseMessage.TimedOut == false)
            {
                title = responseMessage.Result.Content;

                currentBotMessage = await responseMessage.Result.RespondAsync(LocalizationGroup.GetText("DescriptionPrompt", "Please enter a description.")).ConfigureAwait(false);

                messagesToBeDeleted.Add(currentBotMessage);

                responseMessage = await interactivity.WaitForMessageAsync(obj => obj.Author.Id == commandContext.Message.Author.Id).ConfigureAwait(false);
            }

            if (responseMessage.TimedOut == false)
            {
                description = responseMessage.Result.Content;

                currentBotMessage = await responseMessage.Result.RespondAsync(LocalizationGroup.GetText("AliasNamePrompt", "Please enter an alias name.")).ConfigureAwait(false);

                messagesToBeDeleted.Add(currentBotMessage);

                responseMessage = await interactivity.WaitForMessageAsync(obj => obj.Author.Id == commandContext.Message.Author.Id).ConfigureAwait(false);
            }

            if (responseMessage.TimedOut == false)
            {
                var aliasName = responseMessage.Result.Content;

                var entry = new FractalLfgConfigurationEntity
                            {
                                Title = title,
                                Description = description,
                                AliasName = aliasName,
                                ChannelId = commandContext.Channel.Id,
                                MessageId = (await commandContext.Channel.SendMessageAsync(LocalizationGroup.GetText("BuildingProgress", "Building...")).ConfigureAwait(false)).Id
                            };

                using (var dbFactory = RepositoryFactory.CreateInstance())
                {
                    dbFactory.GetRepository<FractalLfgConfigurationRepository>()
                             .Add(entry);
                }

                await MessageBuilder.RefreshMessageAsync(entry.Id).ConfigureAwait(false);

                currentBotMessage = await commandContext.Channel.SendMessageAsync(LocalizationGroup.GetText("CreationCompletedMessage", "Creation completed! All creation messages will be deleted in 30 seconds.")).ConfigureAwait(false);

                messagesToBeDeleted.Add(currentBotMessage);

                await Task.Delay(TimeSpan.FromSeconds(30)).ConfigureAwait(false);

                foreach (var message in messagesToBeDeleted)
                {
                    await commandContext.Channel.DeleteMessageAsync(message).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Joining an appointment
        /// </summary>
        /// <param name="commandContext">Context</param>
        /// <param name="alias">Lfg alias</param>
        /// <param name="timeSpan">Time</param>
        /// <param name="days">Days</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Command("join")]
        public async Task Join(CommandContext commandContext, string alias, string timeSpan, params string[] days)
        {
            await EvaluateRegistrationArguments(commandContext,
                                                new List<string> { alias, timeSpan }.Concat(days),
                                                e =>
                                                {
                                                    using (var dbFactory = RepositoryFactory.CreateInstance())
                                                    {
                                                        dbFactory.GetRepository<FractalRegistrationRepository>()
                                                                 .AddOrRefresh(obj => obj.ConfigurationId == e.ConfigurationId
                                                                                   && obj.AppointmentTimeStamp == e.AppointmentTimeStamp
                                                                                   && obj.UserId == commandContext.User.Id,
                                                                               obj =>
                                                                               {
                                                                                   obj.ConfigurationId = e.ConfigurationId;
                                                                                   obj.AppointmentTimeStamp = e.AppointmentTimeStamp;
                                                                                   obj.UserId = commandContext.User.Id;
                                                                                   obj.RegistrationTimeStamp = DateTime.Now;
                                                                               });
                                                    }
                                                }).ConfigureAwait(false);
        }

        /// <summary>
        /// Joining an appointment
        /// </summary>
        /// <param name="commandContext">Context</param>
        /// <param name="timeSpan">Time</param>
        /// <param name="days">Days</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Command("join")]
        public async Task Join(CommandContext commandContext, string timeSpan, params string[] days)
        {
            await EvaluateRegistrationArguments(commandContext,
                                                new List<string> { timeSpan }.Concat(days),
                                                e =>
                                                {
                                                    using (var dbFactory = RepositoryFactory.CreateInstance())
                                                    {
                                                        dbFactory.GetRepository<FractalRegistrationRepository>()
                                                               .AddOrRefresh(obj => obj.ConfigurationId == e.ConfigurationId
                                                                                 && obj.AppointmentTimeStamp == e.AppointmentTimeStamp
                                                                                 && obj.UserId == commandContext.User.Id,
                                                                             obj =>
                                                                             {
                                                                                 obj.ConfigurationId = e.ConfigurationId;
                                                                                 obj.AppointmentTimeStamp = e.AppointmentTimeStamp;
                                                                                 obj.UserId = commandContext.User.Id;
                                                                                 obj.RegistrationTimeStamp = DateTime.Now;
                                                                             });
                                                    }
                                                }).ConfigureAwait(false);
        }

        /// <summary>
        /// Leaving an appointment
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <param name="alias">Lfg alias</param>
        /// <param name="timeSpan">Time</param>
        /// <param name="days">Days</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("leave")]
        public async Task Leave(CommandContext commandContext, string alias, string timeSpan, params string[] days)
        {
            await EvaluateRegistrationArguments(commandContext,
                                                new List<string> { alias, timeSpan }.Concat(days),
                                                e =>
                                                {
                                                    using (var dbFactory = RepositoryFactory.CreateInstance())
                                                    {
                                                        dbFactory.GetRepository<FractalRegistrationRepository>()
                                                                 .RemoveRange(obj => obj.ConfigurationId == e.ConfigurationId
                                                                                   && obj.AppointmentTimeStamp == e.AppointmentTimeStamp
                                                                                   && obj.UserId == commandContext.User.Id);
                                                    }
                                                }).ConfigureAwait(false);
        }

        /// <summary>
        /// Leaving an appointment
        /// </summary>
        /// <param name="commandContext">Current command context</param>
        /// <param name="timeSpan">Time</param>
        /// <param name="days">Days</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        [Command("leave")]
        public async Task Leave(CommandContext commandContext, string timeSpan, params string[] days)
        {
            await EvaluateRegistrationArguments(commandContext,
                                                new List<string> { timeSpan }.Concat(days),
                                                e =>
                                                {
                                                    using (var dbFactory = RepositoryFactory.CreateInstance())
                                                    {
                                                        dbFactory.GetRepository<FractalRegistrationRepository>()
                                                                 .RemoveRange(obj => obj.ConfigurationId == e.ConfigurationId
                                                                                  && obj.AppointmentTimeStamp == e.AppointmentTimeStamp
                                                                                  && obj.UserId == commandContext.User.Id);
                                                    }
                                                }).ConfigureAwait(false);
        }

        #endregion // Command methods

        #region Private methods

        /// <summary>
        /// Evaluation of the join or leave arguments
        /// </summary>
        /// <param name="commandContext">Context</param>
        /// <param name="argumentsEnumerable">Arguments</param>
        /// <param name="action">Action</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        private async Task EvaluateRegistrationArguments(CommandContext commandContext, IEnumerable<string> argumentsEnumerable, Action<(int ConfigurationId, DateTime AppointmentTimeStamp)> action)
        {
            var arguments = argumentsEnumerable.ToList();
            if (arguments.Count >= 2)
            {
                var checkUser = UserManagementService.CheckUserAsync(commandContext.User.Id);

                using (var dbFactory = RepositoryFactory.CreateInstance())
                {
                    TimeSpan? timeSpan = null;
                    int? configurationId = null;
                    var argumentsIndex = 0;

                    var timeValidation = new Regex(@"\d\d:\d\d");
                    if (timeValidation.IsMatch(arguments[0]))
                    {
                        timeSpan = TimeSpan.ParseExact(arguments[0], "hh\\:mm", CultureInfo.InvariantCulture);
                        configurationId = await dbFactory.GetRepository<FractalLfgConfigurationRepository>()
                                                         .GetQuery()
                                                         .Where(obj => obj.ChannelId == commandContext.Channel.Id)
                                                         .Select(obj => (int?)obj.Id)
                                                         .FirstOrDefaultAsync()
                                                         .ConfigureAwait(false);

                        argumentsIndex = 1;
                    }
                    else if (timeValidation.IsMatch(arguments[1]))
                    {
                        timeSpan = TimeSpan.ParseExact(arguments[1], "hh\\:mm", CultureInfo.InvariantCulture);
                        configurationId = await dbFactory.GetRepository<FractalLfgConfigurationRepository>()
                                                         .GetQuery()
                                                         .Where(obj => obj.AliasName == arguments[0])
                                                         .Select(obj => (int?)obj.Id)
                                                         .FirstOrDefaultAsync()
                                                         .ConfigureAwait(false);

                        argumentsIndex = 2;
                    }

                    if (timeSpan != null
                     && configurationId != null)
                    {
                        var earliestTimeStamp = default(DateTime?);

                        var isNumericValidation = new Regex(@"\d+");

                        foreach (var dayArgument in arguments.Skip(argumentsIndex))
                        {
                            DateTime? appointmentTimeStamp = null;

                            if (isNumericValidation.IsMatch(dayArgument))
                            {
                                if (int.TryParse(dayArgument, out var add)
                                 && add >= 0
                                 && add < 8)
                                {
                                    appointmentTimeStamp = DateTime.Today
                                                                   .AddDays(add)
                                                                   .Add(timeSpan.Value);
                                }
                            }
                            else if (LocalizationGroup.GetText("Today", "Today").Equals(dayArgument, StringComparison.InvariantCultureIgnoreCase))
                            {
                                appointmentTimeStamp = DateTime.Today
                                                               .Add(timeSpan.Value);
                            }
                            else
                            {
                                var appointmentDay = DateTime.Today.AddDays(1);

                                for (var i = 0; i < 7; i++)
                                {
                                    if (LocalizationGroup.CultureInfo.DateTimeFormat.GetDayName(appointmentDay.DayOfWeek).StartsWith(dayArgument, StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        appointmentTimeStamp = appointmentDay.Add(timeSpan.Value);
                                        break;
                                    }

                                    appointmentDay = appointmentDay.AddDays(1);
                                }
                            }

                            if (appointmentTimeStamp != null
                                && appointmentTimeStamp > DateTime.Now.AddHours(2))
                            {
                                await checkUser.ConfigureAwait(false);

                                action((configurationId.Value, appointmentTimeStamp.Value));

                                if (earliestTimeStamp == null
                                    || earliestTimeStamp > appointmentTimeStamp)
                                {
                                    earliestTimeStamp = appointmentTimeStamp;
                                }
                            }
                        }

                        await MessageBuilder.RefreshMessageAsync(configurationId.Value).ConfigureAwait(false);

                        await commandContext.Channel
                                            .DeleteMessageAsync(commandContext.Message)
                                            .ConfigureAwait(false);

                        if (earliestTimeStamp != null)
                        {
                            await FractalReminderService.RefreshNextReminderJobAsync(earliestTimeStamp.Value)
                                                        .ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        #endregion // Private methods
    }
}
