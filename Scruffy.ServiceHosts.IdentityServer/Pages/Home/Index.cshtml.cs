using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Scruffy.ServiceHosts.IdentityServer.Pages.Home;

/// <summary>
/// Main page
/// </summary>
[AllowAnonymous]
public class Index : PageModel
{
}