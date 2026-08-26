using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Padel.Domain.Entities;

namespace Padel.Infrastructure.Configurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("Matches", "sch_Padel");
        builder.HasKey(m => m.MatchId);
        builder.Property(m => m.MatchId).HasColumnName("Match_ID");
        builder.Property(m => m.TerrainId).HasColumnName("Terrain_ID");
        builder.Property(m => m.OrganisateurMatricule).HasColumnName("OrganisateurMatricule");
        builder.Property(m => m.DateHeureDebut).HasColumnName("DateHeureDebut");
        builder.Property(m => m.DateHeureFin).HasColumnName("DateHeureFin");
        builder.Property(m => m.Prix).HasColumnName("Prix");

        builder.Property(m => m.Statut)
            .HasColumnName("Statut")
            .HasConversion<string>();
    }
}