using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Padel.Domain.Entities;

namespace Padel.Infrastructure.Configurations;

public class JourFermetureConfiguration : IEntityTypeConfiguration<JourFermeture>
{
    public void Configure(EntityTypeBuilder<JourFermeture> builder)
    {
        builder.ToTable("Jours_Fermeture", "sch_Padel");
        builder.HasKey(j => j.FermetureId);
        builder.Property(j => j.FermetureId).HasColumnName("Fermeture_ID");
        builder.Property(j => j.SiteId).HasColumnName("Site_ID");
        builder.Property(j => j.Date).HasColumnName("Date");
    }
}