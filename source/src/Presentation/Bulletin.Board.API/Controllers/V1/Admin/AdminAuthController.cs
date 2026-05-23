using Asp.Versioning;
using Bulletin.Board.Application.Commands.Admin.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bulletin.Board.API.Controllers.V1.Admin;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/auth")]
[Authorize(Roles = "Admin")]
public class AdminAuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("reauth")]
    [ProducesResponseType(typeof(ReauthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ReauthResponse>> Reauth([FromBody] ReauthRequest request, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new ReauthCommand(request.Password), ct);
            return Ok(new ReauthResponse(result.Token, result.ExpiresAt));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Problem(title: "Re-authentication failed", detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized);
        }
    }
}

public record ReauthRequest(string Password);
public record ReauthResponse(string ReauthToken, DateTimeOffset ExpiresAt);
