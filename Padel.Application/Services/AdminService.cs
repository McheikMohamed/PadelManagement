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

public class AdminService : IAdminService
{
    private readonly IAdministrateurRepository _administrateurRepository;

    public AdminService(IAdministrateurRepository administrateurRepository)
    {
        _administrateurRepository = administrateurRepository;
    }

    public async Task CreerAdministrateurAsync(CreerAdministrateurDto dto, string appelantMatricule)
    {
        // CF-AA-016 / CF-RV-022 : seul un administrateur Global peut créer/modifier un administrateur.
        var appelant = await _administrateurRepository.ObtenirParMatriculeAsync(appelantMatricule);

        if (appelant is null)
        {
            throw new RegleMetierException("APPELANT_INCONNU", "Administrateur appelant inconnu.");
        }

        if (appelant.Type != TypeAdmin.Global)
        {
            throw new RegleMetierException(
                "ACTION_RESERVEE_GLOBAL",
                "Seul un administrateur global peut créer un administrateur.");
        }

        if (string.IsNullOrWhiteSpace(dto.Matricule))
        {
            throw new RegleMetierException("MATRICULE_OBLIGATOIRE", "Le matricule est obligatoire.");
        }

        if (!Enum.TryParse<TypeAdmin>(dto.Type, ignoreCase: true, out var type))
        {
            throw new RegleMetierException(
                "TYPE_INVALIDE",
                $"Type d'administrateur invalide : '{dto.Type}'. Valeurs autorisées : Global, Site.");
        }

        // CF-RS-018 / CF-RS-019 : cohérence type <-> site
        if (type == TypeAdmin.Site && dto.SiteId is null)
        {
            throw new RegleMetierException(
                "SITE_OBLIGATOIRE",
                "Un administrateur de type Site doit être associé à un site.");
        }

        if (type == TypeAdmin.Global && dto.SiteId is not null)
        {
            throw new RegleMetierException(
                "SITE_NON_APPLICABLE",
                "Un administrateur de type Global ne peut pas être associé à un site.");
        }

        await _administrateurRepository.CreerAsync(dto.Matricule, dto.Type, dto.SiteId);
    }

    public async Task<AdministrateurDto?> ObtenirAdministrateurAsync(string matricule)
    {
        var admin = await _administrateurRepository.ObtenirParMatriculeAsync(matricule);

        if (admin is null)
        {
            return null;
        }

        return new AdministrateurDto
        {
            Matricule = admin.Matricule,
            Type = admin.Type.ToString(),
            SiteId = admin.SiteId
        };
    }
}