using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Padel.Domain.Entities;

namespace Padel.Infrastructure.Configurations;

public class TerrainConfiguration : IEntityTypeConfiguration<Terrain>
{
    public void Configure(EntityTypeBuilder<Terrain> builder)
    {
        builder.ToTable("Terrains", "sch_Padel");
        builder.HasKey(t => t.TerrainId);
        builder.Property(t => t.TerrainId).HasColumnName("Terrain_ID");
        builder.Property(t => t.SiteId).HasColumnName("Site_ID");
        builder.Property(t => t.Numero).HasColumnName("Numero");
    }
}