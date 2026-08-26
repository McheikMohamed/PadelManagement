using Moq;
using Padel.Application.Exceptions;
using Padel.Application.Interfaces;
using Padel.Application.Services;
using Padel.Domain.Entities;
using Padel.Domain.Enums;
using Xunit;

namespace Padel.Tests.Services;

public class PaiementServiceTests
{
    private readonly Mock<IPaiementRepository> _paiementRepositoryMock;
    private readonly Mock<IMatchRepository> _matchRepositoryMock;
    private readonly Mock<IAdministrateurRepository> _administrateurRepositoryMock;
    private readonly PaiementService _paiementService;

    public PaiementServiceTests()
    {
        _paiementRepositoryMock = new Mock<IPaiementRepository>();
        _matchRepositoryMock = new Mock<IMatchRepository>();
        _administrateurRepositoryMock = new Mock<IAdministrateurRepository>();
        _paiementService = new PaiementService(
            _paiementRepositoryMock.Object,
            _matchRepositoryMock.Object,
            _administrateurRepositoryMock.Object);
    }

    [Fact]
    public async Task TraiterPaiementAsync_InscriptionInconnue_LanceRegleMetierException()
    {
        _matchRepositoryMock.Setup(r => r.ObtenirMembreParInscriptionAsync(999)).ReturnsAsync((string?)null);

        var ex = await Assert.ThrowsAsync<RegleMetierException>(
            () => _paiementService.TraiterPaiementAsync(999, "G00001"));

        Assert.Equal("INSCRIPTION_INCONNUE", ex.Code);
    }

    [Fact]
    public async Task TraiterPaiementAsync_AppelantEstLeMembreConcerne_RetournePaiementDto()
    {
        _matchRepositoryMock.Setup(r => r.ObtenirMembreParInscriptionAsync(10)).ReturnsAsync("G00001");
        _paiementRepositoryMock.Setup(r => r.TraiterPaiementAsync(10, 15.00m)).ReturnsAsync(100);

        var resultat = await _paiementService.TraiterPaiementAsync(10, "G00001");

        Assert.Equal(100, resultat.PaiementId);
        Assert.Equal(15.00m, resultat.Montant);
    }

    [Fact]
    public async Task TraiterPaiementAsync_AppelantDifferentEtNonAdmin_LanceRegleMetierException()
    {
        _matchRepositoryMock.Setup(r => r.ObtenirMembreParInscriptionAsync(10)).ReturnsAsync("G00001");
        _administrateurRepositoryMock
            .Setup(r => r.ObtenirParMatriculeAsync("L00002"))
            .ReturnsAsync((Administrateur?)null);

        var ex = await Assert.ThrowsAsync<RegleMetierException>(
            () => _paiementService.TraiterPaiementAsync(10, "L00002"));

        Assert.Equal("PAIEMENT_NON_AUTORISE", ex.Code);
        _paiementRepositoryMock.Verify(r => r.TraiterPaiementAsync(It.IsAny<int>(), It.IsAny<decimal>()), Times.Never);
    }

    [Fact]
    public async Task TraiterPaiementAsync_AppelantEstAdministrateur_AutoriseLePaiement()
    {
        _matchRepositoryMock.Setup(r => r.ObtenirMembreParInscriptionAsync(10)).ReturnsAsync("G00001");
        _administrateurRepositoryMock
            .Setup(r => r.ObtenirParMatriculeAsync("AG0001"))
            .ReturnsAsync(new Administrateur { Matricule = "AG0001", Type = TypeAdmin.Global, SiteId = null });
        _paiementRepositoryMock.Setup(r => r.TraiterPaiementAsync(10, 15.00m)).ReturnsAsync(101);

        var resultat = await _paiementService.TraiterPaiementAsync(10, "AG0001");

        Assert.Equal(101, resultat.PaiementId);
    }

    [Fact]
    public async Task RembourserAsync_AppelleLeRepositoryAvecLeBonPaiementId()
    {
        _paiementRepositoryMock.Setup(r => r.RembourserAsync(5)).Returns(Task.CompletedTask);

        await _paiementService.RembourserAsync(5);

        _paiementRepositoryMock.Verify(r => r.RembourserAsync(5), Times.Once);
    }
}