namespace Padel.UI.Membre.Models;

public class InscriptionDto
{
    public int InscriptionId { get; set; }
    public int MatchId { get; set; }
    public string MembreMatricule { get; set; } = string.Empty;
    public bool APaye { get; set; }
    public DateTime? DatePaiement { get; set; }
}