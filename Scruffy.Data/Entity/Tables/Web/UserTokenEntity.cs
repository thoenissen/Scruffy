using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace Scruffy.Data.Entity.Tables.Web
{
    /// <summary>
    /// User tokens
    /// </summary>
    [Table("UserTokens")]
    public class UserTokenEntity : IdentityUserToken<long>
    {
    }
}