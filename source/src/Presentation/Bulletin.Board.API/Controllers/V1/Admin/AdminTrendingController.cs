using Asp.Versioning;
using Bulletin.Board.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bulletin.Board.API.Controllers.V1.Admin;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/trending")]
[Authorize(Roles = "Admin")]
public class AdminTrendingController(ITrendingScoreCalculator calculator) : ControllerBase
{
    /// <summary>
    /// Force-recomputes <c>trending_score</c> on every Approved, non-deleted, non-paused
    /// listing. Use after changing a provider's hierarchy if you need the new ranking to
    /// take effect immediately instead of waiting for the next hourly background run.
    /// </summary>
    [HttpPost("recompute")]
    [ProducesResponseType(typeof(RecomputeResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RecomputeResponse>> Recompute(CancellationToken ct)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var affected = await calculator.RecalculateAsync(ct);
        sw.Stop();

        return Ok(new RecomputeResponse(affected, sw.ElapsedMilliseconds));
    }

    public record RecomputeResponse(int Affected, long DurationMs);
}
