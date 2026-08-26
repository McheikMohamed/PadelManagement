-- Correctif : SP_ListerMatchsPublics ne renvoyait pas les colonnes MatchId / TerrainId
-- attendues par Padel.Application.Dtos.MatchPublicDto (mappage EF Core par nom exact
-- de propriété via _context.Database.SqlQuery<MatchPublicDto>(...), qui n'est pas
-- configuré via Fluent API et n'a donc aucun renommage de colonne implicite).
-- Seuls deux alias ont été ajoutés (Match_ID -> MatchId, Terrain_ID -> TerrainId) ;
-- aucune autre modification de logique.

USE PadelDB;
GO

ALTER PROCEDURE sch_Padel.SP_ListerMatchsPublics
    @SiteId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT m.Match_ID AS MatchId, m.Terrain_ID AS TerrainId, m.OrganisateurMatricule, m.DateHeureDebut, m.DateHeureFin, m.Statut, m.Prix,
        (SELECT COUNT(*) FROM sch_Padel.Inscriptions_Match i WHERE i.Match_ID = m.Match_ID) AS NombreInscrits
    FROM sch_Padel.Matches m
    JOIN sch_Padel.Terrains t ON t.Terrain_ID = m.Terrain_ID
    WHERE m.Statut = 'Public'
        AND (@SiteId IS NULL OR t.Site_ID = @SiteId)
    ORDER BY m.DateHeureDebut;
END;
GO
