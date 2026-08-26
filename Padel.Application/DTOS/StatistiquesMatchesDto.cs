using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Padel.Application.Dtos;

public class StatistiquesMatchesDto
{
    public int TotalMatches { get; set; }
    public int NombrePublics { get; set; }
    public int NombrePrivesOuConfirmes { get; set; }
    public int NombreAnnules { get; set; }
}