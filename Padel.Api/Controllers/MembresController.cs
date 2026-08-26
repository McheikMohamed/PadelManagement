using Microsoft.AspNetCore.Mvc;
using Padel.Api.Middleware;
using Padel.Application.Dtos;
using Padel.Application.Interfaces;

namespace Padel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembresController : ControllerBase
{
    private readonly IMembreService _membreService;

    public MembresController(IMembreService membreService)
    {
        _membreService = membreService;
    }

    [HttpPost]
    public async Task<IActionResult> CreerMembre([FromBody] CreerMembreDto dto)
    {
        await _membreService.CreerMembreAsync(dto);
        return CreatedAtAction(nameof(ObtenirMembre), new { matricule = dto.Matricule }, dto);
    }

    [HttpGet("{matricule}")]
    public async Task<ActionResult<MembreDto>> ObtenirMembre(string matricule)
    {
        var membre = await _membreService.ObtenirMembreAsync(matricule);

        if (membre is null)
        {
            return NotFound();
        }

        return Ok(membre);
    }

    [HttpGet]
    public async Task<ActionResult<List<MembreDto>>> ListerMembres()
    {
        var identite = (IdentiteAppelant)HttpContext.Items["Identite"]!;

        // CF-AA-013 : le périmètre est déterminé ICI, à partir de l'identité de l'appelant,
        // jamais depuis une valeur que le client pourrait fournir. Un Admin Site ne voit
        // jamais que son propre site, quoi qu'il essaie de demander.
        int? siteIdEffectif = identite.TypeRole switch
        {
            "AdminGlobal" => null,                 // voit tous les membres
            "AdminSite" => identite.SiteId,         // restreint à son propre site, jamais autre chose
            _ => throw new Padel.Application.Exceptions.RegleMetierException(
                "ACTION_RESERVEE_ADMIN", "Seul un administrateur peut lister les membres.")
        };

        var membres = await _membreService.ListerMembresAsync(siteIdEffectif);
        return Ok(membres);
    }
}