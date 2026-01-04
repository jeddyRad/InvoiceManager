using InvoiceManager.Data.Entities;
using InvoiceManager.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.Data.Repositories
{
    /// <summary>
    /// Implémentation du repository Facture
    /// </summary>
    public class FactureRepository : Repository<Facture>, IFactureRepository
    {
        public FactureRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<Facture>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Include(f => f.Client)
                .OrderByDescending(f => f.DateFacture)
                .ToListAsync();
        }

        public async Task<Facture?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(f => f.Client)
                .Include(f => f.Lignes)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<List<Facture>> GetByClientIdAsync(int clientId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(f => f.Lignes)
                .Where(f => f.ClientId == clientId)
                .OrderByDescending(f => f.DateFacture)
                .ToListAsync();
        }

        public async Task<List<Facture>> GetByStatutAsync(FactureStatut statut)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(f => f.Client)
                .Where(f => f.Statut == statut)
                .OrderByDescending(f => f.DateFacture)
                .ToListAsync();
        }

        public async Task<Facture?> GetByNumeroAsync(string numero)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Numero == numero);
        }

        public async Task<int> GetNextFactureNumberAsync()
        {
            return await _dbSet.CountAsync() + 1;
        }

        public void DetachTrackedEntities(int factureId)
        {
            // Détacher la facture si elle est suivie
            var trackedFacture = _context.ChangeTracker.Entries<Facture>()
                .FirstOrDefault(e => e.Entity.Id == factureId);
            
            if (trackedFacture != null)
            {
                trackedFacture.State = EntityState.Detached;
            }

            // Détacher toutes les lignes de cette facture
            var trackedLignes = _context.ChangeTracker.Entries<LigneFacture>()
                .Where(e => e.Entity.FactureId == factureId)
                .ToList();
            
            foreach (var trackedLigne in trackedLignes)
            {
                trackedLigne.State = EntityState.Detached;
            }
        }
    }
}
