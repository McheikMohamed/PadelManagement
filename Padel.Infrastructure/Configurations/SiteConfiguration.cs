using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Padel.Domain.Entities;

namespace Padel.Infrastructure.Configurations;

public class SiteConfiguration : IEntityTypeConfiguration<Site>
{
    public void Configure(EntityTypeBuilder<Site> builder)
    {
        builder.ToTable("Sites", "sch_Padel");
        builder.HasKey(s => s.SiteId);
        builder.Property(s => s.SiteId).HasColumnName("Site_ID");
        builder.Property(s => s.Nom).HasColumnName("Nom");
    }
}