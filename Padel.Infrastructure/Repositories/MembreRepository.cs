using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Padel.Application.Interfaces;
using Padel.Domain.Entities;

namespace Padel.Infrastructure.Repositories;

public class MembreRepository : IMembreRepository
{
    private readonly PadelDbContext _context;

    public MembreRepository(PadelDbContext context)
    {
        _context = context;
    }

    public async Task CreerAsync(string matricule, string type, int? siteId)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sch_Padel.SP_CreerMembre @Matricule = {matricule}, @Type = {type}, @SiteId = {siteId}");
    }

    public async Task<Membre?> ObtenirParMatriculeAsync(string matricule)
    {
        var resultats = await _context.Membres
            .FromSqlInterpolated($"EXEC sch_Padel.SP_SelectMembreParMatricule @Matricule = {matricule}")
            .ToListAsync();

        return resultats.SingleOrDefault();
    }

    public async Task<List<Membre>> ListerAsync(int? siteId)
    {
        return await _context.Membres
            .FromSqlInterpolated($"EXEC sch_Padel.SP_ListerMembres @SiteId = {siteId}")
            .ToListAsync();
    }
    public async Task<string?> ObtenirMembreParInscriptionAsync(int inscriptionId)
    {
        var resultat = await _context.Database
            .SqlQuery<string>(
                $"EXEC sch_Padel.SP_SelectMembreParInscription @InscriptionId = {inscriptionId}")
            .ToListAsync();

        return resultat.SingleOrDefault();
    }
}