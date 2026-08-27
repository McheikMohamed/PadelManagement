namespace Padel.UI.Membre.Models;

public class MaReservationDto
{
    public int MatchId { get; set; }
    public int TerrainId { get; set; }
    public string OrganisateurMatricule { get; set; } = string.Empty;
    public DateTime DateHeureDebut { get; set; }
    public DateTime DateHeureFin { get; set; }
    public string Statut { get; set; } = string.Empty;
    public decimal Prix { get; set; }
    public int InscriptionId { get; set; }
    public bool APaye { get; set; }
    public DateTime? DatePaiement { get; set; }
}