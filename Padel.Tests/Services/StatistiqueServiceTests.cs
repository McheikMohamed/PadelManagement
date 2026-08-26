using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Moq;
using Padel.Application.Dtos;
using Padel.Application.Interfaces;
using Padel.Application.Services;
using Xunit;

namespace Padel.Tests.Services;

public class StatistiqueServiceTests
{
    private readonly Mock<IStatistiqueRepository> _statistiqueRepositoryMock;
    private readonly StatistiqueService _statistiqueService;

    public StatistiqueServiceTests()
    {
        _statistiqueRepositoryMock = new Mock<IStatistiqueRepository>();
        _statistiqueService = new StatistiqueService(_statistiqueRepositoryMock.Object);
    }

    [Fact]
    public async Task ObtenirChiffreAffairesAsync_TransmetLesParametresTelsQuels()
    {
        // Arrange
        var dateDebut = new DateOnly(2026, 1, 1);
        var dateFin = new DateOnly(2026, 12, 31);
        var attendu = new List<ChiffreAffairesDto>
        {
            new() { SiteId = 1, NomSite = "Padel Club Bruxelles", Montant = 75m }
        };
        _statistiqueRepositoryMock
            .Setup(r => r.SelectChiffreAffairesAsync(1, dateDebut, dateFin))
            .ReturnsAsync(attendu);

        // Act
        var resultat = await _statistiqueService.ObtenirChiffreAffairesAsync(1, dateDebut, dateFin);

        // Assert
        Assert.Single(resultat);
        Assert.Equal(75m, resultat[0].Montant);
        _statistiqueRepositoryMock.Verify(
            r => r.SelectChiffreAffairesAsync(1, dateDebut, dateFin), Times.Once);
    }

    [Fact]
    public async Task ObtenirChiffreAffairesAsync_SiteIdNull_TransmetNullPourVueConsolidee()
    {
        // Arrange
        var dateDebut = new DateOnly(2026, 1, 1);
        var dateFin = new DateOnly(2026, 12, 31);
        _statistiqueRepositoryMock
            .Setup(r => r.SelectChiffreAffairesAsync(null, dateDebut, dateFin))
            .ReturnsAsync(new List<ChiffreAffairesDto>());

        // Act
        await _statistiqueService.ObtenirChiffreAffairesAsync(null, dateDebut, dateFin);

        // Assert
        _statistiqueRepositoryMock.Verify(
            r => r.SelectChiffreAffairesAsync(null, dateDebut, dateFin), Times.Once);
    }

    [Fact]
    public async Task ObtenirStatistiquesMatchesAsync_RetourneLeDtoDuRepository()
    {
        // Arrange
        var dateDebut = new DateOnly(2026, 1, 1);
        var dateFin = new DateOnly(2026, 12, 31);
        var attendu = new StatistiquesMatchesDto
        {
            TotalMatches = 6,
            NombrePublics = 3,
            NombrePrivesOuConfirmes = 3,
            NombreAnnules = 0
        };
        _statistiqueRepositoryMock
            .Setup(r => r.SelectStatistiquesMatchesAsync(1, dateDebut, dateFin))
            .ReturnsAsync(attendu);

        // Act
        var resultat = await _statistiqueService.ObtenirStatistiquesMatchesAsync(1, dateDebut, dateFin);

        // Assert
        Assert.Equal(6, resultat.TotalMatches);
        Assert.Equal(3, resultat.NombrePublics);
    }

    [Fact]
    public async Task ObtenirImpayesAsync_RetourneListeEtTotal()
    {
        // Arrange
        var impayesAttendus = new List<ImpayeDto>
        {
            new() { Matricule = "G00001", Type = "Global", SoldeDu = 30m }
        };
        _statistiqueRepositoryMock
            .Setup(r => r.SelectImpayesAsync(null))
            .ReturnsAsync((impayesAttendus, 30m));

        // Act
        var (impayes, total) = await _statistiqueService.ObtenirImpayesAsync(null);

        // Assert
        Assert.Single(impayes);
        Assert.Equal(30m, total);
    }

    [Fact]
    public async Task ObtenirImpayesAsync_AucunImpaye_RetourneListeVideEtTotalZero()
    {
        // Arrange
        _statistiqueRepositoryMock
            .Setup(r => r.SelectImpayesAsync(2))
            .ReturnsAsync((new List<ImpayeDto>(), 0m));

        // Act
        var (impayes, total) = await _statistiqueService.ObtenirImpayesAsync(2);

        // Assert
        Assert.Empty(impayes);
        Assert.Equal(0m, total);
    }

    [Fact]
    public async Task ObtenirPenalitesActivesAsync_TransmetSiteIdEtRetourneLaListe()
    {
        // Arrange
        var attendu = new List<PenaliteActiveDto>
        {
            new() { Matricule = "G00002", Type = "Global", DateProchaineReservationAutorisee = new DateOnly(2026, 9, 15) }
        };
        _statistiqueRepositoryMock
            .Setup(r => r.SelectPenalitesActivesAsync(3))
            .ReturnsAsync(attendu);

        // Act
        var resultat = await _statistiqueService.ObtenirPenalitesActivesAsync(3);

        // Assert
        Assert.Single(resultat);
        Assert.Equal("G00002", resultat[0].Matricule);
    }
}