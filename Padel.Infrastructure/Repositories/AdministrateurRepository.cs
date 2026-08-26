using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Padel.Application.Interfaces;
using Padel.Domain.Entities;

namespace Padel.Infrastructure.Repositories;

public class AdministrateurRepository : IAdministrateurRepository
{
    private readonly PadelDbContext _context;

    public AdministrateurRepository(PadelDbContext context)
    {
        _context = context;
    }

    public async Task CreerAsync(string matricule, string type, int? siteId)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sch_Padel.SP_CreerAdministrateur @Matricule = {matricule}, @Type = {type}, @SiteId = {siteId}");
    }

    public async Task<Administrateur?> ObtenirParMatriculeAsync(string matricule)
    {
        var resultats = await _context.Administrateurs
            .FromSqlInterpolated($"EXEC sch_Padel.SP_SelectAdministrateurParMatricule @Matricule = {matricule}")
            .ToListAsync();

        return resultats.SingleOrDefault();
    }
}