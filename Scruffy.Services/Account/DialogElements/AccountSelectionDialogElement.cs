using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.Entities;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Account;
using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Account.DialogElements
{
    /// <summary>
    /// Account name selection
    /// </summary>
    public class AccountSelectionDialogElement : DialogEmbedMessageElementBase<string>
    {
        #region Fields

        /// <summary>
        /// Accounts
        /// </summary>
        private Dictionary<int, string> _accounts;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public AccountSelectionDialogElement(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region DialogEmbedMessageElementBase<string>

        /// <summary>
        /// Return the message of element
        /// </summary>
        /// <returns>Message</returns>
        public override DiscordEmbedBuilder GetMessage()
        {
            var builder = new DiscordEmbedBuilder();
            builder.WithTitle(LocalizationGroup.GetText("ChooseTitle", "Account selection"));
            builder.WithDescription(LocalizationGroup.GetText("ChooseDescription", "Please choose one of the following accounts:"));

            _accounts = new Dictionary<int, string>();
            var typesField = new StringBuilder();

            var i = 1;

            using (var dbFactory = RepositoryFactory.CreateInstance())
            {
                foreach (var accountName in dbFactory.GetRepository<AccountRepository>()
                                                     .GetQuery()
                                                     .Where(obj => obj.User.DiscordAccounts.Any(obj2 => obj2.Id == CommandContext.User.Id))
                                                     .Select(obj => obj.Name))
                {
                    typesField.Append('`');
                    typesField.Append(i);
                    typesField.Append("` - ");
                    typesField.Append(' ');
                    typesField.Append(accountName);
                    typesField.Append('\n');

                    _accounts[i] = accountName;

                    i++;
                }
            }

            builder.AddField(LocalizationGroup.GetText("AccountsField", "Accounts"), typesField.ToString());

            return builder;
        }

        /// <summary>
        /// Converting the response message
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override Task<string> ConvertMessage(DiscordMessage message)
        {
            return Task.FromResult(int.TryParse(message.Content, out var index)
                                && _accounts.TryGetValue(index, out var selectedAccount)
                                       ? selectedAccount
                                       : throw new InvalidOperationException());
        }

        #endregion // DialogEmbedMessageElementBase<string>
    }
}