using InvoiceManager.Data.Entities;

namespace InvoiceManager.Data.Repositories.Interfaces
{
    /// <summary>
    /// Repository spécifique pour les clients avec méthodes métier
    /// </summary>
    public interface IClientRepository : IRepository<Client>
    {
        Task<bool> EmailExistsAsync(string email, int excludeClientId = 0);
        Task<bool> NomExistsAsync(string nom, int excludeClientId = 0);
        Task<bool> TelephoneExistsAsync(string telephone, int excludeClientId = 0);
        Task<List<Client>> GetClientsWithFacturesAsync();
        Task<Client?> GetClientWithFacturesAsync(int clientId);
    }
}
