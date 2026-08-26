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

namespace Padel.Tests.Services;

public class MembreServiceTests
{
    private readonly Mock<IMembreRepository> _membreRepositoryMock;
    private readonly MembreService _membreService;

    public MembreServiceTests()
    {
        _membreRepositoryMock = new Mock<IMembreRepository>();
        _membreService = new MembreService(_membreRepositoryMock.Object);
    }

    [Fact]
    public async Task CreerMembreAsync_MatriculeManquant_LanceRegleMetierException()
    {
        var dto = new CreerMembreDto { Matricule = "", Type = "Global" };

        var ex = await Assert.ThrowsAsync<RegleMetierException>(
            () => _membreService.CreerMembreAsync(dto));

        Assert.Equal("MATRICULE_OBLIGATOIRE", ex.Code);
    }

    [Fact]
    public async Task CreerMembreAsync_TypeInvalide_LanceRegleMetierException()
    {
        var dto = new CreerMembreDto { Matricule = "X00001", Type = "Inexistant" };

        var ex = await Assert.ThrowsAsync<RegleMetierException>(
            () => _membreService.CreerMembreAsync(dto));

        Assert.Equal("TYPE_INVALIDE", ex.Code);
    }

    [Fact]
    public async Task CreerMembreAsync_TypeSiteSansSiteId_LanceRegleMetierException()
    {
        var dto = new CreerMembreDto { Matricule = "S00002", Type = "Site", SiteId = null };

        var ex = await Assert.ThrowsAsync<RegleMetierException>(
            () => _membreService.CreerMembreAsync(dto));

        Assert.Equal("SITE_OBLIGATOIRE", ex.Code);
    }

    [Theory]
    [InlineData("Global")]
    [InlineData("Libre")]
    public async Task CreerMembreAsync_TypeNonSiteAvecSiteId_LanceRegleMetierException(string type)
    {
        var dto = new CreerMembreDto { Matricule = "X00003", Type = type, SiteId = 1 };

        var ex = await Assert.ThrowsAsync<RegleMetierException>(
            () => _membreService.CreerMembreAsync(dto));

        Assert.Equal("SITE_NON_APPLICABLE", ex.Code);
    }

    [Fact]
    public async Task CreerMembreAsync_TypeSiteAvecSiteId_AppelleRepository()
    {
        var dto = new CreerMembreDto { Matricule = "S00003", Type = "Site", SiteId = 2 };

        await _membreService.CreerMembreAsync(dto);

        _membreRepositoryMock.Verify(r => r.CreerAsync("S00003", "Site", 2), Times.Once);
    }

    [Fact]
    public async Task CreerMembreAsync_TypeGlobalSansSiteId_AppelleRepository()
    {
        var dto = new CreerMembreDto { Matricule = "G00099", Type = "Global", SiteId = null };

        await _membreService.CreerMembreAsync(dto);

        _membreRepositoryMock.Verify(r => r.CreerAsync("G00099", "Global", null), Times.Once);
    }

    [Fact]
    public async Task ObtenirMembreAsync_MembreInexistant_RetourneNull()
    {
        _membreRepositoryMock.Setup(r => r.ObtenirParMatriculeAsync("Z00000")).ReturnsAsync((Membre?)null);

        var resultat = await _membreService.ObtenirMembreAsync("Z00000");

        Assert.Null(resultat);
    }

    [Fact]
    public async Task ObtenirMembreAsync_MembreExistant_RetourneMembreDtoAvecTypeEnString()
    {
        var membre = new Membre { Matricule = "G00001", Type = TypeMembre.Global, SoldeDu = 30m };
        _membreRepositoryMock.Setup(r => r.ObtenirParMatriculeAsync("G00001")).ReturnsAsync(membre);

        var resultat = await _membreService.ObtenirMembreAsync("G00001");

        Assert.NotNull(resultat);
        Assert.Equal("Global", resultat!.Type);
        Assert.Equal(30m, resultat.SoldeDu);
    }

    [Fact]
    public async Task ListerMembresAsync_TransmetSiteIdTelQuelAuRepository()
    {
        _membreRepositoryMock.Setup(r => r.ListerAsync(3)).ReturnsAsync(new List<Membre>());

        await _membreService.ListerMembresAsync(3);

        _membreRepositoryMock.Verify(r => r.ListerAsync(3), Times.Once);
    }

    [Fact]
    public async Task ListerMembresAsync_SiteIdNull_TransmetNullAuRepository()
    {
        _membreRepositoryMock.Setup(r => r.ListerAsync(null)).ReturnsAsync(new List<Membre>());

        await _membreService.ListerMembresAsync(null);

        _membreRepositoryMock.Verify(r => r.ListerAsync(null), Times.Once);
    }
}