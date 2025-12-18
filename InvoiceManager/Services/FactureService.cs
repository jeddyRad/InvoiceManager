using InvoiceManager.Data;
using InvoiceManager.Data.Entities;
using InvoiceManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace InvoiceManager.Services
{
    public class FactureService : IFactureService
    {
        private readonly AppDbContext _context;

        public FactureService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Facture>> GetAllAsync()
        {
            return await _context.Factures
                .AsNoTracking()
                .Include(f => f.Client)
                .OrderByDescending(f => f.DateFacture)
                .ToListAsync();
        }

        public async Task<Facture?> GetByIdAsync(int id)
        {
            return await _context.Factures
                .AsNoTracking()
                .Include(f => f.Client)
                .Include(f => f.Lignes)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task AddAsync(Facture facture)
        {
            // Auto-generate number: FAC-YYYYMM-XXXX
            var count = await _context.Factures.CountAsync() + 1;
            facture.Numero = $"FAC-{DateTime.Now:yyyyMM}-{count:0000}";
            facture.DateFacture = DateTime.Now;
            facture.Statut = FactureStatut.Brouillon;

            _context.Factures.Add(facture);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Facture facture)
        {
            // Détacher toutes les entités potentiellement suivies
            DetachTrackedEntities(facture.Id);

            var existing = await _context.Factures
                .Include(f => f.Lignes)
                .FirstOrDefaultAsync(f => f.Id == facture.Id);

            if (existing == null)
            {
                throw new InvalidOperationException($"La facture avec l'ID {facture.Id} n'existe pas.");
            }

            if (existing.Statut != FactureStatut.Brouillon)
            {
                throw new InvalidOperationException("Impossible de modifier une facture validée ou annulée.");
            }

            // Mettre à jour les propriétés scalaires
            existing.DateFacture = facture.DateFacture;
            existing.ClientId = facture.ClientId;
            existing.TotalHT = facture.TotalHT;
            existing.TotalTTC = facture.TotalTTC;

            // Synchroniser les lignes
            // Supprimer les lignes qui ne sont plus présentes
            var incomingIds = facture.Lignes.Where(l => l.Id != 0).Select(l => l.Id).ToHashSet();
            var toRemove = existing.Lignes.Where(l => !incomingIds.Contains(l.Id)).ToList();
            foreach (var del in toRemove)
            {
                _context.LigneFactures.Remove(del);
            }

            // Ajouter ou mettre à jour les lignes
            foreach (var line in facture.Lignes)
            {
                if (line.Id == 0)
                {
                    // Nouvelle ligne
                    existing.Lignes.Add(new LigneFacture
                    {
                        Description = line.Description,
                        Quantite = line.Quantite,
                        PrixUnitaire = line.PrixUnitaire,
                        FactureId = existing.Id
                    });
                }
                else
                {
                    // Mise à jour d'une ligne existante
                    var target = existing.Lignes.FirstOrDefault(l => l.Id == line.Id);
                    if (target != null)
                    {
                        target.Description = line.Description;
                        target.Quantite = line.Quantite;
                        target.PrixUnitaire = line.PrixUnitaire;
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var facture = await _context.Factures.FindAsync(id);
            if (facture != null)
            {
                if (facture.Statut != FactureStatut.Brouillon)
                {
                    throw new InvalidOperationException("Seules les factures brouillon peuvent être supprimées.");
                }
                _context.Factures.Remove(facture);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RecalculerTotauxAsync(int factureId)
        {
            var facture = await _context.Factures
                .Include(f => f.Lignes)
                .FirstOrDefaultAsync(f => f.Id == factureId);
            if (facture == null)
                return;
            
            facture.TotalHT = facture.Lignes.Sum(l => l.TotalLigne);
            facture.TotalTTC = facture.TotalHT * 1.2m;

            await _context.SaveChangesAsync();
        }

        public async Task ValiderFactureAsync(int id)
        {
            var facture = await _context.Factures
                .Include(f => f.Lignes)
                .FirstOrDefaultAsync(f => f.Id == id);
            
            if (facture == null)
                throw new InvalidOperationException("Facture introuvable.");

            if (facture.Statut != FactureStatut.Brouillon)
                throw new InvalidOperationException("Seules les factures en brouillon peuvent être validées.");

            if (!facture.Lignes.Any())
                throw new InvalidOperationException("Impossible de valider une facture sans ligne.");

            if (facture.ClientId == 0)
                throw new InvalidOperationException("Un client doit être sélectionné.");

            facture.Statut = FactureStatut.Validee;
            await _context.SaveChangesAsync();
        }

        public async Task AnnulerFactureAsync(int id)
        {
            var facture = await _context.Factures.FindAsync(id);
            
            if (facture == null)
                throw new InvalidOperationException("Facture introuvable.");

            if (facture.Statut != FactureStatut.Validee)
                throw new InvalidOperationException("Seules les factures validées peuvent être annulées.");

            facture.Statut = FactureStatut.Annulee;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Détache les entités Facture et LigneFacture suivies pour éviter les conflits
        /// </summary>
        private void DetachTrackedEntities(int factureId)
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
        