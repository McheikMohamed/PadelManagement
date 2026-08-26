using Padel.Application.Dtos;
using Padel.Application.Exceptions;
using Padel.Application.Interfaces;
using Padel.Domain.Enums;

namespace Padel.Application.Services;

public class SiteService : ISiteService
{
    private readonly ISiteRepository _siteRepository;
    private readonly IAdministrateurRepository _administrateurRepository;

    public SiteService(ISiteRepository siteRepository, IAdministrateurRepository administrateurRepository)
    {
        _siteRepository = siteRepository;
        _administrateurRepository = administrateurRepository;
    }

    public async Task<SiteDto> CreerSiteAsync(CreerSiteDto dto, string appelantMatricule)
    {
        // CF-AA-015 : la création d'un site est réservée à l'administrateur global.
        var appelant = await _administrateurRepository.ObtenirParMatriculeAsync(appelantMatricule);

        if (appelant is null)
        {
            throw new RegleMetierException("APPELANT_INCONNU", "Administrateur appelant inconnu.");
        }

        if (appelant.Type != TypeAdmin.Global)
        {
            throw new RegleMetierException(
                "ACTION_RESERVEE_GLOBAL",
                "Seul un administrateur global peut créer un site.");
        }

        if (string.IsNullOrWhiteSpace(dto.Nom))
        {
            throw new RegleMetierException("NOM_OBLIGATOIRE", "Le nom du site est obligatoire.");
        }

        var siteId = await _siteRepository.CreerAsync(dto.Nom);

        return new SiteDto
        {
            SiteId = siteId,
            Nom = dto.Nom
        };
    }

    public async Task<List<SiteDto>> ListerSitesAsync()
    {
        var sites = await _siteRepository.ListerAsync();

        return sites.Select(s => new SiteDto
        {
            SiteId = s.SiteId,
            Nom = s.Nom
        }).ToList();
    }

    public async Task<SiteDto?> ObtenirSiteAsync(int siteId)
    {
        var site = await _siteRepository.ObtenirParIdAsync(siteId);

        if (site is null)
        {
            return null;
        }

        return new SiteDto
        {
            SiteId = site.SiteId,
            Nom = site.Nom
        };
    }
}