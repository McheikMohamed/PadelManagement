using Microsoft.AspNetCore.Mvc;
using Padel.Api.Middleware;
using Padel.Application.Dtos;
using Padel.Application.Interfaces;

namespace Padel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpGet("creneaux-disponibles")]
    public async Task<ActionResult<List<CreneauDisponibleDto>>> ListerCreneauxDisponibles(
        [FromQuery] int siteId,
        [FromQuery] DateOnly date,
        [FromQuery] int? terrainId)
    {
        var creneaux = await _reservationService.ListerCreneauxDisponiblesAsync(siteId, date, terrainId);
        return Ok(creneaux);
    }

    [HttpPost]
    public async Task<ActionResult<MatchDto>> CreerReservation([FromBody] CreerReservationDto dto)
    {
        var identite = (IdentiteAppelant)HttpContext.Items["Identite"]!;

        var match = await _reservationService.CreerReservationAsync(dto, identite.Matricule);

        return CreatedAtAction(nameof(CreerReservation), new { matchId = match.MatchId }, match);
    }

    [HttpPost("{matchId}/inscriptions")]
    public async Task<ActionResult<InscriptionDto>> InscrireJoueur(
        int matchId,
        [FromBody] InscrireJoueurRequestDto requeteDto)
    {
        var identite = (IdentiteAppelant)HttpContext.Items["Identite"]!;

        var dto = new InscrireJoueurDto
        {
            MatchId = matchId,
            MembreMatricule = requeteDto.MembreMatricule
        };

        var inscription = await _reservationService.InscrireJoueurAsync(dto, identite.Matricule);

        return Ok(inscription);
    }
}