using Moq;
using Padel.Application.Dtos;
using Padel.Application.Exceptions;
using Padel.Application.Interfaces;
using Padel.Application.Services;
using Padel.Domain.Entities;
using Padel.Domain.Enums;
using Xunit;

namespace Padel.Tests.Services;

public class AdminServiceTests
{
    private readonly Mock<IAdministrateurRepository> _administrateurRepositoryMock;
    private readonly AdminService _adminService;

    public AdminServiceTests()
    {
        _administrateurRepositoryMock = new Mock<IAdministrateurRepository>();
        _adminService = new AdminService(_administrateurRepositoryMock.Object);
    }

    [Fact]
    public async Task CreerAdministrateurAsync_AppelantInconnu_LanceRegleMetierException()
    {
        _administrateurRepositoryMock
            .Setup(r => r.ObtenirParMatriculeAsync("X00000"))
            .ReturnsAsync((Administrateur?)null);

        var dto = new CreerAdministrateurDto { Matricule = "AS0002", Type = "Site", SiteId = 1 };

        var ex = await Assert.ThrowsAsync<RegleMetierException>(
            () => _adminService.CreerAdministrateurAsync(dto, "X00000"));

        Assert.Equal("APPELANT_INCONNU", ex.Code);
    }

    [Fact]
    public async Task CreerAdministrateurAsync_AppelantEstAdminSite_LanceRegleMetierException()
    {
        var appelant = new Administrateur { Matricule = "AS0001", Type = TypeAdmin.Site, SiteId = 1 };
        _administrateurRepositoryMock
            .Setup(r => r.ObtenirParMatriculeAsync("AS0001"))
            .ReturnsAsync(appelant);

        var dto = new CreerAdministrateurDto { Matricule = "AS0002", Type = "Site", SiteId = 1 };

        var ex = await Assert.ThrowsAsync<RegleMetierException>(
            () => _adminService.CreerAdministrateurAsync(dto, "AS0001"));

        Assert.Equal("ACTION_RESERVEE_GLOBAL", ex.Code);
    }

    [Fact]
    public async Task CreerAdministrateurAsync_AppelantEstAdminGlobal_AutoriseLaCreation()
    {
        var appelant = new Administrateur { Matricule = "AG0001", Type = TypeAdmin.Global, SiteId = null };
        _administrateurRepositoryMock
            .Setup(r => r.ObtenirParMatriculeAsync("AG0001"))
            .ReturnsAsync(appelant);

        var dto = new CreerAdministrateurDto { Matricule = "AS0002", Type = "Site", SiteId = 1 };

        await _adminService.CreerAdministrateurAsync(dto, "AG0001");

        _administrateurRepositoryMock.Verify(r => r.CreerAsync("AS0002", "Site", 1), Times.Once);
    }

    [Fact]
    public async Task CreerAdministrateurAsync_TypeSiteSansSiteId_LanceRegleMetierException()
    {
        var appelant = new Administrateur { Matricule = "AG0001", Type = TypeAdmin.Global };
        _administrateurRepositoryMock.Setup(r => r.ObtenirParMatriculeAsync("AG0001")).ReturnsAsync(appelant);

        var dto = new CreerAdministrateurDto { Matricule = "AS0003", Type = "Site", SiteId = null };

        var ex = await Assert.ThrowsAsync<RegleMetierException>(
            () => _adminService.CreerAdministrateurAsync(dto, "AG0001"));

        Assert.Equal("SITE_OBLIGATOIRE", ex.Code);
    }

    [Fact]
    public async Task CreerAdministrateurAsync_TypeGlobalAvecSiteId_LanceRegleMetierException()
    {
        var appelant = new Administrateur { Matricule = "AG0001", Type = TypeAdmin.Global };
        _administrateurRepositoryMock.Setup(r => r.ObtenirParMatriculeAsync("AG0001")).ReturnsAsync(appelant);

        var dto = new CreerAdministrateurDto { Matricule = "AG0002", Type = "Global", SiteId = 1 };

        var ex = await Assert.ThrowsAsync<RegleMetierException>(
            () => _adminService.CreerAdministrateurAsync(dto, "AG0001"));

        Assert.Equal("SITE_NON_APPLICABLE", ex.Code);
    }

    [Fact]
    public async Task CreerAdministrateurAsync_TypeInvalide_LanceRegleMetierException()
    {
        var appelant = new Administrateur { Matricule = "AG0001", Type = TypeAdmin.Global };
        _administrateurRepositoryMock.Setup(r => r.ObtenirParMatriculeAsync("AG0001")).ReturnsAsync(appelant);

        var dto = new CreerAdministrateurDto { Matricule = "X0001", Type = "SuperAdmin", SiteId = null };

        var ex = await Assert.ThrowsAsync<RegleMetierException>(
            () => _adminService.CreerAdministrateurAsync(dto, "AG0001"));

        Assert.Equal("TYPE_INVALIDE", ex.Code);
    }

    [Fact]
    public async Task ObtenirAdministrateurAsync_Inexistant_RetourneNull()
    {
        _administrateurRepositoryMock
            .Setup(r => r.ObtenirParMatriculeAsync("Z00000"))
            .ReturnsAsync((Administrateur?)null);

        var resultat = await _adminService.ObtenirAdministrateurAsync("Z00000");

        Assert.Null(resultat);
    }

    [Fact]
    public async Task ObtenirAdministrateurAsync_Existant_RetourneDtoAvecTypeEnString()
    {
        var admin = new Administrateur { Matricule = "AG0001", Type = TypeAdmin.Global, SiteId = null };
        _administrateurRepositoryMock.Setup(r => r.ObtenirParMatriculeAsync("AG0001")).ReturnsAsync(admin);

        var resultat = await _adminService.ObtenirAdministrateurAsync("AG0001");

        Assert.NotNull(resultat);
        Assert.Equal("Global", resultat!.Type);
        Assert.Null(resultat.SiteId);
    }
}