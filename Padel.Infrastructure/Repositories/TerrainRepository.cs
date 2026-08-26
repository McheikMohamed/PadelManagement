using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Padel.Application.Interfaces;
using Padel.Domain.Entities;

namespace Padel.Infrastructure.Repositories;

public class TerrainRepository : ITerrainRepository
{
    private readonly PadelDbContext _context;

    public TerrainRepository(PadelDbContext context)
    {
        _context = context;
    }

    public async Task<int> CreerAsync(int siteId, int numero)
    {
        var resultat = await _context.Database
            .SqlQuery<int>($"EXEC sch_Padel.SP_CreerTerrain @SiteId = {siteId}, @Numero = {numero}")
            .ToListAsync();

        return resultat.Single();
    }

    public async Task<List<Terrain>> ListerParSiteAsync(int siteId)
    {
        return await _context.Terrains
            .FromSqlInterpolated($"EXEC sch_Padel.SP_ListerTerrainsParSite @SiteId = {siteId}")
            .ToListAsync();
    }

    public async Task<Terrain?> ObtenirParIdAsync(int terrainId)
    {
        var resultats = await _context.Terrains
            .FromSqlInterpolated($"EXEC sch_Padel.SP_SelectTerrainParId @TerrainId = {terrainId}")
            .ToListAsync();

        return resultats.SingleOrDefault();
    }
}