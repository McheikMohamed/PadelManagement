using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Padel.Application.Interfaces;

public interface IPaiementRepository
{
    Task<int> TraiterPaiementAsync(int inscriptionId, decimal montant);
    Task RembourserAsync(int paiementId);
}