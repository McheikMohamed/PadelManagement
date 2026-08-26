using Microsoft.AspNetCore.Mvc;
using Padel.Api.Middleware;
using Padel.Application.Dtos;
using Padel.Application.Interfaces;

namespace Padel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaiementsController : ControllerBase
{
    private readonly IPaiementService _paiementService;

    public PaiementsController(IPaiementService paiementService)
    {
        _paiementService = paiementService;
    }

    [HttpPost("{inscriptionId}")]
    public async Task<ActionResult<PaiementDto>> TraiterPaiement(int inscriptionId)
    {
        var identite = (IdentiteAppelant)HttpContext.Items["Identite"]!;

        var paiement = await _paiementService.TraiterPaiementAsync(inscriptionId, identite.Matricule);

        return Ok(paiement);
    }

    [HttpPost("{paiementId}/remboursement")]
    public async Task<IActionResult> Rembourser(int paiementId)
    {
        await _paiementService.RembourserAsync(paiementId);
        return NoContent();
    }
}