using Padel.Application.Dtos;
using Padel.Application.Exceptions;
using Padel.Application.Interfaces;

namespace Padel.Application.Services;

public class PaiementService : IPaiementService
{
    private const decimal MontantFixe = 15.00m;

    private readonly IPaiementRepository _paiementRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly IAdministrateurRepository _administrateurRepository;

    public PaiementService(
        IPaiementRepository paiementRepository,
        IMatchRepository matchRepository,
        IAdministrateurRepository administrateurRepository)
    {
        _paiementRepository = paiementRepository;
        _matchRepository = matchRepository;
        _administrateurRepository = administrateurRepository;
    }

    public async Task<PaiementDto> TraiterPaiementAsync(int inscriptionId, string appelantMatricule)
    {
        // Seul le membre concerné par l'inscription, ou un administrateur (CF-ADM-005,
        // gestion manuelle en cas de litige), peut déclencher ce paiement.
        var membreConcerne = await _matchRepository.ObtenirMembreParInscriptionAsync(inscriptionId);

        if (membreConcerne is null)
        {
            throw new RegleMetierException("INSCRIPTION_INCONNUE", "Inscription inconnue.");
        }

        if (membreConcerne != appelantMatricule)
        {
            var estAdmin = await _administrateurRepository.ObtenirParMatriculeAsync(appelantMatricule) is not null;

            if (!estAdmin)
            {
                throw new RegleMetierException(
                    "PAIEMENT_NON_AUTORISE",
                    "Seul le membre concerné par cette inscription peut la payer.");
            }
        }

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
        await _paiementRepository.RembourserAsync(paiementId);
    }
}