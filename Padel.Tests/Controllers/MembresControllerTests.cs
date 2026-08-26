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

public class MembresControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public MembresControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private HttpClient CreerClientAvecServicesMockes(
        Mock<IMembreService>? membreServiceMock = null,
        Mock<IAdministrateurRepository>? administrateurRepositoryMock = null)
    {
        var factoryConfiguree = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                if (membreServiceMock is not null)
                {
                    services.RemoveAll<IMembreService>();
                    services.AddScoped(_ => membreServiceMock.Object);
                }

                if (administrateurRepositoryMock is not null)
                {
                    services.RemoveAll<IAdministrateurRepository>();
                    services.AddScoped(_ => administrateurRepositoryMock.Object);
                }
            });
        });

        return factoryConfiguree.CreateClient();
    }

    [Fact]
    public async Task PostMembre_AvecMembreAppelant_Retourne201()
    {
        // Vérifie que la création reste ouverte : l'appelant est un simple Membre, pas un Admin.
        var membreServiceMock = new Mock<IMembreService>();
        membreServiceMock
            .Setup(s => s.ObtenirMembreAsync("G00001"))
            .ReturnsAsync(new MembreDto { Matricule = "G00001", Type = "Global", SoldeDu = 0 });
        membreServiceMock
            .Setup(s => s.CreerMembreAsync(It.IsAny<CreerMembreDto>()))
            .Returns(Task.CompletedTask);

        var client = CreerClientAvecServicesMockes(membreServiceMock);
        client.DefaultRequestHeaders.Add("X-Matricule", "G00001");

        var dto = new CreerMembreDto { Matricule = "L00099", Type = "Libre" };
        var reponse = await client.PostAsJsonAsync("/api/Membres", dto);

        Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);
    }

    [Fact]
    public async Task GetMembre_Existant_Retourne200()
    {
        var membreServiceMock = new Mock<IMembreService>();
        membreServiceMock
            .Setup(s => s.ObtenirMembreAsync("G00001"))
            .ReturnsAsync(new MembreDto { Matricule = "G00001", Type = "Global", SoldeDu = 0 });
        membreServiceMock
            .Setup(s => s.ObtenirMembreAsync("L00001"))
            .ReturnsAsync(new MembreDto { Matricule = "L00001", Type = "Libre", SoldeDu = 0 });

        var client = CreerClientAvecServicesMockes(membreServiceMock);
        client.DefaultRequestHeaders.Add("X-Matricule", "G00001");

        var reponse = await client.GetAsync("/api/Membres/L00001");

        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
        var membre = await reponse.Content.ReadFromJsonAsync<MembreDto>();
        Assert.Equal("L00001", membre!.Matricule);
    }

    [Fact]
    public async Task GetMembre_Inexistant_Retourne404()
    {
        var membreServiceMock = new Mock<IMembreService>();
        membreServiceMock
            .Setup(s => s.ObtenirMembreAsync("G00001"))
            .ReturnsAsync(new MembreDto { Matricule = "G00001", Type = "Global", SoldeDu = 0 });
        membreServiceMock
            .Setup(s => s.ObtenirMembreAsync("Z00000"))
            .ReturnsAsync((MembreDto?)null);

        var client = CreerClientAvecServicesMockes(membreServiceMock);
        client.DefaultRequestHeaders.Add("X-Matricule", "G00001");

        var reponse = await client.GetAsync("/api/Membres/Z00000");

        Assert.Equal(HttpStatusCode.NotFound, reponse.StatusCode);
    }

    [Fact]
    public async Task GetMembres_AppelantAdminGlobal_TransmetSiteIdNull()
    {
        var adminRepoMock = new Mock<IAdministrateurRepository>();
        adminRepoMock
            .Setup(r => r.ObtenirParMatriculeAsync("AG0001"))
            .ReturnsAsync(new Administrateur { Matricule = "AG0001", Type = TypeAdmin.Global, SiteId = null });

        var membreServiceMock = new Mock<IMembreService>();
        membreServiceMock
            .Setup(s => s.ListerMembresAsync(null))
            .ReturnsAsync(new List<MembreDto>
            {
                new() { Matricule = "G00001", Type = "Global", SoldeDu = 0 },
                new() { Matricule = "S00001", Type = "Site", SiteId = 1, SoldeDu = 0 }
            });

        var client = CreerClientAvecServicesMockes(membreServiceMock, adminRepoMock);
        client.DefaultRequestHeaders.Add("X-Matricule", "AG0001");

        var reponse = await client.GetAsync("/api/Membres");

        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
        var membres = await reponse.Content.ReadFromJsonAsync<List<MembreDto>>();
        Assert.Equal(2, membres!.Count);
        membreServiceMock.Verify(s => s.ListerMembresAsync(null), Times.Once);
    }

    [Fact]
    public async Task GetMembres_AppelantAdminSite_TransmetSonPropreSiteIdUniquement()
    {
        // Test de sécurité central de cette Issue : l'Admin Site (SiteId=3) ne doit
        // JAMAIS pouvoir faire remonter autre chose que son propre périmètre.
        var adminRepoMock = new Mock<IAdministrateurRepository>();
        adminRepoMock
            .Setup(r => r.ObtenirParMatriculeAsync("AS0002"))
            .ReturnsAsync(new Administrateur { Matricule = "AS0002", Type = TypeAdmin.Site, SiteId = 3 });

        var membreServiceMock = new Mock<IMembreService>();
        membreServiceMock
            .Setup(s => s.ListerMembresAsync(3))
            .ReturnsAsync(new List<MembreDto>
            {
                new() { Matricule = "S00005", Type = "Site", SiteId = 3, SoldeDu = 0 }
            });

        var client = CreerClientAvecServicesMockes(membreServiceMock, adminRepoMock);
        client.DefaultRequestHeaders.Add("X-Matricule", "AS0002");

        var reponse = await client.GetAsync("/api/Membres");

        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);

        // Vérification stricte : ListerMembresAsync a été appelé avec 3, jamais avec null
        // ni un autre SiteId — même si un attaquant tentait de le forcer autrement,
        // ce test garantit que le Controller ignore toute tentative externe.
        membreServiceMock.Verify(s => s.ListerMembresAsync(3), Times.Once);
        membreServiceMock.Verify(s => s.ListerMembresAsync(It.Is<int?>(id => id != 3)), Times.Never);
    }

    [Fact]
    public async Task GetMembres_AppelantEstUnSimpleMembre_Retourne400()
    {
        var membreServiceMock = new Mock<IMembreService>();
        membreServiceMock
            .Setup(s => s.ObtenirMembreAsync("G00001"))
            .ReturnsAsync(new MembreDto { Matricule = "G00001", Type = "Global", SoldeDu = 0 });

        var client = CreerClientAvecServicesMockes(membreServiceMock);
        client.DefaultRequestHeaders.Add("X-Matricule", "G00001");

        var reponse = await client.GetAsync("/api/Membres");

        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
        membreServiceMock.Verify(s => s.ListerMembresAsync(It.IsAny<int?>()), Times.Never);
    }
}