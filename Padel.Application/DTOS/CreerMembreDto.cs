using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Padel.Application.Dtos;

public class CreerMembreDto
{
    public string Matricule { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int? SiteId { get; set; }
}