using Microsoft.AspNetCore.Mvc;
using Padel.Api.Middleware;
using Padel.Application.Dtos;
using Padel.Application.Interfaces;

namespace Padel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SitesController : ControllerBase
{
    private readonly ISiteService _siteService;

    public SitesController(ISiteService siteService)
    {
        _siteService = siteService;
    }

    [HttpGet]
    public async Task<ActionResult<List<SiteDto>>> ListerSites()
    {
        var sites = await _siteService.ListerSitesAsync();
        return Ok(sites);
    }

    [HttpGet("{siteId}")]
    public async Task<ActionResult<SiteDto>> ObtenirSite(int siteId)
    {
        var site = await _siteService.ObtenirSiteAsync(siteId);

        if (site is null)
        {
            return NotFound();
        }

        return Ok(site);
    }

    [HttpPost]
    public async Task<ActionResult<SiteDto>> CreerSite([FromBody] CreerSiteDto dto)
    {
        var identite = (IdentiteAppelant)HttpContext.Items["Identite"]!;

        var site = await _siteService.CreerSiteAsync(dto, identite.Matricule);

        return CreatedAtAction(nameof(ObtenirSite), new { siteId = site.SiteId }, site);
    }
}