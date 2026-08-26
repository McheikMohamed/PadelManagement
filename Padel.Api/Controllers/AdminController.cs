using Microsoft.AspNetCore.Mvc;
using Padel.Api.Middleware;
using Padel.Application.Dtos;
using Padel.Application.Exceptions;
using Padel.Application.Interfaces;

namespace Padel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IStatistiqueService _statistiqueService;

    public AdminController(IAdminService adminService, IStatistiqueService statistiqueService)
    {
        _adminService = adminService;
        _statistiqueService = statistiqueService;
    }

    /// <summary>
    /// Détermine le périmètre effectif (SiteId) selon le rôle de l'appelant, à partir
    /// de son identité résolue par le middleware — jamais depuis une valeur fournie
    /// par le client (cf. CF-AA-013, même principe que MembresController).
    /// </summary>
    private static int? DeterminerSiteIdEffectif(IdentiteAppelant identite)
    {
        return identite.TypeRole switch
        {
            "AdminGlobal" => null,
            "AdminSite" => identite.SiteId,
            _ => throw new RegleMetierException(
                "ACTION_RESERVEE_ADMIN", "Seul un administrateur peut effectuer cette action.")
        };
    }

    [HttpPost("administrateurs")]
    public async Task<IActionResult> CreerAdministrateur([FromBody] CreerAdministrateurDto dto)
    {
        var identite = (IdentiteAppelant)HttpContext.Items["Identite"]!;

        await _adminService.CreerAdministrateurAsync(dto, identite.Matricule);

        return CreatedAtAction(nameof(ObtenirAdministrateur), new { matricule = dto.Matricule }, dto);
    }

    [HttpGet("administrateurs/{matricule}")]
    public async Task<ActionResult<AdministrateurDto>> ObtenirAdministrateur(string matricule)
    {
        var identite = (IdentiteAppelant)HttpContext.Items["Identite"]!;

        // Restreint aux administrateurs uniquement — un simple membre ne peut pas
        // consulter la fiche d'un administrateur. On ne restreint pas davantage par
        // périmètre de site ici : consulter un profil administrateur (pas une liste)
        // reste acceptable pour n'importe quel administrateur, Global ou Site.
        if (identite.TypeRole != "AdminGlobal" && identite.TypeRole != "AdminSite")
        {
            throw new RegleMetierException(
                "ACTION_RESERVEE_ADMIN", "Seul un administrateur peut consulter ce profil.");
        }

        var admin = await _adminService.ObtenirAdministrateurAsync(matricule);

        if (admin is null)
        {
            return NotFound();
        }

        return Ok(admin);
    }

    [HttpGet("chiffre-affaires")]
    public async Task<ActionResult<List<ChiffreAffairesDto>>> ObtenirChiffreAffaires(
        [FromQuery] DateOnly dateDebut,
        [FromQuery] DateOnly dateFin)
    {
        var identite = (IdentiteAppelant)HttpContext.Items["Identite"]!;
        var siteIdEffectif = DeterminerSiteIdEffectif(identite);

        var resultat = await _statistiqueService.ObtenirChiffreAffairesAsync(siteIdEffectif, dateDebut, dateFin);
        return Ok(resultat);
    }

    [HttpGet("statistiques-matches")]
    public async Task<ActionResult<StatistiquesMatchesDto>> ObtenirStatistiquesMatches(
        [FromQuery] DateOnly dateDebut,
        [FromQuery] DateOnly dateFin)
    {
        var identite = (IdentiteAppelant)HttpContext.Items["Identite"]!;
        var siteIdEffectif = DeterminerSiteIdEffectif(identite);

        var resultat = await _statistiqueService.ObtenirStatistiquesMatchesAsync(siteIdEffectif, dateDebut, dateFin);
        return Ok(resultat);
    }

    [HttpGet("impayes")]
    public async Task<IActionResult> ObtenirImpayes()
    {
        var identite = (IdentiteAppelant)HttpContext.Items["Identite"]!;
        var siteIdEffectif = DeterminerSiteIdEffectif(identite);

        var (impayes, total) = await _statistiqueService.ObtenirImpayesAsync(siteIdEffectif);
        return Ok(new { impayes, total });
    }

    [HttpGet("penalites-actives")]
    public async Task<ActionResult<List<PenaliteActiveDto>>> ObtenirPenalitesActives()
    {
        var identite = (IdentiteAppelant)HttpContext.Items["Identite"]!;
        var siteIdEffectif = DeterminerSiteIdEffectif(identite);

        var resultat = await _statistiqueService.ObtenirPenalitesActivesAsync(siteIdEffectif);
        return Ok(resultat);
    }
}