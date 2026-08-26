using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Padel.Application.Dtos;

namespace Padel.Application.Interfaces;

public interface IPaiementService
{
    Task<PaiementDto> TraiterPaiementAsync(int inscriptionId, string appelantMatricule);
    Task RembourserAsync(int paiementId);
}