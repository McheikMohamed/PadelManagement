using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Padel.Domain.Entities;

namespace Padel.Infrastructure.Configurations;

public class MembreConfiguration : IEntityTypeConfiguration<Membre>
{
    public void Configure(EntityTypeBuilder<Membre> builder)
    {
        builder.ToTable("Membres", "sch_Padel");
        builder.HasKey(m => m.Matricule);
        builder.Property(m => m.Matricule).HasColumnName("Matricule");
        builder.Property(m => m.SiteId).HasColumnName("Site_ID");
        builder.Property(m => m.SoldeDu).HasColumnName("SoldeDu");
        builder.Property(m => m.DateProchaineReservationAutorisee).HasColumnName("DateProchaineReservationAutorisee");

        // Conversion enum C# <-> string SQL (colonne NVARCHAR en base)
        builder.Property(m => m.Type)
            .HasColumnName("Type")
            .HasConversion<string>();
    }
}