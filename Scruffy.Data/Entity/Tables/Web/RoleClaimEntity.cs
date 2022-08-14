using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace Scruffy.Data.Entity.Tables.Web
{
    /// <summary>
    /// Role claims
    /// </summary>
    [Table("RoleClaims")]
    public class RoleClaimEntity : IdentityRoleClaim<long>
    {
    }
}