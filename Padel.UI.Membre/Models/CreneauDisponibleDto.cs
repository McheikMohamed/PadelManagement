namespace Padel.UI.Membre.Models;

public class CreneauDisponibleDto
{
    public int TerrainId { get; set; }
    public DateTime DateHeureDebut { get; set; }
    public DateTime DateHeureFin { get; set; }

    // Propriété calculée pour un affichage lisible dans la ListBox
    public string Affichage => $"Terrain {TerrainId} — {DateHeureDebut:dd/MM/yyyy HH:mm}";
}