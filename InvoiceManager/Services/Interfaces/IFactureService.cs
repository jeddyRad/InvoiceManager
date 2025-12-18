using InvoiceManager.Data.Entities;

namespace InvoiceManager.Services.Interfaces
{
    public interface IFactureService
    {
        Task<List<Facture>> GetAllAsync();
        Task<Facture?> GetByIdAsync(int id);
        Task AddAsync(Facture facture);
        Task UpdateAsync(Facture facture);
        Task DeleteAsync(int id);
        Task RecalculerTotauxAsync(int factureId);
    }
}
