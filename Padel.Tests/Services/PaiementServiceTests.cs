using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Moq;
using Padel.Application.Interfaces;
using Padel.Application.Services;
using Xunit;

namespace Padel.Tests.Services;

public class PaiementServiceTests
{
    private readonly Mock<IPaiementRepository> _paiementRepositoryMock;
    private readonly PaiementService _paiementService;

    public PaiementServiceTests()
    {
        _paiementRepositoryMock = new Mock<IPaiementRepository>();
        _paiementService = new PaiementService(_paiementRepositoryMock.Object);
    }

    [Fact]
    public async Task TraiterPaiementAsync_RetournePaiementDtoAvecMontantFixe()
    {
        // Arrange
        _paiementRepositoryMock
            .Setup(r => r.TraiterPaiementAsync(10, 15.00m))
            .ReturnsAsync(100);

        // Act
        var resultat = await _paiementService.TraiterPaiementAsync(10);

        // Assert
        Assert.Equal(100, resultat.PaiementId);
        Assert.Equal(10, resultat.InscriptionId);
        Assert.Equal(15.00m, resultat.Montant);
        Assert.False(resultat.EstRembourse);
        Assert.Null(resultat.DateRemboursement);
    }

    [Fact]
    public async Task TraiterPaiementAsync_AppelleRepositoryAvecMontantFixeQuelQueSoitInscriptionId()
    {
        // Arrange
        _paiementRepositoryMock
            .Setup(r => r.TraiterPaiementAsync(It.IsAny<int>(), It.IsAny<decimal>()))
            .ReturnsAsync(1);

        // Act
        await _paiementService.TraiterPaiementAsync(42);

        // Assert : le montant transmis au Repository doit toujours être 15.00m, jamais autre chose
        _paiementRepositoryMock.Verify(r => r.TraiterPaiementAsync(42, 15.00m), Times.Once);
    }

    [Fact]
    public async Task RembourserAsync_AppelleLeRepositoryAvecLeBonPaiementId()
    {
        // Arrange
        _paiementRepositoryMock.Setup(r => r.RembourserAsync(5)).Returns(Task.CompletedTask);

        // Act
        await _paiementService.RembourserAsync(5);

        // Assert
        _paiementRepositoryMock.Verify(r => r.RembourserAsync(5), Times.Once);
    }
}