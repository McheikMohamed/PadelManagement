using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Padel.Application.Dtos;

public class CreerReservationDto
{
    public int TerrainId { get; set; }
    public DateTime DateHeureDebut { get; set; }
    public bool EstPrive { get; set; } = true;
}