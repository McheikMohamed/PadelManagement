using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Padel.Application.Dtos;
using Padel.Application.Exceptions;
using Padel.Application.Interfaces;
using Padel.Domain.Enums;

namespace Padel.Application.Services;

public class ReservationService : IReservationService
{
    private readonly IMatchRepository _matchRepository;
    private readonly IMembreRepository _membreRepository;
    private readonly ITerrainRepository _terrainRepository;

    public ReservationService(
        IMatchRepository matchRepository,
        IMembreRepository membreRepository,
        ITerrainRepository terrainRepository)
    {
        _matchRepository = matchRepository;
        _membreRepository = membreRepository;
        _terrainRepository = terrainRepository;
    }

    public async Task<List<CreneauDisponibleDto>> ListerCreneauxDisponiblesAsync(
        int siteId, DateOnly date, int? terrainId)
    {
        return await _matchRepository.ListerCreneauxDisponiblesAsync(siteId, date, terrainId);
    }

    public async Task<MatchDto> CreerReservationAsync(CreerReservationDto dto, string organisateurMatricule)
    {
        // CF-RV-005 / CF-RV-006 / CF-RV-001 / CF-RV-002 : vérifications déterministes,
        // sans risque de concurrence, faites ici en C# avant d'appeler le Repository.

        var membre = await _membreRepository.ObtenirParMatriculeAsync(organisateurMatricule);
        if (membre is null)
        {
            throw new RegleMetierException("MEMBRE_INCONNU", "Membre inconnu.");
        }

        // CF-RV-005 : solde dû bloque toute nouvelle réservation
        if (membre.SoldeDu > 0)
        {
            throw new RegleMetierException("SOLDE_DU", "Solde dû non régularisé : réservation impossible.");
        }

        // CF-RV-006 : pénalité de délai en cours
        var aujourdHui = DateOnly.FromDateTime(DateTime.Now);
        if (membre.DateProchaineReservationAutorisee.HasValue
            && membre.DateProchaineReservationAutorisee.Value > aujourdHui)
        {
            throw new RegleMetierException(
                "PENALITE_ACTIVE",
                $"Pénalité en cours : réservation possible seulement à partir du {membre.DateProchaineReservationAutorisee.Value:yyyy-MM-dd}.");
        }

        // CF-RV-001 : délai autorisé selon le type de membre
        var joursDelai = membre.Type switch
        {
            TypeMembre.Global => 21,
            TypeMembre.Site => 14,
            TypeMembre.Libre => 5,
            _ => throw new RegleMetierException("TYPE_INCONNU", "Type de membre non reconnu.")
        };

        if (dto.DateHeureDebut > DateTime.Now.AddDays(joursDelai))
        {
            throw new RegleMetierException(
                "DELAI_DEPASSE",
                "Délai de réservation dépassé pour ce type de membre.");
        }

        // CF-RV-002 : périmètre site pour un membre de type Site
        var terrain = await _terrainRepository.ObtenirParIdAsync(dto.TerrainId);
        if (terrain is null)
        {
            throw new RegleMetierException("TERRAIN_INCONNU", "Terrain inconnu.");
        }

        if (membre.Type == TypeMembre.Site && terrain.SiteId != membre.SiteId)
        {
            throw new RegleMetierException(
                "PERIMETRE_SITE",
                "Un membre Site ne peut réserver que sur son propre site.");
        }

        // CF-RV-003 (disponibilité du créneau) et CF-RS-026 (unicité terrain+créneau)
        // sont volontairement laissées à la procédure stockée : ce sont des contrôles
        // sensibles à la concurrence (deux réservations simultanées sur le même créneau),
        // qui doivent rester atomiques en base plutôt que dupliqués ici en "check-then-act".
        var matchId = await _matchRepository.CreerReservationAsync(
            dto.TerrainId, dto.DateHeureDebut, organisateurMatricule, dto.EstPrive);

        return new MatchDto
        {
            MatchId = matchId,
            TerrainId = dto.TerrainId,
            OrganisateurMatricule = organisateurMatricule,
            DateHeureDebut = dto.DateHeureDebut,
            DateHeureFin = dto.DateHeureDebut.AddMinutes(105),
            Statut = dto.EstPrive ? "Prive" : "Public",
            Prix = 60m
        };
    }

    public async Task<InscriptionDto> InscrireJoueurAsync(InscrireJoueurDto dto, string appelantMatricule)
    {
        // CF-RV-007 (max 4) et CF-RV-008 (unicité inscription) restent volontairement
        // en SQL uniquement : contrôles sensibles à la concurrence.

        // CF-RV-010 : vérifiée ici en C# en plus du contrôle SQL (défense en profondeur,
        // CF-AA-017) — aucun risque de concurrence sur cette règle précise, donc autant
        // échouer vite plutôt que d'attendre un aller-retour base de données.
        var match = await _matchRepository.ObtenirParIdAsync(dto.MatchId);
        if (match is null)
        {
            throw new RegleMetierException("MATCH_INCONNU", "Match inconnu.");
        }

        if (match.Statut == StatutMatch.Public && appelantMatricule != dto.MembreMatricule)
        {
            throw new RegleMetierException(
                "INSCRIPTION_NON_AUTORISEE",
                "Seul le joueur concerné peut s'inscrire à un match public.");
        }

        var inscriptionId = await _matchRepository.InscrireJoueurAsync(
            dto.MatchId, dto.MembreMatricule, appelantMatricule);

        return new InscriptionDto
        {
            InscriptionId = inscriptionId,
            MatchId = dto.MatchId,
            MembreMatricule = dto.MembreMatricule,
            APaye = false,
            DatePaiement = null
        };
    }
    public async Task AnnulerReservationAsync(int matchId, string appelantMatricule)
    {
        var match = await _matchRepository.ObtenirParIdAsync(matchId);
        if (match is null)
        {
            throw new RegleMetierException("MATCH_INCONNU", "Match inconnu.");
        }

        if (match.Statut == StatutMatch.Annule)
        {
            throw new RegleMetierException("MATCH_DEJA_ANNULE", "Ce match est déjà annulé.");
        }

        if (match.OrganisateurMatricule != appelantMatricule)
        {
            throw new RegleMetierException(
                "ACTION_RESERVEE_ORGANISATEUR", "Seul l'organisateur peut annuler ce match.");
        }

        await _matchRepository.AnnulerMatchAsync(matchId, appelantMatricule);
    }

    public async Task DesinscrireJoueurAsync(int matchId, string membreMatricule, string appelantMatricule)
    {
        var match = await _matchRepository.ObtenirParIdAsync(matchId);
        if (match is null)
        {
            throw new RegleMetierException("MATCH_INCONNU", "Match inconnu.");
        }

        if (membreMatricule == match.OrganisateurMatricule)
        {
            throw new RegleMetierException(
                "ORGANISATEUR_DOIT_ANNULER", "L'organisateur doit annuler le match entier.");
        }

        if (appelantMatricule != membreMatricule)
        {
            throw new RegleMetierException(
                "DESINSCRIPTION_NON_AUTORISEE", "Seul le joueur concerné peut se désinscrire.");
        }

        await _matchRepository.DesinscrireJoueurAsync(matchId, membreMatricule, appelantMatricule);
    }
    public async Task<List<MatchPublicDto>> ListerMatchsPublicsAsync(int? siteId)
    {
        return await _matchRepository.ListerMatchsPublicsAsync(siteId);
    }
    public async Task<List<MaReservationDto>> ListerMesReservationsAsync(string matricule)
    {
        return await _matchRepository.ListerMesReservationsAsync(matricule);
    }
}