using Microsoft.AspNetCore.Mvc;

namespace OpoMatic3000.Api.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "ok",
            application = "OpoMatic-3000"
        });
    }
}
