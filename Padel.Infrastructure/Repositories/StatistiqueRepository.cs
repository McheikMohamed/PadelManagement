using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Padel.Application.Dtos;
using Padel.Application.Interfaces;

namespace Padel.Infrastructure.Repositories;

public class StatistiqueRepository : IStatistiqueRepository
{
    private readonly PadelDbContext _context;

    public StatistiqueRepository(PadelDbContext context)
    {
        _context = context;
    }

    public async Task<List<ChiffreAffairesDto>> SelectChiffreAffairesAsync(
        int? siteId, DateOnly dateDebut, DateOnly dateFin)
    {
        return await _context.Database
            .SqlQuery<ChiffreAffairesDto>(
                $"EXEC sch_Padel.SP_SelectChiffreAffaires @SiteId = {siteId}, @DateDebut = {dateDebut}, @DateFin = {dateFin}")
            .ToListAsync();
    }

    public async Task<List<TauxOccupationDto>> SelectTauxOccupationAsync(
        int? siteId, DateOnly dateDebut, DateOnly dateFin)
    {
        return await _context.Database
            .SqlQuery<TauxOccupationDto>(
                $"EXEC sch_Padel.SP_SelectTauxOccupation @SiteId = {siteId}, @DateDebut = {dateDebut}, @DateFin = {dateFin}")
            .ToListAsync();
    }

    public async Task<StatistiquesMatchesDto> SelectStatistiquesMatchesAsync(
        int? siteId, DateOnly dateDebut, DateOnly dateFin)
    {
        var resultat = await _context.Database
            .SqlQuery<StatistiquesMatchesDto>(
                $"EXEC sch_Padel.SP_SelectStatistiquesMatches @SiteId = {siteId}, @DateDebut = {dateDebut}, @DateFin = {dateFin}")
            .ToListAsync();

        return resultat.Single();
    }

    public async Task<(List<ImpayeDto> Impayes, decimal Total)> SelectImpayesAsync(int? siteId)
    {
        // SP_SelectImpayes retourne 2 jeux de résultats (liste détaillée + total).
        // SqlQuery/FromSqlInterpolated ne lisent que le PREMIER jeu de résultats d'une procédure :
        // on doit donc descendre au niveau ADO.NET brut pour lire le second (via NextResultAsync).
        var impayes = new List<ImpayeDto>();
        decimal total = 0;

        var connection = _context.Database.GetDbConnection();
        var ouvrirConnexionIci = connection.State != System.Data.ConnectionState.Open;

        if (ouvrirConnexionIci)
        {
            await connection.OpenAsync();
        }

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "EXEC sch_Padel.SP_SelectImpayes @SiteId";
            command.CommandType = System.Data.CommandType.Text;

            var parametre = command.CreateParameter();
            parametre.ParameterName = "@SiteId";
            parametre.Value = (object?)siteId ?? DBNull.Value;
            command.Parameters.Add(parametre);

            using var reader = await command.ExecuteReaderAsync();

            // Premier jeu de résultats : la liste détaillée des impayés
            while (await reader.ReadAsync())
            {
                impayes.Add(new ImpayeDto
                {
                    Matricule = reader.GetString(reader.GetOrdinal("Matricule")),
                    Type = reader.GetString(reader.GetOrdinal("Type")),
                    SoldeDu = reader.GetDecimal(reader.GetOrdinal("SoldeDu"))
                });
            }

            // Second jeu de résultats : le total agrégé
            if (await reader.NextResultAsync() && await reader.ReadAsync())
            {
                total = reader.GetDecimal(reader.GetOrdinal("TotalImpayes"));
            }
        }
        finally
        {
            if (ouvrirConnexionIci)
            {
                await connection.CloseAsync();
            }
        }

        return (impayes, total);
    }

    public async Task<List<PenaliteActiveDto>> SelectPenalitesActivesAsync(int? siteId)
    {
        return await _context.Database
            .SqlQuery<PenaliteActiveDto>(
                $"EXEC sch_Padel.SP_SelectPenalitesActives @SiteId = {siteId}")
            .ToListAsync();
    }
}