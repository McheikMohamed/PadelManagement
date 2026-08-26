using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Padel.Application.Dtos;
using Padel.Application.Interfaces;

namespace Padel.Application.Services;

public class StatistiqueService : IStatistiqueService
{
    private readonly IStatistiqueRepository _statistiqueRepository;

    public StatistiqueService(IStatistiqueRepository statistiqueRepository)
    {
        _statistiqueRepository = statistiqueRepository;
    }

    public async Task<List<ChiffreAffairesDto>> ObtenirChiffreAffairesAsync(
        int? siteId, DateOnly dateDebut, DateOnly dateFin)
    {
        return await _statistiqueRepository.SelectChiffreAffairesAsync(siteId, dateDebut, dateFin);
    }

    public async Task<StatistiquesMatchesDto> ObtenirStatistiquesMatchesAsync(
        int? siteId, DateOnly dateDebut, DateOnly dateFin)
    {
        return await _statistiqueRepository.SelectStatistiquesMatchesAsync(siteId, dateDebut, dateFin);
    }

    public async Task<(List<ImpayeDto> Impayes, decimal Total)> ObtenirImpayesAsync(int? siteId)
    {
        return await _statistiqueRepository.SelectImpayesAsync(siteId);
    }

    public async Task<List<PenaliteActiveDto>> ObtenirPenalitesActivesAsync(int? siteId)
    {
        return await _statistiqueRepository.SelectPenalitesActivesAsync(siteId);
    }
}