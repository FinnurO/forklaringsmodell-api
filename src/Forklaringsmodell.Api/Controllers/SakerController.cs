using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Forklaringsmodell.Api.Controllers;

[ApiController]
[Route("api/saker")]
public class SakerController : ControllerBase
{
    private readonly SakService _sakService;
    private readonly ForklaringService _forklaringService;

    public SakerController(SakService sakService, ForklaringService forklaringService)
    {
        _sakService = sakService;
        _forklaringService = forklaringService;
    }

    [HttpGet]
    public async Task<ActionResult<List<SakDto>>> List(CancellationToken ct) =>
        Ok(await _sakService.ListAsync(ct));

    [HttpPost]
    public async Task<ActionResult<SakDto>> Opprett([FromBody] OpprettSakDto dto, CancellationToken ct)
    {
        var sak = await _sakService.OpprettAsync(dto, ct);
        return CreatedAtAction(nameof(Get), new { id = sak.SakId }, sak);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SakDto>> Get(Guid id, CancellationToken ct) =>
        Ok(await _sakService.GetAsync(id, ct));

    /// <summary>Hele saken samlet i ett svar: levende visning, også uten vedtak. <c>erLaast</c> viser hva et vedtak har frosset.</summary>
    [HttpGet("{id:guid}/forklaring")]
    public async Task<ActionResult<SakForklaringDto>> GetForklaring(Guid id, CancellationToken ct) =>
        Ok(await _forklaringService.GetSakForklaringAsync(id, ct));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SakDto>> Oppdater(Guid id, [FromBody] OppdaterSakDto dto, CancellationToken ct) =>
        Ok(await _sakService.OppdaterAsync(id, dto, ct));
}
