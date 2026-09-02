using Microsoft.AspNetCore.Mvc;

namespace TraceFlow.Api.Controllers;

[ApiController]
[Route("api/system")]
public class SystemController : ControllerBase
{
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            status = "ok"
        });
    }
}