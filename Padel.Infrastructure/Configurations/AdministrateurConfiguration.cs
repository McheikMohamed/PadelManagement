using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Padel.Domain.Entities;

namespace Padel.Infrastructure.Configurations;

public class AdministrateurConfiguration : IEntityTypeConfiguration<Administrateur>
{
    public void Configure(EntityTypeBuilder<Administrateur> builder)
    {
        builder.ToTable("Administrateurs", "sch_Padel");
        builder.HasKey(a => a.Matricule);
        builder.Property(a => a.Matricule).HasColumnName("Matricule");
        builder.Property(a => a.SiteId).HasColumnName("Site_ID");

        builder.Property(a => a.Type)
            .HasColumnName("Type")
            .HasConversion<string>();
    }
}