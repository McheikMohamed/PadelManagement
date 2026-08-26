namespace Padel.UI.Membre.Models;

public class MembreDto
{
    public string Matricule { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int? SiteId { get; set; }
    public decimal SoldeDu { get; set; }
    public DateOnly? DateProchaineReservationAutorisee { get; set; }
}