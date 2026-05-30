using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace DnD.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new UnauthorizedAccessException("User ID not found in token.");
        }
        return Guid.Parse(userIdClaim);
    }

    protected bool IsAdmin => User.IsInRole("Admin");
}
