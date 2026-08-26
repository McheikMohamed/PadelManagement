using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Padel.Application.Dtos;

public class InscrireJoueurDto
{
    public int MatchId { get; set; }
    public string MembreMatricule { get; set; } = string.Empty;
}