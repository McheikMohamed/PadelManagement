using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Padel.Application.Dtos;

public class InscriptionDto
{
    public int InscriptionId { get; set; }
    public int MatchId { get; set; }
    public string MembreMatricule { get; set; } = string.Empty;
    public bool APaye { get; set; }
    public DateTime? DatePaiement { get; set; }
}