using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Padel.Application.Dtos;

namespace Padel.Application.Interfaces;

public interface IStatistiqueService
{
    Task<List<ChiffreAffairesDto>> ObtenirChiffreAffairesAsync(int? siteId, DateOnly dateDebut, DateOnly dateFin);
    Task<StatistiquesMatchesDto> ObtenirStatistiquesMatchesAsync(int? siteId, DateOnly dateDebut, DateOnly dateFin);
    Task<(List<ImpayeDto> Impayes, decimal Total)> ObtenirImpayesAsync(int? siteId);
    Task<List<PenaliteActiveDto>> ObtenirPenalitesActivesAsync(int? siteId);
}