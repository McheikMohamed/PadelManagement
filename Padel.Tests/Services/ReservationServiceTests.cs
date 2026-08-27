using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Moq;
using Padel.Application.Dtos;
using Padel.Application.Exceptions;
using Padel.Application.Interfaces;
using Padel.Application.Services;
using Padel.Domain.Entities;
using Padel.Domain.Enums;
using Xunit;
using MatchEntity = Padel.Domain.Entities.Match;

namespace Padel.Tests.Services;

public class ReservationServiceTests
{
    private readonly Mock<IMatchRepository> _matchRepositoryMock;
    private readonly Mock<IMembreRepository> _membreRepositoryMock;
    private readonly Mock<ITerrainRepository> _terrainRepositoryMock;
    private readonly ReservationService _reservationService;

    public ReservationServiceTests()
    {
        _matchRepositoryMock = new Mock<IMatchRepository>();
        _membreRepositoryMock = new Mock<IMembreRepository>();
        _terrainRepositoryMock = new Mock<ITerrainRepository>();

        _reservationService = new ReservationService(
            _matchRepositoryMock.Object,
            _membreRepositoryMock.Object,
            _terrainRepositoryMock.Object);
    }

    // ---------- CreerReservationAsync ----------

    [Fact]
    public async Task CreerReservationAsync_MembreInconnu_LanceRegleMetierException()
    {
        // Arrange
        _membreRepositoryMock
            .Setup(r => r.ObtenirParMatriculeAsync("X00000"))
            .ReturnsAsync((Membre?)null);

        var dto = new CreerReservationDto { TerrainId = 1, DateHeureDebut = DateTime.Now.AddDays(1) };

        // Act
        var ex = await Assert.ThrowsAsync<RegleMetierException>(
            () => _reservationService.CreerReservationAsync(dto, "X00000"));

        // Assert
        Assert.Equal("MEMBRE_INCONNU", ex.Code);
    }

    [Fact]
    public async Task CreerReservationAsync_SoldeDu_LanceRegleMetierException()
    {
        // Arrange
        var membre = new Membre { Matricule = "G00001", Type = TypeMembre.Global, SoldeDu = 15m };
        _membreRepositoryMock.Setup(r => r.ObtenirParMatriculeAsync("G00001")).ReturnsAsync(membre);

        var dto = new CreerReservationDto { TerrainId = 1, DateHeureDebut = DateTime.Now.AddDays(1) };

        // Act
        var ex = await Assert.ThrowsAsync<RegleMetierException>(
            () => _reservationService.CreerReservationAsync(dto, "G00001"));

        // Assert
        Assert.Equal("SOLDE_DU", ex.Code);
    }

    [Fact]
    public async Task CreerReservationAsync_PenaliteActive_LanceRegleMetierException()
    {
        // Arrange
        var membre = new Membre
        {
            Matricule = "G00001",
            Type = TypeMembre.Global,
            SoldeDu = 0,
            DateProchaineReservationAutorisee = DateOnly.FromDateTime(DateTime.Now.AddDays(3))
        };
        _membreRepositoryMock.Setup(r => r.ObtenirParMatriculeAsync("G00001")).ReturnsAsync(membre);

        var dto = new CreerReservationDto { TerrainId = 1, DateHeureDebut = DateTime.Now.AddDays(1) };

        // Act
        var ex = await Assert.ThrowsAsync<RegleMetierException>(
            () => _reservationService.CreerReservationAsync(dto, "G00001"));

        // Assert
        Assert.Equal("PENALITE_ACTIVE", ex.Code);
    }

    [Theory]
    [InlineData(TypeMembre.Global, 22)]  // délai autorisé = 21 jours
    [InlineData(TypeMembre.Site, 15)]    // délai autorisé = 14 jours
    [InlineData(TypeMembre.Libre, 6)]    // délai autorisé = 5 jours
    public async Task CreerReservationAsync_DelaiDepasseSelonType_LanceRegleMetierException(
        TypeMembre type, int joursDansLeFutur)
    {
        // Arrange
        var membre = new Membre { Matricule = "M00001", Type = type, SoldeDu = 0, SiteId = type == TypeMembre.Site ? 1 : null };
        _membreRepositoryMock.Setup(r => r.ObtenirParMatriculeAsync("M00001")).ReturnsAsync(membre);

        var dto = new CreerReservationDto
        {
            TerrainId = 1,
            DateHeureDebut = DateTime.Now.AddDays(joursDansLeFutur)
        };

        // Act
        var ex = await Assert.ThrowsAsync<RegleMetierException>(
            () => _reservationService.CreerReservationAsync(dto, "M00001"));

        // Assert
        Assert.Equal("DELAI_DEPASSE", ex.Code);
    }

    [Theory]
    [InlineData(TypeMembre.Global, 20)]
    [InlineData(TypeMembre.Site, 13)]
    [InlineData(TypeMembre.Libre, 4)]
    public async Task CreerReservationAsync_DansLeDelaiAutorise_NeLancePasException(
        TypeMembre type, int joursDansLeFutur)
    {
        // Arrange
        var siteId = type == TypeMembre.Site ? 1 : (int?)null;
        var membre = new Membre { Matricule = "M00001", Type = type, SoldeDu = 0, SiteId = siteId };
        _membreRepositoryMock.Setup(r => r.ObtenirParMatriculeAsync("M00001")).ReturnsAsync(membre);

        var terrain = new Terrain { TerrainId = 1, SiteId = siteId ?? 1, Numero = 1 };
        _terrainRepositoryMock.Setup(r => r.ObtenirParIdAsync(1)).ReturnsAsync(terrain);

        _matchRepositoryMock
            .Setup(r => r.CreerReservationAsync(1, It.IsAny<DateTime>(), "M00001", true))
            .ReturnsAsync(99);

        var dto = new CreerReservationDto
        {
            TerrainId = 1,
            DateHeureDebut = DateTime.Now.AddDays(joursDansLeFutur),
            EstPrive = true
        };

        // Act
        var resultat = await _reservationService.CreerReservationAsync(dto, "M00001");

        // Assert
        Assert.Equal(99, resultat.MatchId);
    }

    [Fact]
    public async Task CreerReservationAsync_TerrainInconnu_LanceRegleMetierException()
    {
        // Arrange
        var membre = new Membre { Matricule = "G00001", Type = TypeMembre.Global, SoldeDu = 0 };
        _membreRepositoryMock.Setup(r => r.ObtenirParMatriculeAsync("G00001")).ReturnsAsync(membre);
        _terrainRepositoryMock.Setup(r => r.ObtenirParIdAsync(999)).ReturnsAsync((Terrain?)null);

        var dto = new CreerReservationDto { TerrainId = 999, DateHeureDebut = DateTime.Now.AddDays(1) };

        // Act
        var ex = await Assert.ThrowsAsync<RegleMetierException>(
            () => _reservationService.CreerReservationAsync(dto, "G00001"));

        // Assert
        Assert.Equal("TERRAIN_INCONNU", ex.Code);
    }

    [Fact]
    public async Task CreerReservationAsync_MembreSiteHorsPerimetre_LanceRegleMetierException()
    {
        // Arrange : membre rattaché au site 1, terrain sur le site 2
        var membre = new Membre { Matricule = "S00001", Type = TypeMembre.Site, SoldeDu = 0, SiteId = 1 };
        _membreRepositoryMock.Setup(r => r.ObtenirParMatriculeAsync("S00001")).ReturnsAsync(membre);

        var terrain = new Terrain { TerrainId = 5, SiteId = 2, Numero = 1 };
        _terrainRepositoryMock.Setup(r => r.ObtenirParIdAsync(5)).ReturnsAsync(terrain);

        var dto = new CreerReservationDto { TerrainId = 5, DateHeureDebut = DateTime.Now.AddDays(1) };

        // Act
        var ex = await Assert.ThrowsAsync<RegleMetierException>(
            () => _reservationService.CreerReservationAsync(dto, "S00001"));

        // Assert
        Assert.Equal("PERIMETRE_SITE", ex.Code);
    }

    [Fact]
    public async Task CreerReservationAsync_MembreSiteDansSonPerimetre_NeLancePasException()
    {
        // Arrange : membre et terrain sur le même site
        var membre = new Membre { Matricule = "S00001", Type = TypeMembre.Site, SoldeDu = 0, SiteId = 1 };
        _membreRepositoryMock.Setup(r => r.ObtenirParMatriculeAsync("S00001")).ReturnsAsync(membre);

        var terrain = new Terrain { TerrainId = 5, SiteId = 1, Numero = 1 };
        _terrainRepositoryMock.Setup(r => r.ObtenirParIdAsync(5)).ReturnsAsync(terrain);

        _matchRepositoryMock
            .Setup(r => r.CreerReservationAsync(5, It.IsAny<DateTime>(), "S00001", true))
            .ReturnsAsync(10);

        var dto = new CreerReservationDto { TerrainId = 5, DateHeureDebut = DateTime.Now.AddDays(1), EstPrive = true };

        // Act
        var resultat = await _reservationService.CreerReservationAsync(dto, "S00001");

        // Assert
        Assert.Equal(10, resultat.MatchId);
        Assert.Equal("Prive", resultat.Statut);
    }

    [Fact]
    public async Task CreerReservationAsync_CasValidePublic_RetourneMatchDtoAvecStatutPublic()
    {
        // Arrange
        var membre = new Membre { Matricule = "L00001", Type = TypeMembre.Libre, SoldeDu = 0 };
        _membreRepositoryMock.Setup(r => r.ObtenirParMatriculeAsync("L00001")).ReturnsAsync(membre);

        var terrain = new Terrain { TerrainId = 2, SiteId = 1, Numero = 2 };
        _terrainRepositoryMock.Setup(r => r.ObtenirParIdAsync(2)).ReturnsAsync(terrain);

        _matchRepositoryMock
            .Setup(r => r.CreerReservationAsync(2, It.IsAny<DateTime>(), "L00001", false))
            .ReturnsAsync(7);

        var debut = DateTime.Now.AddDays(2);
        var dto = new CreerReservationDto { TerrainId = 2, DateHeureDebut = debut, EstPrive = false };

        // Act
        var resultat = await _reservationService.CreerReservationAsync(dto, "L00001");

        // Assert
        Assert.Equal(7, resultat.MatchId);
        Assert.Equal("Public", resultat.Statut);
        Assert.Equal(debut.AddMinutes(105), resultat.DateHeureFin);
        Assert.Equal(60m, resultat.Prix);
    }

    // ---------- InscrireJoueurAsync ----------

    [Fact]
    public async Task InscrireJoueurAsync_MatchInconnu_LanceRegleMetierException()
    {
        // Arrange
        _matchRepositoryMock.Setup(r => r.ObtenirParIdAsync(999)).ReturnsAsync((MatchEntity?)null);
        var dto = new InscrireJoueurDto { MatchId = 999, MembreMatricule = "G00001" };

        // Act
        var ex = await Assert.ThrowsAsync<RegleMetierException>(
            () => _reservationService.InscrireJoueurAsync(dto, "G00001"));

        // Assert
        Assert.Equal("MATCH_INCONNU", ex.Code);
    }

    [Fact]
    public async Task InscrireJoueurAsync_MatchPublicAppelantDifferentDuMembre_LanceRegleMetierException()
    {
        // Arrange
        var match = new MatchEntity
        {
            MatchId = 1,
            TerrainId = 1,
            OrganisateurMatricule = "G00001",
            DateHeureDebut = DateTime.Now.AddDays(1),
            DateHeureFin = DateTime.Now.AddDays(1).AddMinutes(105),
            Statut = StatutMatch.Public
        };
        _matchRepositoryMock.Setup(r => r.ObtenirParIdAsync(1)).ReturnsAsync(match);

        var dto = new InscrireJoueurDto { MatchId = 1, MembreMatricule = "L00001" };

        // Act
        var ex = await Assert.ThrowsAsync<RegleMetierException>(
            () => _reservationService.InscrireJoueurAsync(dto, "G00001"));

        // Assert
        Assert.Equal("INSCRIPTION_NON_AUTORISEE", ex.Code);
    }

    [Fact]
    public async Task InscrireJoueurAsync_MatchPublicAppelantEstLeMembre_NeLancePasException()
    {
        // Arrange
        var match = new MatchEntity
        {
            MatchId = 1,
            TerrainId = 1,
            OrganisateurMatricule = "G00001",
            DateHeureDebut = DateTime.Now.AddDays(1),
            DateHeureFin = DateTime.Now.AddDays(1).AddMinutes(105),
            Statut = StatutMatch.Public
        };
        _matchRepositoryMock.Setup(r => r.ObtenirParIdAsync(1)).ReturnsAsync(match);
        _matchRepositoryMock
            .Setup(r => r.InscrireJoueurAsync(1, "L00001", "L00001"))
            .ReturnsAsync(50);

        var dto = new InscrireJoueurDto { MatchId = 1, MembreMatricule = "L00001" };

        // Act
        var resultat = await _reservationService.InscrireJoueurAsync(dto, "L00001");

        // Assert
        Assert.Equal(50, resultat.InscriptionId);
    }

    [Fact]
    public async Task InscrireJoueurAsync_MatchPriveOrganisateurInscritUnAutreJoueur_NeLancePasException()
    {
        // Arrange
        var match = new MatchEntity
        {
            MatchId = 1,
            TerrainId = 1,
            OrganisateurMatricule = "G00001",
            DateHeureDebut = DateTime.Now.AddDays(1),
            DateHeureFin = DateTime.Now.AddDays(1).AddMinutes(105),
            Statut = StatutMatch.Prive
        };
        _matchRepositoryMock.Setup(r => r.ObtenirParIdAsync(1)).ReturnsAsync(match);
        _matchRepositoryMock
            .Setup(r => r.InscrireJoueurAsync(1, "S00001", "G00001"))
            .ReturnsAsync(51);

        var dto = new InscrireJoueurDto { MatchId = 1, MembreMatricule = "S00001" };

        // Act
        var resultat = await _reservationService.InscrireJoueurAsync(dto, "G00001");

        // Assert
        Assert.Equal(51, resultat.InscriptionId);
        Assert.Equal("S00001", resultat.MembreMatricule);
    }
    [Fact]
    public async Task AnnulerReservationAsync_AppelantEstOrganisateur_AppelleRepository()
    {
        var match = new MatchEntity
        {
            MatchId = 1,
            TerrainId = 1,
            OrganisateurMatricule = "G00001",
            DateHeureDebut = DateTime.Now.AddDays(1),
            DateHeureFin = DateTime.Now.AddDays(1).AddMinutes(105),
            Statut = StatutMatch.Prive
        };
        _matchRepositoryMock.Setup(r => r.ObtenirParIdAsync(1)).ReturnsAsync(match);

        await _reservationService.AnnulerReservationAsync(1, "G00001");

        _matchRepositoryMock.Verify(r => r.AnnulerMatchAsync(1, "G00001"), Times.Once);
    }

    [Fact]
    public async Task AnnulerReservationAsync_AppelantNestPasOrganisateur_LanceRegleMetierException()
    {
        var match = new MatchEntity
        {
            MatchId = 1,
            TerrainId = 1,
            OrganisateurMatricule = "G00001",
            DateHeureDebut = DateTime.Now.AddDays(1),
            DateHeureFin = DateTime.Now.AddDays(1).AddMinutes(105),
            Statut = StatutMatch.Prive
        };
        _matchRepositoryMock.Setup(r => r.ObtenirParIdAsync(1)).ReturnsAsync(match);

        var ex = await Assert.ThrowsAsync<RegleMetierException>(
            () => _reservationService.AnnulerReservationAsync(1, "L00002"));

        Assert.Equal("ACTION_RESERVEE_ORGANISATEUR", ex.Code);
    }

    [Fact]
    public async Task DesinscrireJoueurAsync_OrganisateurEssaieDeSeDesinscrire_LanceRegleMetierException()
    {
        var match = new MatchEntity
        {
            MatchId = 1,
            TerrainId = 1,
            OrganisateurMatricule = "G00001",
            DateHeureDebut = DateTime.Now.AddDays(1),
            DateHeureFin = DateTime.Now.AddDays(1).AddMinutes(105),
            Statut = StatutMatch.Public
        };
        _matchRepositoryMock.Setup(r => r.ObtenirParIdAsync(1)).ReturnsAsync(match);

        var ex = await Assert.ThrowsAsync<RegleMetierException>(
            () => _reservationService.DesinscrireJoueurAsync(1, "G00001", "G00001"));

        Assert.Equal("ORGANISATEUR_DOIT_ANNULER", ex.Code);
    }

    [Fact]
    public async Task DesinscrireJoueurAsync_CasValide_AppelleRepository()
    {
        var match = new MatchEntity
        {
            MatchId = 1,
            TerrainId = 1,
            OrganisateurMatricule = "G00001",
            DateHeureDebut = DateTime.Now.AddDays(1),
            DateHeureFin = DateTime.Now.AddDays(1).AddMinutes(105),
            Statut = StatutMatch.Public
        };
        _matchRepositoryMock.Setup(r => r.ObtenirParIdAsync(1)).ReturnsAsync(match);

        await _reservationService.DesinscrireJoueurAsync(1, "L00002", "L00002");

        _matchRepositoryMock.Verify(r => r.DesinscrireJoueurAsync(1, "L00002", "L00002"), Times.Once);
    }
}