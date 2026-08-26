using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Padel.Application.Interfaces;
using Padel.Domain.Entities;

namespace Padel.Infrastructure.Repositories;

public class SiteRepository : ISiteRepository
{
    private readonly PadelDbContext _context;

    public SiteRepository(PadelDbContext context)
    {
        _context = context;
    }

    public async Task<int> CreerAsync(string nom)
    {
        var resultat = await _context.Database
            .SqlQuery<int>($"EXEC sch_Padel.SP_CreerSite @Nom = {nom}")
            .ToListAsync();

        return resultat.Single();
    }

    public async Task<List<Site>> ListerAsync()
    {
        return await _context.Sites
            .FromSqlInterpolated($"EXEC sch_Padel.SP_ListerSites")
            .ToListAsync();
    }

    public async Task<Site?> ObtenirParIdAsync(int siteId)
    {
        var resultats = await _context.Sites
            .FromSqlInterpolated($"EXEC sch_Padel.SP_SelectSiteParId @SiteId = {siteId}")
            .ToListAsync();

        return resultats.SingleOrDefault();
    }
}