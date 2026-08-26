namespace Padel.UI.Membre.Models;

public class CreerReservationDto
{
    public int TerrainId { get; set; }
    public DateTime DateHeureDebut { get; set; }
    public bool EstPrive { get; set; } = true;
}