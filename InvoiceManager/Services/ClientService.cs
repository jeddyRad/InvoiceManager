using InvoiceManager.Data;
using InvoiceManager.Data.Entities;
using InvoiceManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.Services
{
    public class ClientService : IClientService
    {
        private readonly AppDbContext _context;
        public ClientService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Client>> GetAllAsync()
        {
            return await _context.Clients
                .AsNoTracking()
                .OrderBy(c => c.Nom)
                .ToListAsync();
        }

        public async Task<Client?> GetByIdAsync(int id)
        {
            return await _context.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Client client)
        {
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Client client)
        {
            // Récupérer l'entité existante depuis la base de données
            var existingClient = await _context.Clients.FindAsync(client.Id);
            
            if (existingClient == null)
            {
                throw new InvalidOperationException($"Le client avec l'ID {client.Id} n'existe pas.");
            }

            // Mettre à jour uniquement les propriétés modifiables
            existingClient.Nom = client.Nom;
            existingClient.Email = client.Email;
            existingClient.Telephone = client.Telephone;
            existingClient.Adresse = client.Adresse;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();
            }
        }
    }
}
