using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Forklaringsmodell.Api.Controllers;

[ApiController]
[Route("api/saker/{sakId:guid}/partsmedvirkning")]
public class PartsmedvirkningController : ControllerBase
{
    private readonly PartsmedvirkningService _partsmedvirkningService;

    public PartsmedvirkningController(PartsmedvirkningService partsmedvirkningService)
    {
        _partsmedvirkningService = partsmedvirkningService;
    }

    [HttpGet]
    public async Task<ActionResult<List<PartsmedvirkningDto>>> List(Guid sakId, CancellationToken ct) =>
        Ok(await _partsmedvirkningService.ListForSakAsync(sakId, ct));

    [HttpPost]
    public async Task<ActionResult<PartsmedvirkningDto>> Opprett(Guid sakId, [FromBody] OpprettPartsmedvirkningDto dto, CancellationToken ct)
    {
        var partsmedvirkning = await _partsmedvirkningService.OpprettAsync(sakId, dto, ct);
        return CreatedAtAction(nameof(List), new { sakId }, partsmedvirkning);
    }
}
