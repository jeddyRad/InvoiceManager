using InvoiceManager.Data;
using InvoiceManager.Data.Entities;
using InvoiceManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InvoiceManager.Services
{
    public class ClientService : IClientService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ClientService> _logger;

        public ClientService(AppDbContext context, ILogger<ClientService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Client>> GetAllAsync()
        {
            _logger.LogInformation("Récupération de tous les clients");
            try
            {
                var clients = await _context.Clients
                    .AsNoTracking()
                    .OrderBy(c => c.Nom)
                    .ToListAsync();
                
                _logger.LogInformation("Récupération de {Count} clients", clients.Count);
                return clients;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des clients");
                throw;
            }
        }

        public async Task<Client?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Récupération du client {ClientId}", id);
            try
            {
                var client = await _context.Clients
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (client == null)
                {
                    _logger.LogWarning("Client {ClientId} introuvable", id);
                }

                return client;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du client {ClientId}", id);
                throw;
            }
        }

        public async Task AddAsync(Client client)
        {
            _logger.LogInformation("Ajout d'un nouveau client: {Nom}", client.Nom);
            try
            {
                _context.Clients.Add(client);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Client {ClientId} créé avec succès: {Nom}", client.Id, client.Nom);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création du client: {Nom}", client.Nom);
                throw;
            }
        }

        public async Task UpdateAsync(Client client)
        {
            _logger.LogInformation("Mise à jour du client {ClientId}: {Nom}", client.Id, client.Nom);
            try
            {
                var existingClient = await _context.Clients.FindAsync(client.Id);
                
                if (existingClient == null)
                {
                    _logger.LogWarning("Impossible de mettre à jour le client {ClientId}: client introuvable", client.Id);
                    throw new InvalidOperationException($"Le client avec l'ID {client.Id} n'existe pas.");
                }

                existingClient.Nom = client.Nom;
                existingClient.Email = client.Email;
                existingClient.Telephone = client.Telephone;
                existingClient.Adresse = client.Adresse;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Client {ClientId} mis à jour avec succès", client.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour du client {ClientId}", client.Id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Suppression du client {ClientId}", id);
            try
            {
                var client = await _context.Clients.FindAsync(id);
                if (client != null)
                {
                    _context.Clients.Remove(client);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Client {ClientId} supprimé: {Nom}", id, client.Nom);
                }
                else
                {
                    _logger.LogWarning("Impossible de supprimer le client {ClientId}: client introuvable", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression du client {ClientId}", id);
                throw;
            }
        }
    }
}
