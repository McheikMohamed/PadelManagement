using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Padel.Domain.Entities;

namespace Padel.Infrastructure.Configurations;

public class PaiementConfiguration : IEntityTypeConfiguration<Paiement>
{
    public void Configure(EntityTypeBuilder<Paiement> builder)
    {
        builder.ToTable("Paiements", "sch_Padel");
        builder.HasKey(p => p.PaiementId);
        builder.Property(p => p.PaiementId).HasColumnName("Paiement_ID");
        builder.Property(p => p.InscriptionId).HasColumnName("Inscription_ID");
        builder.Property(p => p.Montant).HasColumnName("Montant");
        builder.Property(p => p.DateHeure).HasColumnName("DateHeure");
        builder.Property(p => p.EstRembourse).HasColumnName("EstRembourse");
        builder.Property(p => p.DateRemboursement).HasColumnName("DateRemboursement");
    }
}