using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Padel.Application.Dtos;

public class PaiementDto
{
    public int PaiementId { get; set; }
    public int InscriptionId { get; set; }
    public decimal Montant { get; set; }
    public DateTime DateHeure { get; set; }
    public bool EstRembourse { get; set; }
    public DateTime? DateRemboursement { get; set; }
}