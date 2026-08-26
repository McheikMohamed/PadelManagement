using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Padel.Domain.Entities;

namespace Padel.Infrastructure.Configurations;

public class HoraireSiteConfiguration : IEntityTypeConfiguration<HoraireSite>
{
    public void Configure(EntityTypeBuilder<HoraireSite> builder)
    {
        builder.ToTable("Horaires_Site", "sch_Padel");
        builder.HasKey(h => h.HoraireId);
        builder.Property(h => h.HoraireId).HasColumnName("Horaire_ID");
        builder.Property(h => h.SiteId).HasColumnName("Site_ID");
        builder.Property(h => h.Annee).HasColumnName("Annee");
        builder.Property(h => h.HeureOuverture).HasColumnName("HeureOuverture");
        builder.Property(h => h.HeureFermeture).HasColumnName("HeureFermeture");
    }
}