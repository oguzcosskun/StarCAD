using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace StarCAD.Api.Controllers;

[AllowAnonymous]
[Route("api/[controller]")]
public class HealthController : BaseApiController
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "ok", time = DateTime.UtcNow });
}