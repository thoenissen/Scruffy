using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace Scruffy.Data.Entity.Tables.Web;

/// <summary>
/// User roles
/// </summary>
[Table("UserRoles")]
public class UserRoleEntity : IdentityUserRole<long>
{
}