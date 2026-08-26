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

public class MembreService : IMembreService
{
    private readonly IMembreRepository _membreRepository;

    public MembreService(IMembreRepository membreRepository)
    {
        _membreRepository = membreRepository;
    }

    public async Task CreerMembreAsync(CreerMembreDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Matricule))
        {
            throw new RegleMetierException("MATRICULE_OBLIGATOIRE", "Le matricule est obligatoire.");
        }

        if (!Enum.TryParse<TypeMembre>(dto.Type, ignoreCase: true, out var type))
        {
            throw new RegleMetierException(
                "TYPE_INVALIDE",
                $"Type de membre invalide : '{dto.Type}'. Valeurs autorisées : Global, Site, Libre.");
        }

        // CF-RS-013 / CF-RS-014 : cohérence type <-> site, revérifiée ici pour un message
        // d'erreur clair avant même d'aller en base (la contrainte CK_Membres_Type_SiteId
        // rejettera de toute façon l'insertion si ce contrôle était contourné).
        if (type == TypeMembre.Site && dto.SiteId is null)
        {
            throw new RegleMetierException(
                "SITE_OBLIGATOIRE",
                "Un membre de type Site doit être associé à un site.");
        }

        if (type != TypeMembre.Site && dto.SiteId is not null)
        {
            throw new RegleMetierException(
                "SITE_NON_APPLICABLE",
                "Un membre de type Global ou Libre ne peut pas être associé à un site.");
        }

        await _membreRepository.CreerAsync(dto.Matricule, dto.Type, dto.SiteId);
    }

    public async Task<MembreDto?> ObtenirMembreAsync(string matricule)
    {
        var membre = await _membreRepository.ObtenirParMatriculeAsync(matricule);

        if (membre is null)
        {
            return null;
        }

        return new MembreDto
        {
            Matricule = membre.Matricule,
            Type = membre.Type.ToString(),
            SiteId = membre.SiteId,
            SoldeDu = membre.SoldeDu,
            DateProchaineReservationAutorisee = membre.DateProchaineReservationAutorisee
        };
    }

    public async Task<List<MembreDto>> ListerMembresAsync(int? siteId)
    {
        // CF-AA-013 : le filtrage par site est transmis tel quel au Repository, qui le
        // relaiera à SP_ListerMembres (@SiteId). C'est au Controller (Issue #48) de
        // décider quelle valeur transmettre selon l'identité de l'appelant :
        // - Admin Global => siteId = null (voit tout)
        // - Admin Site => siteId = son propre SiteId (jamais un autre, jamais null)
        var membres = await _membreRepository.ListerAsync(siteId);

        return membres.Select(m => new MembreDto
        {
            Matricule = m.Matricule,
            Type = m.Type.ToString(),
            SiteId = m.SiteId,
            SoldeDu = m.SoldeDu,
            DateProchaineReservationAutorisee = m.DateProchaineReservationAutorisee
        }).ToList();
    }
}