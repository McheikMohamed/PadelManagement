namespace Padel.Api.Middleware;

public class IdentiteAppelant
{
    public required string Matricule { get; set; }
    public required string TypeRole { get; set; } // "MembreGlobal", "MembreSite", "MembreLibre", "AdminGlobal", "AdminSite"
    public int? SiteId { get; set; }
}