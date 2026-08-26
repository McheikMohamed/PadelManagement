using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Padel.Domain.Entities;

namespace Padel.Infrastructure.Configurations;

public class InscriptionMatchConfiguration : IEntityTypeConfiguration<InscriptionMatch>
{
    public void Configure(EntityTypeBuilder<InscriptionMatch> builder)
    {
        builder.ToTable("Inscriptions_Match", "sch_Padel");
        builder.HasKey(i => i.InscriptionId);
        builder.Property(i => i.InscriptionId).HasColumnName("Inscription_ID");
        builder.Property(i => i.MatchId).HasColumnName("Match_ID");
        builder.Property(i => i.MembreMatricule).HasColumnName("MembreMatricule");
        builder.Property(i => i.APaye).HasColumnName("APaye");
        builder.Property(i => i.DatePaiement).HasColumnName("DatePaiement");
    }
}