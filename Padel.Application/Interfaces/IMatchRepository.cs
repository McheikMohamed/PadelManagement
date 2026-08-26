using Padel.Application.Dtos;
using Padel.Domain.Entities;

namespace Padel.Application.Interfaces;

public interface IMatchRepository
{
    Task<List<CreneauDisponibleDto>> ListerCreneauxDisponiblesAsync(
        int siteId, DateOnly date, int? terrainId);

    Task<int> CreerReservationAsync(
        int terrainId, DateTime dateHeureDebut, string organisateurMatricule, bool estPrive);

    Task<int> InscrireJoueurAsync(int matchId, string membreMatricule, string appelantMatricule);

    Task<Match?> ObtenirParIdAsync(int matchId);

    Task<string?> ObtenirMembreParInscriptionAsync(int inscriptionId);
}