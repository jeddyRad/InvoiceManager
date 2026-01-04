using InvoiceManager.Data.Entities;

namespace InvoiceManager.Data.Repositories.Interfaces
{
    /// <summary>
    /// Repository spécifique pour les factures
    /// </summary>
    public interface IFactureRepository : IRepository<Facture>
    {
        Task<List<Facture>> GetAllWithDetailsAsync();
        Task<Facture?> GetByIdWithDetailsAsync(int id);
        Task<List<Facture>> GetByClientIdAsync(int clientId);
        Task<List<Facture>> GetByStatutAsync(FactureStatut statut);
        Task<Facture?> GetByNumeroAsync(string numero);
        Task<int> GetNextFactureNumberAsync();
        void DetachTrackedEntities(int factureId);
    }
}
