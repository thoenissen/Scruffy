using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace Scruffy.Data.Entity.Tables.Web;

/// <summary>
/// User logins
/// </summary>
[Table("UserLogins")]
public class UserLoginEntity : IdentityUserLogin<long>
{
}