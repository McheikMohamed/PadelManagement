using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Padel.Application.Dtos;

public class CreneauDisponibleDto
{
    public int TerrainId { get; set; }
    public DateTime DateHeureDebut { get; set; }
    public DateTime DateHeureFin { get; set; }
}