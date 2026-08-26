using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Padel.Application.Dtos;

public class MembreDto
{
    public string Matricule { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;   // string plutôt que l'enum : voir note plus bas
    public int? SiteId { get; set; }
    public decimal SoldeDu { get; set; }
    public DateOnly? DateProchaineReservationAutorisee { get; set; }
}