using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Forklaringsmodell.Api.Controllers;

// Opprett vedtak under en sak.
[ApiController]
[Route("api/saker/{sakId:guid}/vedtak")]
public class SakVedtakController : ControllerBase
{
    private readonly VedtakService _vedtakService;

    public SakVedtakController(VedtakService vedtakService)
    {
        _vedtakService = vedtakService;
    }

    [HttpPost]
    public async Task<ActionResult<VedtakDto>> Opprett(Guid sakId, [FromBody] OpprettVedtakDto dto, CancellationToken ct)
    {
        var vedtak = await _vedtakService.OpprettAsync(sakId, dto, ct);
        return CreatedAtAction("Get", "Vedtak", new { id = vedtak.VedtakId }, vedtak);
    }
}

// Les vedtak / hydrert forklaring. Bevisst kun GET/POST mappet her (regel 3.1): ingen
// [HttpPut]/[HttpDelete]-metoder finnes for /api/vedtak/{id} eller forklaringslogg-
// relaterte ruter, slik at ASP.NET Core sin routing gir 405 Method Not Allowed naturlig
// når PUT/DELETE forsøkes mot en rute der andre verb (GET) er registrert, i stedet for
// 404 (som ville vært tilfelle om ruten ikke eksisterte i det hele tatt).
[ApiController]
[Route("api/vedtak")]
public class VedtakController : ControllerBase
{
    private readonly VedtakService _vedtakService;

    public VedtakController(VedtakService vedtakService)
    {
        _vedtakService = vedtakService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VedtakDto>> Get(Guid id, CancellationToken ct) =>
        Ok(await _vedtakService.GetAsync(id, ct));

    [HttpGet("{id:guid}/forklaring")]
    public async Task<ActionResult<HydrertForklaringDto>> GetForklaring(Guid id, CancellationToken ct) =>
        Ok(await _vedtakService.GetForklaringAsync(id, ct));

    [HttpGet("{id:guid}/virkninger")]
    public async Task<ActionResult<List<VedtaksvirkningDto>>> GetVirkninger(Guid id, CancellationToken ct) =>
        Ok(await _vedtakService.GetVirkningerAsync(id, ct));
}
