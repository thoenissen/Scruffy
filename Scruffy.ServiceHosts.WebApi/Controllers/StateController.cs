using System.Diagnostics;

using Microsoft.AspNetCore.Mvc;

namespace Scruffy.ServiceHosts.WebApi.Controllers;

/// <summary>
/// State controller
/// </summary>
[ApiController]
[Route("state")]
#if !DEBUG
[Microsoft.AspNetCore.Authorization.Authorize]
#endif
public class StateController : ControllerBase
{
    /// <summary>
    /// Get method
    /// </summary>
    /// <returns>Action result</returns>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { Uptime = DateTime.Now - Process.GetCurrentProcess().StartTime });
    }
}