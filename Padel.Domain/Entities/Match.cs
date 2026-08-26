using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Padel.Domain.Enums;

namespace Padel.Domain.Entities;

public class Match
{
    public int MatchId { get; set; }
    public int TerrainId { get; set; }
    public string OrganisateurMatricule { get; set; } = string.Empty;
    public DateTime DateHeureDebut { get; set; }
    public DateTime DateHeureFin { get; set; }
    public StatutMatch Statut { get; set; }
    public decimal Prix { get; set; } = 60m;
}