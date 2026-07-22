using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Forklaringsmodell.Api.Controllers;

[ApiController]
[Route("api/vilkar")]
public class VilkarController : ControllerBase
{
    private readonly VilkarService _vilkarService;

    public VilkarController(VilkarService vilkarService)
    {
        _vilkarService = vilkarService;
    }

    [HttpGet]
    public async Task<ActionResult<List<VilkarDto>>> List(CancellationToken ct) =>
        Ok(await _vilkarService.ListAsync(ct));

    [HttpPost]
    public async Task<ActionResult<VilkarDto>> Opprett([FromBody] OpprettVilkarDto dto, CancellationToken ct)
    {
        var vilkar = await _vilkarService.OpprettAsync(dto, ct);
        return CreatedAtAction(nameof(List), vilkar);
    }
}
