using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Padel.Application.Dtos;

public class TauxOccupationDto
{
    public int SiteId { get; set; }
    public string NomSite { get; set; } = string.Empty;
    public int CreneauxOccupes { get; set; }
}