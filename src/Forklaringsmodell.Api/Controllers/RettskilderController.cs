using Forklaringsmodell.Application.Dtos;
using Forklaringsmodell.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Forklaringsmodell.Api.Controllers;

[ApiController]
[Route("api/rettskilder")]
public class RettskilderController : ControllerBase
{
    private readonly RettskildeService _rettskildeService;

    public RettskilderController(RettskildeService rettskildeService)
    {
        _rettskildeService = rettskildeService;
    }

    [HttpGet]
    public async Task<ActionResult<List<RettskildeDto>>> List(CancellationToken ct) =>
        Ok(await _rettskildeService.ListAsync(ct));

    [HttpPost]
    public async Task<ActionResult<RettskildeDto>> Opprett([FromBody] OpprettRettskildeDto dto, CancellationToken ct)
    {
        var rettskilde = await _rettskildeService.OpprettAsync(dto, ct);
        return CreatedAtAction(nameof(List), rettskilde);
    }
}
