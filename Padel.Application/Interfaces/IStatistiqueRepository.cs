using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Padel.Application.Dtos;

namespace Padel.Application.Interfaces;

public interface IStatistiqueRepository
{
    Task<List<ChiffreAffairesDto>> SelectChiffreAffairesAsync(int? siteId, DateOnly dateDebut, DateOnly dateFin);
    Task<List<TauxOccupationDto>> SelectTauxOccupationAsync(int? siteId, DateOnly dateDebut, DateOnly dateFin);
    Task<StatistiquesMatchesDto> SelectStatistiquesMatchesAsync(int? siteId, DateOnly dateDebut, DateOnly dateFin);
    Task<(List<ImpayeDto> Impayes, decimal Total)> SelectImpayesAsync(int? siteId);
    Task<List<PenaliteActiveDto>> SelectPenalitesActivesAsync(int? siteId);
}