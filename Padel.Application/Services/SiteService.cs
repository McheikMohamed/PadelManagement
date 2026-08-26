using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Padel.Application.Dtos;
using Padel.Application.Interfaces;

namespace Padel.Application.Services;

public class SiteService : ISiteService
{
    private readonly ISiteRepository _siteRepository;

    public SiteService(ISiteRepository siteRepository)
    {
        _siteRepository = siteRepository;
    }

    public async Task<SiteDto> CreerSiteAsync(CreerSiteDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nom))
        {
            throw new ArgumentException("Le nom du site est obligatoire.");
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