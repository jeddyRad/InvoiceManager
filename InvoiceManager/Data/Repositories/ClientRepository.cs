using InvoiceManager.Data.Entities;
using InvoiceManager.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.Data.Repositories
{
    /// <summary>
    /// Implémentation du repository Client
    /// </summary>
    public class ClientRepository : Repository<Client>, IClientRepository
    {
        public ClientRepository(AppDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Client>> GetAllAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .OrderBy(c => c.Nom)
                .ToListAsync();
        }

        public async Task<bool> EmailExistsAsync(string email, int excludeClientId = 0)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return await _dbSet
                .AsNoTracking()
                .AnyAsync(c => c.Email == email && c.Id != excludeClientId);
        }

        public async Task<bool> NomExistsAsync(string nom, int excludeClientId = 0)
        {
            if (string.IsNullOrWhiteSpace(nom))
                return false;

            return await _dbSet
                .AsNoTracking()
                .AnyAsync(c => c.Nom.ToLower() == nom.ToLower() && c.Id != excludeClientId);
        }

        public async Task<bool> TelephoneExistsAsync(string telephone, int excludeClientId = 0)
        {
            if (string.IsNullOrWhiteSpace(telephone))
                return false;

            var normalizedPhone = telephone.Replace(" ", "").Replace("-", "").Replace(".", "");

            return await _dbSet
                .AsNoTracking()
                .AnyAsync(c => c.Telephone != null 
                    && c.Telephone.Replace(" ", "").Replace("-", "").Replace(".", "") == normalizedPhone 
                    && c.Id != excludeClientId);
        }

        public async Task<List<Client>> GetClientsWithFacturesAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Include(c => c.Factures)
                .OrderBy(c => c.Nom)
                .ToListAsync();
        }

        public async Task<Client?> GetClientWithFacturesAsync(int clientId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(c => c.Factures)
                .FirstOrDefaultAsync(c => c.Id == clientId);
        }
    }
}
