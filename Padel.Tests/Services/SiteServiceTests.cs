using Moq;
using Padel.Application.Dtos;
using Padel.Application.Exceptions;
using Padel.Application.Interfaces;
using Padel.Application.Services;
using Padel.Domain.Entities;
using Padel.Domain.Enums;
using Xunit;

namespace Padel.Tests.Services;

public class SiteServiceTests
{
    private readonly Mock<ISiteRepository> _siteRepositoryMock;
    private readonly Mock<IAdministrateurRepository> _administrateurRepositoryMock;
    private readonly SiteService _siteService;

    public SiteServiceTests()
    {
        _siteRepositoryMock = new Mock<ISiteRepository>();
        _administrateurRepositoryMock = new Mock<IAdministrateurRepository>();
        _siteService = new SiteService(_siteRepositoryMock.Object, _administrateurRepositoryMock.Object);
    }

    [Fact]
    public async Task CreerSiteAsync_AppelantInconnu_LanceRegleMetierException()
    {
        _administrateurRepositoryMock
            .Setup(r => r.ObtenirParMatriculeAsync("AX0000"))
            .ReturnsAsync((Administrateur?)null);

        var dto = new CreerSiteDto { Nom = "Padel Club Liège" };

        var ex = await Assert.ThrowsAsync<RegleMetierException>(
            () => _siteService.CreerSiteAsync(dto, "AX0000"));

        Assert.Equal("APPELANT_INCONNU", ex.Code);
    }

    [Fact]
    public async Task CreerSiteAsync_AppelantEstAdminSite_LanceRegleMetierException()
    {
        var appelant = new Administrateur { Matricule = "AS0001", Type = TypeAdmin.Site, SiteId = 1 };
        _administrateurRepositoryMock
            .Setup(r => r.ObtenirParMatriculeAsync("AS0001"))
            .ReturnsAsync(appelant);

        var dto = new CreerSiteDto { Nom = "Padel Club Liège" };

        var ex = await Assert.ThrowsAsync<RegleMetierException>(
            () => _siteService.CreerSiteAsync(dto, "AS0001"));

        Assert.Equal("ACTION_RESERVEE_GLOBAL", ex.Code);
        _siteRepositoryMock.Verify(r => r.CreerAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreerSiteAsync_AppelantEstAdminGlobalAvecNomValide_RetourneSiteDtoAvecId()
    {
        var appelant = new Administrateur { Matricule = "AG0001", Type = TypeAdmin.Global, SiteId = null };
        _administrateurRepositoryMock
            .Setup(r => r.ObtenirParMatriculeAsync("AG0001"))
            .ReturnsAsync(appelant);

        var dto = new CreerSiteDto { Nom = "Padel Club Liège" };
        _siteRepositoryMock.Setup(r => r.CreerAsync(dto.Nom)).ReturnsAsync(42);

        var resultat = await _siteService.CreerSiteAsync(dto, "AG0001");

        Assert.Equal(42, resultat.SiteId);
        Assert.Equal("Padel Club Liège", resultat.Nom);
        _siteRepositoryMock.Verify(r => r.CreerAsync(dto.Nom), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreerSiteAsync_NomVideMemeSiAppelantValide_LanceRegleMetierException(string nomInvalide)
    {
        var appelant = new Administrateur { Matricule = "AG0001", Type = TypeAdmin.Global, SiteId = null };
        _administrateurRepositoryMock
            .Setup(r => r.ObtenirParMatriculeAsync("AG0001"))
            .ReturnsAsync(appelant);

        var dto = new CreerSiteDto { Nom = nomInvalide };

        var ex = await Assert.ThrowsAsync<RegleMetierException>(
            () => _siteService.CreerSiteAsync(dto, "AG0001"));

        Assert.Equal("NOM_OBLIGATOIRE", ex.Code);
        _siteRepositoryMock.Verify(r => r.CreerAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ListerSitesAsync_RetourneListeDeSiteDto()
    {
        var sites = new List<Site>
        {
            new() { SiteId = 1, Nom = "Padel Club Bruxelles" },
            new() { SiteId = 2, Nom = "Padel Club Namur" }
        };
        _siteRepositoryMock.Setup(r => r.ListerAsync()).ReturnsAsync(sites);

        var resultat = await _siteService.ListerSitesAsync();

        Assert.Equal(2, resultat.Count);
        Assert.Contains(resultat, s => s.Nom == "Padel Club Bruxelles");
    }

    [Fact]
    public async Task ObtenirSiteAsync_AvecIdInexistant_RetourneNull()
    {
        _siteRepositoryMock.Setup(r => r.ObtenirParIdAsync(999)).ReturnsAsync((Site?)null);

        var resultat = await _siteService.ObtenirSiteAsync(999);

        Assert.Null(resultat);
    }
}