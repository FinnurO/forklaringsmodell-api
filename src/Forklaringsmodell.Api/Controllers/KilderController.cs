using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Forklaringsmodell.Api.Controllers;

[ApiController]
[Route("api/kilder")]
public class KilderController : ControllerBase
{
    private readonly KildeService _kildeService;

    public KilderController(KildeService kildeService)
    {
        _kildeService = kildeService;
    }

    [HttpGet]
    public async Task<ActionResult<List<KildeDto>>> List(CancellationToken ct) =>
        Ok(await _kildeService.ListAsync(ct));

    [HttpPost]
    public async Task<ActionResult<KildeDto>> Opprett([FromBody] OpprettKildeDto dto, CancellationToken ct)
    {
        var kilde = await _kildeService.OpprettAsync(dto, ct);
        return CreatedAtAction(nameof(List), kilde);
    }
}
