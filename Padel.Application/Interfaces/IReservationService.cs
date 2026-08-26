using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Padel.Application.Dtos;

namespace Padel.Application.Interfaces;

public interface IReservationService
{
    Task<List<CreneauDisponibleDto>> ListerCreneauxDisponiblesAsync(int siteId, DateOnly date, int? terrainId);
    Task<MatchDto> CreerReservationAsync(CreerReservationDto dto, string organisateurMatricule);
    Task<InscriptionDto> InscrireJoueurAsync(InscrireJoueurDto dto, string appelantMatricule);
    Task<List<MatchPublicDto>> ListerMatchsPublicsAsync(int? siteId);
    Task AnnulerReservationAsync(int matchId, string appelantMatricule);
    Task DesinscrireJoueurAsync(int matchId, string membreMatricule, string appelantMatricule);
    Task<List<MaReservationDto>> ListerMesReservationsAsync(string matricule);
}