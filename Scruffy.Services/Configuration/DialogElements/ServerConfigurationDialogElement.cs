using System.Reflection;

using Discord;

using Scruffy.Services.Core.Localization;
using Scruffy.Services.Discord;

namespace Scruffy.Services.Configuration.DialogElements;

/// <summary>
/// Server configuration
/// </summary>
public class ServerConfigurationDialogElement : DialogEmbedSelectMenuElementBase<bool>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public ServerConfigurationDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogSelectMenuElementBase<bool>

    /// <inheritdoc/>
    public override Task<EmbedBuilder> GetMessage()
    {
        var builder = new EmbedBuilder().WithTitle(LocalizationGroup.GetText("Title", "Server configuration"))
                                        .WithDescription(LocalizationGroup.GetText("Description", "With the following assistant you are able to configure your server."))
                                        .WithFooter("Scruffy", "https://cdn.discordapp.com/app-icons/838381119585648650/823930922cbe1e5a9fa8552ed4b2a392.png?size=64")
                                        .WithColor(Color.Green)
                                        .WithTimestamp(DateTime.Now);

        return Task.FromResult(builder);
    }

    /// <inheritdoc/>
    public override string GetPlaceholder() => LocalizationGroup.GetText("ChooseAction", "Choose one of the following options...");

    /// <inheritdoc/>
    public override IReadOnlyList<SelectMenuEntryData<bool>> GetEntries()
    {
        return [
                   new SelectMenuEntryData<bool>
                   {
                       CommandText = LocalizationGroup.GetText("InstallCommands", "Command installation"),
                       Response = async () =>
                              {
                                  IEnumerable<ApplicationCommandProperties> commands = null;

                                  var buildContext = new SlashCommandBuildContext
                                                     {
                                                         Guild = CommandContext.Guild,
                                                         ServiceProvider = CommandContext.ServiceProvider,
                                                         CultureInfo = LocalizationGroup.CultureInfo
                                                     };

                                  foreach (var type in Assembly.Load("Scruffy.Commands")
                                                               .GetTypes()
                                                               .Where(obj => typeof(SlashCommandModuleBase).IsAssignableFrom(obj)
                                                                          && obj.IsAbstract == false))
                                  {
                                      var commandModule = (SlashCommandModuleBase)Activator.CreateInstance(type);

                                      if (commandModule != null)
                                      {
                                          commands = commands?.Concat(commandModule.GetCommands(buildContext)) ?? commandModule.GetCommands(buildContext);
                                      }
                                  }

                                  foreach (var type in Assembly.Load("Scruffy.Commands")
                                                               .GetTypes()
                                                               .Where(obj => typeof(MessageCommandModuleBase).IsAssignableFrom(obj)
                                                                          && obj.IsAbstract == false))
                                  {
                                      var commandModule = (MessageCommandModuleBase)Activator.CreateInstance(type);

                                      if (commandModule != null)
                                      {
                                          commands = commands?.Concat(commandModule.GetCommands(buildContext)) ?? commandModule.GetCommands(buildContext);
                                      }
                                  }

                                  if (commands != null)
                                  {
                                      await CommandContext.Guild
                                                          .BulkOverwriteApplicationCommandsAsync(commands.ToArray())
                                                          .ConfigureAwait(false);
                                  }

                                  return true;
                              }
                   },
                   new SelectMenuEntryData<bool>
                   {
                       CommandText = LocalizationGroup.GetText("UninstallCommands", "Command uninstallation"),
                       Response = async () =>
                              {
                                  await CommandContext.Guild
                                                      .BulkOverwriteApplicationCommandsAsync([])
                                                      .ConfigureAwait(false);

                                  return true;
                              }
                   }
               ];
    }

    /// <inheritdoc/>
    protected override bool DefaultFunc() => false;

    #endregion // DialogSelectMenuElementBase<bool>
}