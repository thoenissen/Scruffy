using Microsoft.Extensions.DependencyInjection;
using Scruffy.Data.Enumerations.GuildWars2;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.Core.Exceptions.WebApi;

/// <summary>
/// Missing Guild Wars 2 API permission exception
/// </summary>
public class MissingGuildWars2ApiPermissionException : ScruffyUserMessageException
{
    #region Fields

    /// <summary>
    /// Permission
    /// </summary>
    private GuildWars2ApiPermission _permission;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="permission">Permission</param>
    public MissingGuildWars2ApiPermissionException(GuildWars2ApiPermission permission)
    {
        _permission = permission;
    }

    #endregion // Constructor

    #region ScruffyException

    /// <inheritdoc/>
    public override string GetLocalizedMessage()
    {
        using (var serviceProvider = ServiceProviderContainer.Current.GetServiceProvider())
        {
            var localizationGroup = serviceProvider.GetRequiredService<LocalizationService>()
                                                   .GetGroup(nameof(MissingGuildWars2ApiPermissionException));

            var permissions = string.Empty;

            if (_permission == GuildWars2ApiPermission.None)
            {
                permissions += localizationGroup.GetText("General", "General");
            }
            else
            {
                foreach (var permission in Enum.GetValues(typeof(GuildWars2ApiPermission)).OfType<GuildWars2ApiPermission>()
                                               .Skip(1))
                {
                    if (_permission.HasFlag(permission))
                    {
                        if (permissions.Length != 0)
                        {
                            permissions += ", ";
                        }

                        permissions += localizationGroup.GetText(permission.ToString(), permission.ToString());
                    }
                }
            }

            return serviceProvider.GetService<LocalizationService>()
                                  .GetGroup(nameof(MissingGuildWars2ApiPermissionException))
                                  .GetFormattedText("MissingPermissions", "The assigned API key does not have permissions ({0}) to execute this command.", permissions);
        }
    }

    #endregion // ScruffyException
}