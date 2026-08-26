using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Padel.Application.Dtos;

public class MatchDto
{
    public int MatchId { get; set; }
    public int TerrainId { get; set; }
    public string OrganisateurMatricule { get; set; } = string.Empty;
    public DateTime DateHeureDebut { get; set; }
    public DateTime DateHeureFin { get; set; }
    public string Statut { get; set; } = string.Empty;
    public decimal Prix { get; set; }
}