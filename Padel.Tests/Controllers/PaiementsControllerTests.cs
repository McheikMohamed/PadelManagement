using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Padel.Application.Dtos;
using Padel.Application.Exceptions;
using Padel.Application.Interfaces;
using Xunit;

namespace Padel.Tests.Controllers;

public class PaiementsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PaiementsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private HttpClient CreerClientAvecServicesMockes(
        Mock<IPaiementService>? paiementServiceMock = null,
        Mock<IMembreService>? membreServiceMock = null)
    {
        var factoryConfiguree = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                if (paiementServiceMock is not null)
                {
                    services.RemoveAll<IPaiementService>();
                    services.AddScoped(_ => paiementServiceMock.Object);
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

    // Rappel : le middleware résout l'identité via IMembreService, pas IMembreRepository.
    private Mock<IMembreService> CreerMembreServiceMockAvecUnMembre(string matricule)
    {
        var mock = new Mock<IMembreService>();
        mock.Setup(s => s.ObtenirMembreAsync(matricule))
            .ReturnsAsync(new MembreDto { Matricule = matricule, Type = "Global", SoldeDu = 0 });
        return mock;
    }

    [Fact]
    public async Task PostPaiement_CasValide_Retourne200()
    {
        var membreServiceMock = CreerMembreServiceMockAvecUnMembre("G00001");

        var paiementServiceMock = new Mock<IPaiementService>();
        paiementServiceMock
            .Setup(s => s.TraiterPaiementAsync(10, "G00001"))
            .ReturnsAsync(new PaiementDto
            {
                PaiementId = 100,
                InscriptionId = 10,
                Montant = 15.00m,
                DateHeure = DateTime.Now,
                EstRembourse = false
            });

        var client = CreerClientAvecServicesMockes(paiementServiceMock, membreServiceMock);
        client.DefaultRequestHeaders.Add("X-Matricule", "G00001");

        var reponse = await client.PostAsync("/api/Paiements/10", null);

        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
        var paiement = await reponse.Content.ReadFromJsonAsync<PaiementDto>();
        Assert.Equal(100, paiement!.PaiementId);
    }

    [Fact]
    public async Task PostPaiement_NonAutorise_Retourne403AvecCode()
    {
        var membreServiceMock = CreerMembreServiceMockAvecUnMembre("L00002");

        var paiementServiceMock = new Mock<IPaiementService>();
        paiementServiceMock
            .Setup(s => s.TraiterPaiementAsync(10, "L00002"))
            .ThrowsAsync(new RegleMetierException(
                "PAIEMENT_NON_AUTORISE", "Seul le membre concerné par cette inscription peut la payer."));

        var client = CreerClientAvecServicesMockes(paiementServiceMock, membreServiceMock);
        client.DefaultRequestHeaders.Add("X-Matricule", "L00002");

        var reponse = await client.PostAsync("/api/Paiements/10", null);

        Assert.Equal(HttpStatusCode.Forbidden, reponse.StatusCode);
        var corps = await reponse.Content.ReadAsStringAsync();
        Assert.Contains("PAIEMENT_NON_AUTORISE", corps);
    }

    [Fact]
    public async Task PostRemboursement_CasValide_Retourne204()
    {
        var membreServiceMock = CreerMembreServiceMockAvecUnMembre("G00003");

        var paiementServiceMock = new Mock<IPaiementService>();
        paiementServiceMock
            .Setup(s => s.RembourserAsync(50))
            .Returns(Task.CompletedTask);

        var client = CreerClientAvecServicesMockes(paiementServiceMock, membreServiceMock);
        client.DefaultRequestHeaders.Add("X-Matricule", "G00003");

        var reponse = await client.PostAsync("/api/Paiements/50/remboursement", null);

        Assert.Equal(HttpStatusCode.NoContent, reponse.StatusCode);
        paiementServiceMock.Verify(s => s.RembourserAsync(50), Times.Once);
    }

    [Fact]
    public async Task PostPaiement_SansMatricule_Retourne401()
    {
        var client = CreerClientAvecServicesMockes();

        var reponse = await client.PostAsync("/api/Paiements/10", null);

        Assert.Equal(HttpStatusCode.Unauthorized, reponse.StatusCode);
    }
}