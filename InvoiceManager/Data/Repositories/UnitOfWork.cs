using InvoiceManager.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace InvoiceManager.Data.Repositories
{
    /// <summary>
    /// Implémentation du Unit of Work
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        public IClientRepository Clients { get; }
        public IFactureRepository Factures { get; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Clients = new ClientRepository(context);
            Factures = new FactureRepository(context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
