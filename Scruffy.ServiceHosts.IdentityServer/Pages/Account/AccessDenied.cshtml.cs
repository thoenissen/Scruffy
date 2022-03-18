using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Scruffy.ServiceHosts.IdentityServer.Pages.Account;

/// <summary>
/// Access Denied page
/// </summary>
[AllowAnonymous]
public class AccessDeniedModel : PageModel
{
}