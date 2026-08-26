using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Padel.Application.Dtos;
using Padel.Application.Interfaces;
using Xunit;

namespace Padel.Tests.Controllers;

public class AdminControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AdminControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private HttpClient CreerClientAvecServicesMockes(
        Mock<IAdminService>? adminServiceMock = null,
        Mock<IStatistiqueService>? statistiqueServiceMock = null,
        Mock<IMembreService>? membreServiceMock = null)
    {
        var factoryConfiguree = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                if (adminServiceMock is not null)
                {
                    services.RemoveAll<IAdminService>();
                    services.AddScoped(_ => adminServiceMock.Object);
                }

                if (statistiqueServiceMock is not null)
                {
                    services.RemoveAll<IStatistiqueService>();
                    services.AddScoped(_ => statistiqueServiceMock.Object);
                }

                if (membreServiceMock is not null)
                {
                    services.RemoveAll<IMembreService>();
                    services.AddScoped(_ => membreServiceMock.Object);
                }
            });
        });

        return factoryConfiguree.CreateClient();
    }

    // Rappel définitif : le middleware résout un matricule 'A...' via IAdminService
    // (pas IAdministrateurRepository) — c'est la dépendance réellement injectée
    // dans MatriculeAuthMiddleware.InvokeAsync.
    private Mock<IAdminService> CreerAdminServiceMockAvecUnAdmin(string matricule, string type, int? siteId = null)
    {
        var mock = new Mock<IAdminService>();
        mock.Setup(s => s.ObtenirAdministrateurAsync(matricule))
            .ReturnsAsync(new AdministrateurDto { Matricule = matricule, Type = type, SiteId = siteId });
        return mock;
    }

    private Mock<IMembreService> CreerMembreServiceMockAvecUnMembre(string matricule)
    {
        var mock = new Mock<IMembreService>();
        mock.Setup(s => s.ObtenirMembreAsync(matricule))
            .ReturnsAsync(new MembreDto { Matricule = matricule, Type = "Global", SoldeDu = 0 });
        return mock;
    }

    [Fact]
    public async Task PostAdministrateur_AppelantGlobal_Retourne201()
    {
        var adminServiceMock = CreerAdminServiceMockAvecUnAdmin("AG0001", "Global");
        adminServiceMock
            .Setup(s => s.CreerAdministrateurAsync(It.IsAny<CreerAdministrateurDto>(), "AG0001"))
            .Returns(Task.CompletedTask);

        var client = CreerClientAvecServicesMockes(adminServiceMock);
        client.DefaultRequestHeaders.Add("X-Matricule", "AG0001");

        var dto = new CreerAdministrateurDto { Matricule = "AS0099", Type = "Site", SiteId = 1 };
        var reponse = await client.PostAsJsonAsync("/api/Admin/administrateurs", dto);

        Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);
    }

    [Fact]
    public async Task GetAdministrateur_AppelantEstUnMembre_Retourne400()
    {
        var membreServiceMock = CreerMembreServiceMockAvecUnMembre("G00001");

        var client = CreerClientAvecServicesMockes(membreServiceMock: membreServiceMock);
        client.DefaultRequestHeaders.Add("X-Matricule", "G00001");

        var reponse = await client.GetAsync("/api/Admin/administrateurs/AG0001");

        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
    }

    [Fact]
    public async Task GetAdministrateur_AppelantEstAdmin_Retourne200()
    {
        // L'appelant (AS0001, Site) et la cible consultée (AG0001, Global) sont deux
        // matricules différents résolus par le même mock IAdminService, chacun avec
        // son propre Setup — Moq distingue les appels par la valeur exacte de l'argument.
        var adminServiceMock = CreerAdminServiceMockAvecUnAdmin("AS0001", "Site", siteId: 1);
        adminServiceMock
            .Setup(s => s.ObtenirAdministrateurAsync("AG0001"))
            .ReturnsAsync(new AdministrateurDto { Matricule = "AG0001", Type = "Global", SiteId = null });

        var client = CreerClientAvecServicesMockes(adminServiceMock);
        client.DefaultRequestHeaders.Add("X-Matricule", "AS0001");

        var reponse = await client.GetAsync("/api/Admin/administrateurs/AG0001");

        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
        var admin = await reponse.Content.ReadFromJsonAsync<AdministrateurDto>();
        Assert.Equal("AG0001", admin!.Matricule);
    }

    [Fact]
    public async Task GetChiffreAffaires_AdminGlobal_TransmetSiteIdNull()
    {
        var adminServiceMock = CreerAdminServiceMockAvecUnAdmin("AG0002", "Global");

        var statistiqueServiceMock = new Mock<IStatistiqueService>();
        statistiqueServiceMock
            .Setup(s => s.ObtenirChiffreAffairesAsync(null, It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(new List<ChiffreAffairesDto>
            {
                new() { SiteId = 1, NomSite = "Padel Club Bruxelles", Montant = 75m }
            });

        var client = CreerClientAvecServicesMockes(adminServiceMock, statistiqueServiceMock);
        client.DefaultRequestHeaders.Add("X-Matricule", "AG0002");

        var reponse = await client.GetAsync("/api/Admin/chiffre-affaires?dateDebut=2026-01-01&dateFin=2026-12-31");

        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
        statistiqueServiceMock.Verify(
            s => s.ObtenirChiffreAffairesAsync(null, It.IsAny<DateOnly>(), It.IsAny<DateOnly>()), Times.Once);
    }

    [Fact]
    public async Task GetChiffreAffaires_AdminSite_TransmetSonPropreSiteIdUniquement()
    {
        var adminServiceMock = CreerAdminServiceMockAvecUnAdmin("AS0003", "Site", siteId: 5);

        var statistiqueServiceMock = new Mock<IStatistiqueService>();
        statistiqueServiceMock
            .Setup(s => s.ObtenirChiffreAffairesAsync(5, It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(new List<ChiffreAffairesDto>());

        var client = CreerClientAvecServicesMockes(adminServiceMock, statistiqueServiceMock);
        client.DefaultRequestHeaders.Add("X-Matricule", "AS0003");

        var reponse = await client.GetAsync("/api/Admin/chiffre-affaires?dateDebut=2026-01-01&dateFin=2026-12-31");

        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
        statistiqueServiceMock.Verify(
            s => s.ObtenirChiffreAffairesAsync(5, It.IsAny<DateOnly>(), It.IsAny<DateOnly>()), Times.Once);
        statistiqueServiceMock.Verify(
            s => s.ObtenirChiffreAffairesAsync(It.Is<int?>(id => id != 5), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()),
            Times.Never);
    }

    [Fact]
    public async Task GetImpayes_AppelantEstUnMembre_Retourne400()
    {
        var membreServiceMock = CreerMembreServiceMockAvecUnMembre("L00001");

        var client = CreerClientAvecServicesMockes(membreServiceMock: membreServiceMock);
        client.DefaultRequestHeaders.Add("X-Matricule", "L00001");

        var reponse = await client.GetAsync("/api/Admin/impayes");

        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
    }

    [Fact]
    public async Task GetPenalitesActives_AdminGlobal_Retourne200()
    {
        var adminServiceMock = CreerAdminServiceMockAvecUnAdmin("AG0004", "Global");

        var statistiqueServiceMock = new Mock<IStatistiqueService>();
        statistiqueServiceMock
            .Setup(s => s.ObtenirPenalitesActivesAsync(null))
            .ReturnsAsync(new List<PenaliteActiveDto>
            {
                new() { Matricule = "G00005", Type = "Global", DateProchaineReservationAutorisee = new DateOnly(2026, 9, 1) }
            });

        var client = CreerClientAvecServicesMockes(adminServiceMock, statistiqueServiceMock);
        client.DefaultRequestHeaders.Add("X-Matricule", "AG0004");

        var reponse = await client.GetAsync("/api/Admin/penalites-actives");

        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
        var penalites = await reponse.Content.ReadFromJsonAsync<List<PenaliteActiveDto>>();
        Assert.Single(penalites!);
    }
}