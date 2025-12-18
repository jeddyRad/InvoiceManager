using InvoiceManager.Data;
using InvoiceManager.Data.Entities;
using InvoiceManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace InvoiceManager.Services
{
    public class FactureService : IFactureService
    {
        private readonly AppDbContext _context;

        public FactureService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Facture>> GetAllAsync()
        {
            return await _context.Invoices
                .Include(f => f.Client)
                .OrderByDescending(f => f.DateFacture)
                .ToListAsync();
        }

        public async Task<Facture?> GetByIdAsync(int id)
        {
            return await _context.Invoices
                .Include(f => f.Client)
                .Include(f => f.Lignes)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task AddAsync(Facture facture)
        {
            _context.Invoices.Add(facture);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Facture facture)
        {
            _context.Invoices.Update(facture);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var facture = await GetByIdAsync(id);
            if (facture != null)
            {
                _context.Invoices.Remove(facture);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RecalculerTotauxAsync(int factureId)
        {
            var facture = await GetByIdAsync(factureId);
            if (facture == null)
                return;
            facture.TotalHT = facture.Lignes.Sum(l => l.TotalLigne);
            facture.TotalTTC = facture.TotalHT * 1.2m;

            await _context.SaveChangesAsync();
        }

    }
}
