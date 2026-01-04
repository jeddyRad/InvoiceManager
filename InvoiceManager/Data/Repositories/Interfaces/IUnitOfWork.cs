namespace InvoiceManager.Data.Repositories.Interfaces
{
    /// <summary>
    /// Pattern Unit of Work pour gérer les transactions
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IClientRepository Clients { get; }
        IFactureRepository Factures { get; }
        
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
