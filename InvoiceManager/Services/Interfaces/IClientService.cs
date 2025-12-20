using InvoiceManager.Data.Entities;

namespace InvoiceManager.Services.Interfaces
{
    public interface IClientService
    {
        Task<List<Client>> GetAllAsync();
        Task<Client?> GetByIdAsync(int id);
        Task AddAsync(Client client);
        Task UpdateAsync(Client client);
        Task DeleteAsync(int id);
        Task<bool> EmailExistsAsync(string email, int excludeClientId = 0);
        Task<bool> NomExistsAsync(string nom, int excludeClientId = 0);
        Task<bool> TelephoneExistsAsync(string telephone, int excludeClientId = 0);
    }
}
