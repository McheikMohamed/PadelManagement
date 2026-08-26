namespace Padel.UI.Membre.Models;

public class MatchPublicDto
{
    public int MatchId { get; set; }
    public int TerrainId { get; set; }
    public string OrganisateurMatricule { get; set; } = string.Empty;
    public DateTime DateHeureDebut { get; set; }
    public DateTime DateHeureFin { get; set; }
    public string Statut { get; set; } = string.Empty;
    public decimal Prix { get; set; }
    public int NombreInscrits { get; set; }

    public string Affichage =>
        $"Match #{MatchId} — {DateHeureDebut:dd/MM/yyyy HH:mm} — {NombreInscrits}/4 joueurs";
}