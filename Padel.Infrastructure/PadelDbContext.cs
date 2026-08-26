using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Padel.Domain.Entities;

namespace Padel.Infrastructure;

public class PadelDbContext : DbContext
{
    public PadelDbContext(DbContextOptions<PadelDbContext> options) : base(options)
    {
    }

    public DbSet<Site> Sites => Set<Site>();
    public DbSet<HoraireSite> HorairesSite => Set<HoraireSite>();
    public DbSet<Terrain> Terrains => Set<Terrain>();
    public DbSet<JourFermeture> JoursFermeture => Set<JourFermeture>();
    public DbSet<Membre> Membres => Set<Membre>();
    public DbSet<Administrateur> Administrateurs => Set<Administrateur>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<InscriptionMatch> InscriptionsMatch => Set<InscriptionMatch>();
    public DbSet<Paiement> Paiements => Set<Paiement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PadelDbContext).Assembly);
    }
}