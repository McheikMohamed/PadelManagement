using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Padel.Application.Dtos;
using Padel.Application.Interfaces;
using Padel.Domain.Entities;
using Padel.Domain.Enums;
using Xunit;

namespace Padel.Tests.Controllers;

public class SitesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SitesControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private HttpClient CreerClientAvecServicesMockes(
        Mock<ISiteService>? siteServiceMock = null,
        Mock<IAdministrateurRepository>? administrateurRepositoryMock = null,
        Mock<IMembreRepository>? membreRepositoryMock = null)
    {
        var factoryConfiguree = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // On retire les vraies implémentations pour les remplacer par des mocks,
                // évitant ainsi tout appel réel à SQL Server pendant ces tests.
                if (siteServiceMock is not null)
                {
                    services.RemoveAll<ISiteService>();
                    services.AddScoped(_ => siteServiceMock.Object);
                }

                if (administrateurRepositoryMock is not null)
                {
                    services.RemoveAll<IAdministrateurRepository>();
                    services.AddScoped(_ => administrateurRepositoryMock.Object);
                }

                if (membreRepositoryMock is not null)
                {
                    services.RemoveAll<IMembreRepository>();
                    services.AddScoped(_ => membreRepositoryMock.Object);
                }
            });
        });

        return factoryConfiguree.CreateClient();
    }

    [Fact]
    public async Task GetSites_SansHeaderMatricule_Retourne401()
    {
        // Arrange
        var client = CreerClientAvecServicesMockes();

        // Act
        var reponse = await client.GetAsync("/api/Sites");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, reponse.StatusCode);
    }

    [Fact]
    public async Task GetSites_AvecMatriculeValide_Retourne200EtLaListe()
    {
        // Arrange
        var adminMock = new Mock<IAdministrateurRepository>();
        adminMock
            .Setup(r => r.ObtenirParMatriculeAsync("AG0001"))
            .ReturnsAsync(new Administrateur { Matricule = "AG0001", Type = TypeAdmin.Global, SiteId = null });

        var siteServiceMock = new Mock<ISiteService>();
        siteServiceMock
            .Setup(s => s.ListerSitesAsync())
            .ReturnsAsync(new List<SiteDto>
            {
                new() { SiteId = 1, Nom = "Padel Club Bruxelles" }
            });

        var client = CreerClientAvecServicesMockes(siteServiceMock, adminMock);
        client.DefaultRequestHeaders.Add("X-Matricule", "AG0001");

        // Act
        var reponse = await client.GetAsync("/api/Sites");

        // Assert
        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
        var sites = await reponse.Content.ReadFromJsonAsync<List<SiteDto>>();
        Assert.Single(sites!);
        Assert.Equal("Padel Club Bruxelles", sites![0].Nom);
    }

    [Fact]
    public async Task GetSiteParId_IdInexistant_Retourne404()
    {
        // Arrange
        var adminMock = new Mock<IAdministrateurRepository>();
        adminMock
            .Setup(r => r.ObtenirParMatriculeAsync("AG0001"))
            .ReturnsAsync(new Administrateur { Matricule = "AG0001", Type = TypeAdmin.Global, SiteId = null });

        var siteServiceMock = new Mock<ISiteService>();
        siteServiceMock
            .Setup(s => s.ObtenirSiteAsync(999))
            .ReturnsAsync((SiteDto?)null);

        var client = CreerClientAvecServicesMockes(siteServiceMock, adminMock);
        client.DefaultRequestHeaders.Add("X-Matricule", "AG0001");

        // Act
        var reponse = await client.GetAsync("/api/Sites/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, reponse.StatusCode);
    }

    [Fact]
    public async Task PostSite_AvecAdminGlobal_Retourne201EtLocation()
    {
        // Arrange
        var adminMock = new Mock<IAdministrateurRepository>();
        adminMock
            .Setup(r => r.ObtenirParMatriculeAsync("AG0001"))
            .ReturnsAsync(new Administrateur { Matricule = "AG0001", Type = TypeAdmin.Global, SiteId = null });

        var siteServiceMock = new Mock<ISiteService>();
        siteServiceMock
            .Setup(s => s.CreerSiteAsync(It.IsAny<CreerSiteDto>(), "AG0001"))
            .ReturnsAsync(new SiteDto { SiteId = 42, Nom = "Padel Club Test" });

        var client = CreerClientAvecServicesMockes(siteServiceMock, adminMock);
        client.DefaultRequestHeaders.Add("X-Matricule", "AG0001");

        // Act
        var reponse = await client.PostAsJsonAsync("/api/Sites", new CreerSiteDto { Nom = "Padel Club Test" });

        // Assert
        Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);
        Assert.NotNull(reponse.Headers.Location);
        var siteCree = await reponse.Content.ReadFromJsonAsync<SiteDto>();
        Assert.Equal(42, siteCree!.SiteId);
    }

    [Fact]
    public async Task PostSite_AvecAdminSite_Retourne400AvecCodeActionReserveeGlobal()
    {
        // Arrange
        var adminMock = new Mock<IAdministrateurRepository>();
        adminMock
            .Setup(r => r.ObtenirParMatriculeAsync("AS0001"))
            .ReturnsAsync(new Administrateur { Matricule = "AS0001", Type = TypeAdmin.Site, SiteId = 1 });

        var siteServiceMock = new Mock<ISiteService>();
        siteServiceMock
            .Setup(s => s.CreerSiteAsync(It.IsAny<CreerSiteDto>(), "AS0001"))
            .ThrowsAsync(new Padel.Application.Exceptions.RegleMetierException(
                "ACTION_RESERVEE_GLOBAL", "Seul un administrateur global peut créer un site."));

        var client = CreerClientAvecServicesMockes(siteServiceMock, adminMock);
        client.DefaultRequestHeaders.Add("X-Matricule", "AS0001");

        // Act
        var reponse = await client.PostAsJsonAsync("/api/Sites", new CreerSiteDto { Nom = "Padel Club Test" });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, reponse.StatusCode);
        var corps = await reponse.Content.ReadAsStringAsync();
        Assert.Contains("ACTION_RESERVEE_GLOBAL", corps);
    }
}