using Microsoft.EntityFrameworkCore;
using Padel.Application.Dtos;
using Padel.Application.Interfaces;
using Padel.Domain.Entities;

namespace Padel.Infrastructure.Repositories;

public class MatchRepository : IMatchRepository
{
    private readonly PadelDbContext _context;

    public MatchRepository(PadelDbContext context)
    {
        _context = context;
    }

    public async Task<List<CreneauDisponibleDto>> ListerCreneauxDisponiblesAsync(
        int siteId, DateOnly date, int? terrainId)
    {
        return await _context.Database
            .SqlQuery<CreneauDisponibleDto>(
                $"EXEC sch_Padel.SP_ListerCreneauxDisponibles @SiteId = {siteId}, @Date = {date}, @TerrainId = {terrainId}")
            .ToListAsync();
    }

    public async Task<int> CreerReservationAsync(
        int terrainId, DateTime dateHeureDebut, string organisateurMatricule, bool estPrive)
    {
        var resultat = await _context.Database
            .SqlQuery<int>(
                $"EXEC sch_Padel.SP_CreerReservation @TerrainId = {terrainId}, @DateHeureDebut = {dateHeureDebut}, @OrganisateurMatricule = {organisateurMatricule}, @EstPrive = {estPrive}")
            .ToListAsync();

        return resultat.Single();
    }

    public async Task<int> InscrireJoueurAsync(int matchId, string membreMatricule, string appelantMatricule)
    {
        var resultat = await _context.Database
            .SqlQuery<int>(
                $"EXEC sch_Padel.SP_InscrireJoueurMatch @MatchId = {matchId}, @MembreMatricule = {membreMatricule}, @AppelantMatricule = {appelantMatricule}")
            .ToListAsync();

        return resultat.Single();
    }

    public async Task<Match?> ObtenirParIdAsync(int matchId)
    {
        var resultats = await _context.Matches
            .FromSqlInterpolated($"EXEC sch_Padel.SP_SelectMatchParId @MatchId = {matchId}")
            .ToListAsync();

        return resultats.SingleOrDefault();
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