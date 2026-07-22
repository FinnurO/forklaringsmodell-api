using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Forklaringsmodell.Api.Controllers;

[ApiController]
[Route("api/saker/{sakId:guid}/relasjoner")]
public class SakRelasjonerController : ControllerBase
{
    private readonly SakRelasjonService _sakRelasjonService;

    public SakRelasjonerController(SakRelasjonService sakRelasjonService)
    {
        _sakRelasjonService = sakRelasjonService;
    }

    [HttpGet]
    public async Task<ActionResult<List<SakRelasjonDto>>> List(Guid sakId, CancellationToken ct) =>
        Ok(await _sakRelasjonService.ListForSakAsync(sakId, ct));

    [HttpPost]
    public async Task<ActionResult<SakRelasjonDto>> Opprett(Guid sakId, [FromBody] OpprettSakRelasjonDto dto, CancellationToken ct)
    {
        var relasjon = await _sakRelasjonService.OpprettAsync(sakId, dto, ct);
        return CreatedAtAction(nameof(List), new { sakId }, relasjon);
    }
}
