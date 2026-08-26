using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Padel.Application.Dtos;
using Padel.Application.Interfaces;

namespace Padel.Application.Services;

public class PaiementService : IPaiementService
{
    private const decimal MontantFixe = 15.00m;

    private readonly IPaiementRepository _paiementRepository;

    public PaiementService(IPaiementRepository paiementRepository)
    {
        _paiementRepository = paiementRepository;
    }

    public async Task<PaiementDto> TraiterPaiementAsync(int inscriptionId)
    {
        // CF-RV-011 : montant fixe, 60€ / 4 joueurs = 15€, aucune variation possible
        // puisqu'un match confirmé compte toujours exactement 4 joueurs (CF-RS-030).
        var paiementId = await _paiementRepository.TraiterPaiementAsync(inscriptionId, MontantFixe);

        return new PaiementDto
        {
            PaiementId = paiementId,
            InscriptionId = inscriptionId,
            Montant = MontantFixe,
            DateHeure = DateTime.Now,
            EstRembourse = false,
            DateRemboursement = null
        };
    }

    public async Task RembourserAsync(int paiementId)
    {
        // CF-RS-035 (pas de double remboursement) reste vérifié en SQL :
        // même raisonnement de concurrence que pour les autres contrôles d'état partagé.
        await _paiementRepository.RembourserAsync(paiementId);
    }
}