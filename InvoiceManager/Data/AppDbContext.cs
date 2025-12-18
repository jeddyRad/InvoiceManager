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
        public DbSet<Facture> Invoices { get; set; }
        public DbSet<LigneFacture> Products { get; set; }
    }
}
