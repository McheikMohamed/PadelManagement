using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Padel.Application.Dtos;
using Padel.Application.Exceptions;
using Padel.Application.Interfaces;
using Padel.Domain.Enums;
using Xunit;

namespace Padel.Tests.Controllers;

public class ReservationsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ReservationsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private HttpClient CreerClientAvecServicesMockes(
        Mock<IReservationService>? reservationServiceMock = null,
        Mock<IMembreService>? membreServiceMock = null,
        Mock<IAdministrateurRepository>? administrateurRepositoryMock = null)
    {
        var factoryConfiguree = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                if (reservationServiceMock is not null)
                {
                    services.RemoveAll<IReservationService>();
                    services.AddScoped(_ => reservationServiceMock.Object);
                }

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

    // Rappel : le middleware cherche côté Membre (via IMembreService) si le matricule
    // ne commence pas par 'A'. On mocke IMembreService (pas IMembreRepository) pour que
    // le middleware, qui dépend du Service, trouve bien l'identité de l'appelant.
    private Mock<IMembreService> CreerMembreServiceMockAvecUnMembre(string matricule, TypeMembre type, int? siteId = null)
    {
        var mock = new Mock<IMembreService>();
        mock.Setup(s => s.ObtenirMembreAsync(matricule))
            .ReturnsAsync(new MembreDto { Matricule = matricule, Type = type.ToString(), SiteId = siteId, SoldeDu = 0 });
        return mock;
    }

    [Fact]
    public async Task GetCreneauxDisponibles_SansMatricule_Retourne401()
    {
        var client = CreerClientAvecServicesMockes();

        var reponse = await client.GetAsync("/api/Reservations/creneaux-disponibles?siteId=1&date=2026-10-15");

        Assert.Equal(HttpStatusCode.Unauthorized, reponse.StatusCode);
    }

    [Fact]
    public async Task GetCreneauxDisponibles_AvecMembreValide_Retourne200()
    {
        var membreServiceMock = CreerMembreServiceMockAvecUnMembre("G00001", TypeMembre.Global);

        var reservationServiceMock = new Mock<IReservationService>();
        reservationServiceMock
            .Setup(s => s.ListerCreneauxDisponiblesAsync(1, new DateOnly(2026, 10, 15), null))
            .ReturnsAsync(new List<CreneauDisponibleDto>
            {
                new() { TerrainId = 1, DateHeureDebut = new DateTime(2026, 10, 15, 8, 0, 0), DateHeureFin = new DateTime(2026, 10, 15, 9, 45, 0) }
            });

        var client = CreerClientAvecServicesMockes(reservationServiceMock, membreServiceMock);
        client.DefaultRequestHeaders.Add("X-Matricule", "G00001");

        var reponse = await client.GetAsync("/api/Reservations/creneaux-disponibles?siteId=1&date=2026-10-15");

        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
        var creneaux = await reponse.Content.ReadFromJsonAsync<List<CreneauDisponibleDto>>();
        Assert.Single(creneaux!);
    }

    [Fact]
    public async Task PostReservation_CasValide_Retourne201()
    {
        var membreServiceMock = CreerMembreServiceMockAvecUnMembre("L00001", TypeMembre.Libre);

        var reservationServiceMock = new Mock<IReservationService>();
        reservationServiceMock
            .Setup(s => s.CreerReservationAsync(It.IsAny<CreerReservationDto>(), "L00001"))
            .ReturnsAsync(new MatchDto
            {
                MatchId = 5,
                TerrainId = 1,
                OrganisateurMatricule = "L00001",
                DateHeureDebut = new DateTime(2026, 9, 1, 10, 0, 0),
                DateHeureFin = new DateTime(2026, 9, 1, 11, 45, 0),
                Statut = "Prive",
                Prix = 60m
            });

        var client = CreerClientAvecServicesMockes(reservationServiceMock, membreServiceMock);
        client.DefaultRequestHeaders.Add("X-Matricule", "L00001");

        var dto = new CreerReservationDto { TerrainId = 1, DateHeureDebut = new DateTime(2026, 9, 1, 10, 0, 0), EstPrive = true };
        var reponse = await client.PostAsJsonAsync("/api/Reservations", dto);

        Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);
        var match = await reponse.Content.ReadFromJsonAsync<MatchDto>();
        Assert.Equal(5, match!.MatchId);
    }

    [Fact]
    public async Task PostReservation_SoldeDu_Retourne403AvecCode()
    {
        var membreServiceMock = CreerMembreServiceMockAvecUnMembre("G00002", TypeMembre.Global);

        var reservationServiceMock = new Mock<IReservationService>();
        reservationServiceMock
            .Setup(s => s.CreerReservationAsync(It.IsAny<CreerReservationDto>(), "G00002"))
            .ThrowsAsync(new RegleMetierException("SOLDE_DU", "Solde dû non régularisé : réservation impossible."));

        var client = CreerClientAvecServicesMockes(reservationServiceMock, membreServiceMock);
        client.DefaultRequestHeaders.Add("X-Matricule", "G00002");

        var dto = new CreerReservationDto { TerrainId = 1, DateHeureDebut = DateTime.Now.AddDays(1) };
        var reponse = await client.PostAsJsonAsync("/api/Reservations", dto);

        Assert.Equal(HttpStatusCode.Forbidden, reponse.StatusCode);
        var corps = await reponse.Content.ReadAsStringAsync();
        Assert.Contains("SOLDE_DU", corps);
    }

    [Fact]
    public async Task PostReservation_DelaiDepasse_Retourne400AvecCode()
    {
        var membreServiceMock = CreerMembreServiceMockAvecUnMembre("L00002", TypeMembre.Libre);

        var reservationServiceMock = new Mock<IReservationService>();
        reservationServiceMock
            .Setup(s => s.CreerReservationAsync(It.IsAny<CreerReservationDto>(), "L00002"))
            .ThrowsAsync(new RegleMetierException("DELAI_DEPASSE", "Délai de réservation dépassé pour ce type de membre."));

        var client = CreerClientAvecServicesMockes(reservationServiceMock, membreServiceMock);
        client.DefaultRequestHeaders.Add("X-Matricule", "L00002");

        var dto = new CreerReservationDto { TerrainId = 1, DateHeureDebut = DateTime.Now.AddDays(20) };
        var reponse = await client.PostAsJsonAsync("/api/Reservations", dto);

        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
        var corps = await reponse.Content.ReadAsStringAsync();
        Assert.Contains("DELAI_DEPASSE", corps);
    }

    [Fact]
    public async Task PostReservation_PerimetreSite_Retourne403AvecCode()
    {
        var membreServiceMock = CreerMembreServiceMockAvecUnMembre("S00001", TypeMembre.Site, siteId: 1);

        var reservationServiceMock = new Mock<IReservationService>();
        reservationServiceMock
            .Setup(s => s.CreerReservationAsync(It.IsAny<CreerReservationDto>(), "S00001"))
            .ThrowsAsync(new RegleMetierException("PERIMETRE_SITE", "Un membre Site ne peut réserver que sur son propre site."));

        var client = CreerClientAvecServicesMockes(reservationServiceMock, membreServiceMock);
        client.DefaultRequestHeaders.Add("X-Matricule", "S00001");

        var dto = new CreerReservationDto { TerrainId = 99, DateHeureDebut = DateTime.Now.AddDays(1) };
        var reponse = await client.PostAsJsonAsync("/api/Reservations", dto);

        Assert.Equal(HttpStatusCode.Forbidden, reponse.StatusCode);
        var corps = await reponse.Content.ReadAsStringAsync();
        Assert.Contains("PERIMETRE_SITE", corps);
    }

    [Fact]
    public async Task PostInscription_CasValide_Retourne200()
    {
        var membreServiceMock = CreerMembreServiceMockAvecUnMembre("G00003", TypeMembre.Global);

        var reservationServiceMock = new Mock<IReservationService>();
        reservationServiceMock
            .Setup(s => s.InscrireJoueurAsync(
                It.Is<InscrireJoueurDto>(d => d.MatchId == 7 && d.MembreMatricule == "L00003"),
                "G00003"))
            .ReturnsAsync(new InscriptionDto
            {
                InscriptionId = 20,
                MatchId = 7,
                MembreMatricule = "L00003",
                APaye = false
            });

        var client = CreerClientAvecServicesMockes(reservationServiceMock, membreServiceMock);
        client.DefaultRequestHeaders.Add("X-Matricule", "G00003");

        var requeteDto = new InscrireJoueurRequestDto { MembreMatricule = "L00003" };
        var reponse = await client.PostAsJsonAsync("/api/Reservations/7/inscriptions", requeteDto);

        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
        var inscription = await reponse.Content.ReadFromJsonAsync<InscriptionDto>();
        Assert.Equal(20, inscription!.InscriptionId);
    }

    [Fact]
    public async Task PostInscription_NonAutorisee_Retourne403AvecCode()
    {
        var membreServiceMock = CreerMembreServiceMockAvecUnMembre("G00004", TypeMembre.Global);

        var reservationServiceMock = new Mock<IReservationService>();
        reservationServiceMock
            .Setup(s => s.InscrireJoueurAsync(It.IsAny<InscrireJoueurDto>(), "G00004"))
            .ThrowsAsync(new RegleMetierException(
                "INSCRIPTION_NON_AUTORISEE", "Seul le joueur concerné peut s'inscrire à un match public."));

        var client = CreerClientAvecServicesMockes(reservationServiceMock, membreServiceMock);
        client.DefaultRequestHeaders.Add("X-Matricule", "G00004");

        var requeteDto = new InscrireJoueurRequestDto { MembreMatricule = "L00004" };
        var reponse = await client.PostAsJsonAsync("/api/Reservations/8/inscriptions", requeteDto);

        Assert.Equal(HttpStatusCode.Forbidden, reponse.StatusCode);
        var corps = await reponse.Content.ReadAsStringAsync();
        Assert.Contains("INSCRIPTION_NON_AUTORISEE", corps);
    }
}