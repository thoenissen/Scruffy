using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.Entities;

using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.WebApi;

namespace Scruffy.Services.GuildAdministration.DialogElements;

/// <summary>
/// Selection of a template
/// </summary>
public class GuildAdministrationGuildDialogElement : DialogEmbedMessageElementBase<string>
{
    #region Fields

    /// <summary>
    /// Templates
    /// </summary>
    private Dictionary<int, string> _guilds;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="localizationService">Localization service</param>
    public GuildAdministrationGuildDialogElement(LocalizationService localizationService)
        : base(localizationService)
    {
    }

    #endregion // Constructor

    #region DialogEmbedMessageElementBase<long>

    /// <summary>
    /// Return the message of element
    /// </summary>
    /// <returns>Message</returns>
    public override DiscordEmbedBuilder GetMessage()
    {
        var builder = new DiscordEmbedBuilder();
        builder.WithTitle(LocalizationGroup.GetText("ChooseGuildTitle", "Guild selection"));
        builder.WithDescription(LocalizationGroup.GetText("ChooseGuildDescription", "Please choose one of the following guilds:"));

        _guilds = new Dictionary<int, string>();

        using (var connector = new GuidWars2ApiConnector(DialogContext.GetValue<string>("ApiKey")))
        {
            var fieldText = new StringBuilder();
            var i = 1;

            // TODO GetMessage -> GetMessageAsync
            var accountInformation = connector.GetAccountInformationAsync().Result;

            foreach (var guildId in accountInformation.GuildLeader)
            {
                var guildInformation = connector.GetGuildInformation(guildId).Result;

                fieldText.Append('`');
                fieldText.Append(i);
                fieldText.Append("` - ");
                fieldText.Append(' ');
                fieldText.Append(guildInformation.Name);
                fieldText.Append('[');
                fieldText.Append(guildInformation.Tag);
                fieldText.Append(']');
                fieldText.Append('\n');

                _guilds[i] = guildId;

                i++;
            }

            builder.AddField(LocalizationGroup.GetText("GuildsField", "Guilds"), fieldText.ToString());
        }

        return builder;
    }

    /// <summary>
    /// Converting the response message
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override Task<string> ConvertMessage(DiscordMessage message)
    {
        return Task.FromResult(int.TryParse(message.Content, out var index) && _guilds.TryGetValue(index, out var selectedGuild) ? selectedGuild : throw new InvalidOperationException());
    }

    #endregion // DialogEmbedMessageElementBase<long>
}