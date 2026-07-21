using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Forklaringsmodell.Api.Controllers;

[ApiController]
[Route("api/saker/{sakId:guid}/vurderinger")]
public class SakVurderingerController : ControllerBase
{
    private readonly VurderingService _vurderingService;

    public SakVurderingerController(VurderingService vurderingService)
    {
        _vurderingService = vurderingService;
    }

    [HttpGet]
    public async Task<ActionResult<List<VurderingDto>>> List(Guid sakId, CancellationToken ct) =>
        Ok(await _vurderingService.ListForSakAsync(sakId, ct));

    [HttpPost]
    public async Task<ActionResult<VurderingDto>> Opprett(Guid sakId, [FromBody] OpprettVurderingDto dto, CancellationToken ct)
    {
        var vurdering = await _vurderingService.OpprettAsync(sakId, dto, ct);
        return CreatedAtAction("Get", "Vurderinger", new { id = vurdering.VurderingId }, vurdering);
    }
}

[ApiController]
[Route("api/vurderinger")]
public class VurderingerController : ControllerBase
{
    private readonly VurderingService _vurderingService;

    public VurderingerController(VurderingService vurderingService)
    {
        _vurderingService = vurderingService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VurderingDto>> Get(Guid id, CancellationToken ct) =>
        Ok(await _vurderingService.GetAsync(id, ct));
}
