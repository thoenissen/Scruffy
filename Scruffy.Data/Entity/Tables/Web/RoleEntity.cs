using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace Scruffy.Data.Entity.Tables.Web
{
    /// <summary>
    /// Roles
    /// </summary>
    [Table("Roles")]
    public class RoleEntity : IdentityRole<long>
    {
    }
}