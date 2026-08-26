using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Moq;
using Padel.Application.Dtos;
using Padel.Application.Interfaces;
using Padel.Application.Services;
using Padel.Domain.Entities;
using Xunit;

namespace Padel.Tests.Services;

public class SiteServiceTests
{
    private readonly Mock<ISiteRepository> _siteRepositoryMock;
    private readonly SiteService _siteService;

    public SiteServiceTests()
    {
        _siteRepositoryMock = new Mock<ISiteRepository>();
        _siteService = new SiteService(_siteRepositoryMock.Object);
    }

    [Fact]
    public async Task CreerSiteAsync_AvecNomValide_RetourneSiteDtoAvecId()
    {
        // Arrange
        var dto = new CreerSiteDto { Nom = "Padel Club Liège" };
        _siteRepositoryMock
            .Setup(r => r.CreerAsync(dto.Nom))
            .ReturnsAsync(42);

        // Act
        var resultat = await _siteService.CreerSiteAsync(dto);

        // Assert
        Assert.Equal(42, resultat.SiteId);
        Assert.Equal("Padel Club Liège", resultat.Nom);
        _siteRepositoryMock.Verify(r => r.CreerAsync(dto.Nom), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreerSiteAsync_AvecNomVideOuNull_LanceArgumentException(string? nomInvalide)
    {
        // Arrange
        var dto = new CreerSiteDto { Nom = nomInvalide! };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _siteService.CreerSiteAsync(dto));

        // Le Repository ne doit jamais être appelé si la validation échoue avant
        _siteRepositoryMock.Verify(r => r.CreerAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ListerSitesAsync_RetourneListeDeSiteDto()
    {
        // Arrange
        var sites = new List<Site>
        {
            new() { SiteId = 1, Nom = "Padel Club Bruxelles" },
            new() { SiteId = 2, Nom = "Padel Club Namur" }
        };
        _siteRepositoryMock.Setup(r => r.ListerAsync()).ReturnsAsync(sites);

        // Act
        var resultat = await _siteService.ListerSitesAsync();

        // Assert
        Assert.Equal(2, resultat.Count);
        Assert.Contains(resultat, s => s.Nom == "Padel Club Bruxelles");
    }

    [Fact]
    public async Task ObtenirSiteAsync_AvecIdInexistant_RetourneNull()
    {
        // Arrange
        _siteRepositoryMock.Setup(r => r.ObtenirParIdAsync(999)).ReturnsAsync((Site?)null);

        // Act
        var resultat = await _siteService.ObtenirSiteAsync(999);

        // Assert
        Assert.Null(resultat);
    }
}