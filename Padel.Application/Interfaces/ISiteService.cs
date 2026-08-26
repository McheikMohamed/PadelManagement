using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Padel.Application.Dtos;

namespace Padel.Application.Interfaces;

public interface ISiteService
{
    Task<SiteDto> CreerSiteAsync(CreerSiteDto dto);
    Task<List<SiteDto>> ListerSitesAsync();
    Task<SiteDto?> ObtenirSiteAsync(int siteId);
}