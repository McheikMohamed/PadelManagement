using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Padel.Application.Interfaces;

namespace Padel.Infrastructure.Repositories;

public class PaiementRepository : IPaiementRepository
{
    private readonly PadelDbContext _context;

    public PaiementRepository(PadelDbContext context)
    {
        _context = context;
    }

    public async Task<int> TraiterPaiementAsync(int inscriptionId, decimal montant)
    {
        var resultat = await _context.Database
            .SqlQuery<int>(
                $"EXEC sch_Padel.SP_TraiterPaiement @InscriptionId = {inscriptionId}, @Montant = {montant}")
            .ToListAsync();

        return resultat.Single();
    }

    public async Task RembourserAsync(int paiementId)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sch_Padel.SP_RembourserPaiement @PaiementId = {paiementId}");
    }
}