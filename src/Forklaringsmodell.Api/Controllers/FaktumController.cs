using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Forklaringsmodell.Api.Controllers;

// Faktum lest/opprettet under en sak.
[ApiController]
[Route("api/saker/{sakId:guid}/faktum")]
public class SakFaktumController : ControllerBase
{
    private readonly FaktumService _faktumService;

    public SakFaktumController(FaktumService faktumService)
    {
        _faktumService = faktumService;
    }

    [HttpGet]
    public async Task<ActionResult<List<FaktumDto>>> List(Guid sakId, CancellationToken ct) =>
        Ok(await _faktumService.ListForSakAsync(sakId, ct));

    [HttpPost]
    public async Task<ActionResult<FaktumDto>> Opprett(Guid sakId, [FromBody] OpprettFaktumDto dto, CancellationToken ct)
    {
        var faktum = await _faktumService.OpprettAsync(sakId, dto, ct);
        return CreatedAtAction("Get", "Faktum", new { id = faktum.FaktumId }, faktum);
    }
}

// Enkeltstående faktum-operasjoner (les én, transformer).
[ApiController]
[Route("api/faktum")]
public class FaktumController : ControllerBase
{
    private readonly FaktumService _faktumService;

    public FaktumController(FaktumService faktumService)
    {
        _faktumService = faktumService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FaktumDto>> Get(Guid id, CancellationToken ct) =>
        Ok(await _faktumService.GetAsync(id, ct));

    [HttpPost("{id:guid}/transformer")]
    public async Task<ActionResult<FaktumDto>> Transformer(Guid id, [FromBody] TransformerFaktumDto dto, CancellationToken ct)
    {
        var nyttFaktum = await _faktumService.TransformerAsync(id, dto, ct);
        return CreatedAtAction(nameof(Get), new { id = nyttFaktum.FaktumId }, nyttFaktum);
    }
}
