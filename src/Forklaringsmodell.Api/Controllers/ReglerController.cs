using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Forklaringsmodell.Api.Controllers;

[ApiController]
[Route("api/regler")]
public class ReglerController : ControllerBase
{
    private readonly RegelService _regelService;

    public ReglerController(RegelService regelService)
    {
        _regelService = regelService;
    }

    [HttpGet]
    public async Task<ActionResult<List<RegelDto>>> List(CancellationToken ct) =>
        Ok(await _regelService.ListAsync(ct));

    [HttpPost]
    public async Task<ActionResult<RegelDto>> Opprett([FromBody] OpprettRegelDto dto, CancellationToken ct)
    {
        var regel = await _regelService.OpprettAsync(dto, ct);
        return CreatedAtAction(nameof(List), regel);
    }
}
