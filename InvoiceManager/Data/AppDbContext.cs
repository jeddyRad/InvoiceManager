using InvoiceManager.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Ajoutez vos DbSet ici, par exemple :
        // public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Facture> Factures { get; set; }
        public DbSet<LigneFacture> LigneFactures { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships for SQLite
            modelBuilder.Entity<Facture>(entity =>
            {
                // ❌ RETIREZ CES LIGNES - incompatibles avec SQLite
                // entity.Property(f => f.TotalHT).HasPrecision(18, 2);
                // entity.Property(f => f.TotalTTC).HasPrecision(18, 2);

                // ✅ SQLite gère les decimal automatiquement
                entity.Property(f => f.TotalHT).HasColumnType("TEXT");
                entity.Property(f => f.TotalTTC).HasColumnType("TEXT");

                entity.HasOne(f => f.Client)
                    .WithMany(c => c.Factures)
                    .HasForeignKey(f => f.ClientId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<LigneFacture>(entity =>
            {
                // ❌ RETIREZ cette ligne aussi
                // entity.Property(l => l.PrixUnitaire).HasPrecision(18, 2);
                
                // ✅ SQLite utilise TEXT pour les decimals
                entity.Property(l => l.PrixUnitaire).HasColumnType("TEXT");
                
                entity.Ignore(l => l.TotalLigne);

                entity.HasOne(l => l.Facture)
                    .WithMany(f => f.Lignes)
                    .HasForeignKey(l => l.FactureId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Facture>().ToTable("Factures");
            modelBuilder.Entity<LigneFacture>().ToTable("LigneFactures");
        }
    }
}
