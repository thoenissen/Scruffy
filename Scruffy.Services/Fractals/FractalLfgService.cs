using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Fractals;
using Scruffy.Data.Entity.Tables.Fractals;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.CoreData;
using Scruffy.Services.Fractals.DialogElements.Forms;

namespace Scruffy.Services.Fractals;

/// <summary>
/// Fractal registration service
/// </summary>
public class FractalLfgService : LocatedServiceBase
{
    #region Fields

    /// <summary>
    /// User management
    /// </summary>
    private readonly UserManagementService _userManagementService;

    /// <summary>
    /// Message builder
    /// </summary>
    private readonly FractalLfgMessageBuilder _messageBuilder;

    /// <summary>
    /// Reminder service
    /// </summary>
    private readonly FractalLfgReminderService _reminderService;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    /// <param name="userManagementService">User management</param>
    /// <param name="messageBuilder">Message builder</param>
    /// <param name="reminderService">Reminder service</param>
    public FractalLfgService(LocalizationService localizationService, UserManagementService userManagementService, FractalLfgMessageBuilder messageBuilder, FractalLfgReminderService reminderService)
        : base(localizationService)
    {
        _userManagementService = userManagementService;
        _messageBuilder = messageBuilder;
        _reminderService = reminderService;
    }

    #endregion // Constructor

    #region Methods

    #region Public methods

    /// <summary>
    /// Setup assistant
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RunSetupAssistant(CommandContextContainer commandContext)
    {
        var dialogHandler = new DialogHandler(commandContext);
        await using (dialogHandler.ConfigureAwait(false))
        {
            var creationData = await dialogHandler.RunForm<FractalLfgCreationFormData>()
                                                  .ConfigureAwait(false);

            var entry = new FractalLfgConfigurationEntity
                        {
                            Title = creationData.Title,
                            Description = creationData.Description,
                            AliasName = creationData.AliasName,
                            DiscordChannelId = commandContext.Channel.Id,
                            DiscordMessageId = (await commandContext.Channel.SendMessageAsync(LocalizationGroup.GetText("BuildingProgress", "Building...")).ConfigureAwait(false)).Id
                        };

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                dbFactory.GetRepository<FractalLfgConfigurationRepository>()
                         .Add(entry);
            }

            await _messageBuilder.RefreshMessageAsync(entry.Id).ConfigureAwait(false);

            var currentBotMessage = await commandContext.Channel
                                                        .SendMessageAsync(LocalizationGroup.GetText("CreationCompletedMessage", "Creation completed! All creation messages will be deleted in 30 seconds.")).ConfigureAwait(false);

            dialogHandler.DialogContext.Messages.Add(currentBotMessage);

            await Task.Delay(TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            await dialogHandler.DeleteMessages()
                               .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Join
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <param name="arguments">Arguments</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task Join(CommandContextContainer commandContext, IEnumerable<string> arguments)
    {
        return EvaluateRegistrationArguments(commandContext,
                                             arguments,
                                             async e =>
                                             {
                                                 var user = await commandContext.GetCurrentUser()
                                                                                .ConfigureAwait(false);

                                                 using (var dbFactory = RepositoryFactory.CreateInstance())
                                                 {
                                                     dbFactory.GetRepository<FractalRegistrationRepository>()
                                                              .AddOrRefresh(obj => obj.ConfigurationId == e.ConfigurationId
                                                                                && obj.AppointmentTimeStamp == e.AppointmentTimeStamp
                                                                                && obj.UserId == user.Id,
                                                                            obj =>
                                                                            {
                                                                                obj.ConfigurationId = e.ConfigurationId;
                                                                                obj.AppointmentTimeStamp = e.AppointmentTimeStamp;
                                                                                obj.UserId = user.Id;
                                                                                obj.RegistrationTimeStamp = DateTime.Now;
                                                                            });
                                                 }
                                             });
    }

    /// <summary>
    /// Leave
    /// </summary>
    /// <param name="commandContext">Command context</param>
    /// <param name="arguments">Arguments</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task Leave(CommandContextContainer commandContext, IEnumerable<string> arguments)
    {
        return EvaluateRegistrationArguments(commandContext,
                                             arguments,
                                             async e =>
                                             {
                                                 var user = await commandContext.GetCurrentUser()
                                                                                .ConfigureAwait(false);

                                                 using (var dbFactory = RepositoryFactory.CreateInstance())
                                                 {
                                                     dbFactory.GetRepository<FractalRegistrationRepository>()
                                                              .RemoveRange(obj => obj.ConfigurationId == e.ConfigurationId
                                                                               && obj.AppointmentTimeStamp == e.AppointmentTimeStamp
                                                                               && obj.UserId == user.Id);
                                                 }
                                             });
    }

    #endregion // Public methods

    #region Private methods

    /// <summary>
    /// Evaluation of the join or leave arguments
    /// </summary>
    /// <param name="commandContextContainer">Context</param>
    /// <param name="argumentsEnumerable">Arguments</param>
    /// <param name="action">Action</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task EvaluateRegistrationArguments(CommandContextContainer commandContextContainer, IEnumerable<string> argumentsEnumerable, Func<(int ConfigurationId, DateTime AppointmentTimeStamp), Task> action)
    {
        var arguments = argumentsEnumerable.ToList();
        if (arguments.Count >= 2)
        {
            var checkUser = _userManagementService.CheckDiscordAccountAsync(commandContextContainer.User.Id);

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
                                                     .Where(obj => obj.DiscordChannelId == commandContextContainer.Channel.Id)
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
                             && add is >= 0 and < 8)
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

                            await action((configurationId.Value, appointmentTimeStamp.Value)).ConfigureAwait(false);

                            if (earliestTimeStamp == null
                             || earliestTimeStamp > appointmentTimeStamp)
                            {
                                earliestTimeStamp = appointmentTimeStamp;
                            }
                        }
                    }

                    await _messageBuilder.RefreshMessageAsync(configurationId.Value).ConfigureAwait(false);

                    await commandContextContainer.Channel
                                                 .DeleteMessageAsync(commandContextContainer.Message)
                                                 .ConfigureAwait(false);

                    if (earliestTimeStamp != null)
                    {
                        await _reminderService.RefreshNextReminderJobAsync(earliestTimeStamp.Value)
                                              .ConfigureAwait(false);
                    }
                }
            }
        }
    }

    #endregion // Private methods

    #endregion // Methods
}