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
            return await _context.Factures
                .AsNoTracking()
                .Include(f => f.Client)
                .OrderByDescending(f => f.DateFacture)
                .ToListAsync();
        }

        public async Task<Facture?> GetByIdAsync(int id)
        {
            return await _context.Factures
                .AsNoTracking()
                .Include(f => f.Client)
                .Include(f => f.Lignes)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task AddAsync(Facture facture)
        {
            _context.Factures.Add(facture);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Facture facture)
        {
            var existing = await _context.Factures
                .Include(f => f.Lignes)
                .FirstOrDefaultAsync(f => f.Id == facture.Id);
            if (existing == null)
                return;

            // update scalar properties
            existing.Numero = facture.Numero;
            existing.DateFacture = facture.DateFacture;
            existing.ClientId = facture.ClientId;
            existing.TotalHT = facture.TotalHT;
            existing.TotalTTC = facture.TotalTTC;

            // sync lines
            // remove deleted
            var incomingIds = facture.Lignes.Select(l => l.Id).ToHashSet();
            var toRemove = existing.Lignes.Where(l => l.Id != 0 && !incomingIds.Contains(l.Id)).ToList();
            foreach (var del in toRemove)
            {
                _context.Remove(del);
            }

            // add or update
            foreach (var line in facture.Lignes)
            {
                if (line.Id == 0)
                {
                    existing.Lignes.Add(new LigneFacture
                    {
                        Description = line.Description,
                        Quantite = line.Quantite,
                        PrixUnitaire = line.PrixUnitaire
                    });
                }
                else
                {
                    var target = existing.Lignes.FirstOrDefault(l => l.Id == line.Id);
                    if (target != null)
                    {
                        target.Description = line.Description;
                        target.Quantite = line.Quantite;
                        target.PrixUnitaire = line.PrixUnitaire;
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var facture = await _context.Factures.FindAsync(id);
            if (facture != null)
            {
                _context.Factures.Remove(facture);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RecalculerTotauxAsync(int factureId)
        {
            var facture = await _context.Factures
                .Include(f => f.Lignes)
                .FirstOrDefaultAsync(f => f.Id == factureId);
            if (facture == null)
                return;
            facture.TotalHT = facture.Lignes.Sum(l => l.TotalLigne);
            facture.TotalTTC = facture.TotalHT * 1.2m;

            await _context.SaveChangesAsync();
        }

    }
}
