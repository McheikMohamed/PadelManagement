using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Padel.Domain.Entities;

namespace Padel.Application.Interfaces;

public interface ITerrainRepository
{
    Task<int> CreerAsync(int siteId, int numero);
    Task<List<Terrain>> ListerParSiteAsync(int siteId);
    Task<Terrain?> ObtenirParIdAsync(int terrainId);
}