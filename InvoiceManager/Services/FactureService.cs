using InvoiceManager.Data;
using InvoiceManager.Data.Entities;
using InvoiceManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace InvoiceManager.Services
{
    public class FactureService : IFactureService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FactureService> _logger;

        public FactureService(AppDbContext context, ILogger<FactureService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Facture>> GetAllAsync()
        {
            _logger.LogInformation("Récupération de toutes les factures");
            try
            {
                var factures = await _context.Factures
                    .AsNoTracking()
                    .Include(f => f.Client)
                    .OrderByDescending(f => f.DateFacture)
                    .ToListAsync();

                _logger.LogInformation("Récupération de {Count} factures", factures.Count);
                return factures;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des factures");
                throw;
            }
        }

        public async Task<Facture?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Récupération de la facture {FactureId}", id);
            try
            {
                var facture = await _context.Factures
                    .AsNoTracking()
                    .Include(f => f.Client)
                    .Include(f => f.Lignes)
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (facture == null)
                {
                    _logger.LogWarning("Facture {FactureId} introuvable", id);
                }

                return facture;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la facture {FactureId}", id);
                throw;
            }
        }

        public async Task AddAsync(Facture facture)
        {
            _logger.LogInformation("Création d'une nouvelle facture pour le client {ClientId}", facture.ClientId);
            try
            {
                var count = await _context.Factures.CountAsync() + 1;
                facture.Numero = $"FAC-{DateTime.Now:yyyyMM}-{count:0000}";
                facture.DateFacture = DateTime.Now;
                facture.Statut = FactureStatut.Brouillon;

                facture.CalculerTotaux();

                _context.Factures.Add(facture);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Facture {Numero} créée avec succès (ID: {FactureId})", facture.Numero, facture.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la facture");
                throw;
            }
        }

        public async Task UpdateAsync(Facture facture)
        {
            _logger.LogInformation("Mise à jour de la facture {FactureId}", facture.Id);
            try
            {
                DetachTrackedEntities(facture.Id);

                var existing = await _context.Factures
                    .Include(f => f.Lignes)
                    .FirstOrDefaultAsync(f => f.Id == facture.Id);

                if (existing == null)
                {
                    _logger.LogWarning("Impossible de mettre à jour la facture {FactureId}: facture introuvable", facture.Id);
                    throw new InvalidOperationException($"La facture avec l'ID {facture.Id} n'existe pas.");
                }

                if (!existing.PeutEtreModifiee())
                {
                    _logger.LogWarning("Tentative de modification de la facture {FactureId} avec statut {Statut}", 
                        facture.Id, existing.Statut);
                    throw new InvalidOperationException("Impossible de modifier une facture validée ou annulée.");
                }

                existing.DateFacture = facture.DateFacture;
                existing.ClientId = facture.ClientId;

                var incomingIds = facture.Lignes.Where(l => l.Id != 0).Select(l => l.Id).ToHashSet();
                var toRemove = existing.Lignes.Where(l => !incomingIds.Contains(l.Id)).ToList();
                
                _logger.LogDebug("Suppression de {Count} lignes", toRemove.Count);
                foreach (var del in toRemove)
                {
                    _context.LigneFactures.Remove(del);
                }

                foreach (var line in facture.Lignes)
                {
                    if (line.Id == 0)
                    {
                        _logger.LogDebug("Ajout d'une nouvelle ligne: {Description}", line.Description);
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
                        var target = existing.Lignes.FirstOrDefault(l => l.Id == line.Id);
                        if (target != null)
                        {
                            target.Description = line.Description;
                            target.Quantite = line.Quantite;
                            target.PrixUnitaire = line.PrixUnitaire;
                        }
                    }
                }

                existing.CalculerTotaux();
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Facture {FactureId} mise à jour avec succès", facture.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de la facture {FactureId}", facture.Id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Suppression de la facture {FactureId}", id);
            try
            {
                var facture = await _context.Factures.FindAsync(id);
                if (facture != null)
                {
                    if (!facture.PeutEtreModifiee())
                    {
                        _logger.LogWarning("Impossible de supprimer la facture {FactureId} avec statut {Statut}", 
                            id, facture.Statut);
                        throw new InvalidOperationException("Seules les factures brouillon peuvent être supprimées.");
                    }
                    _context.Factures.Remove(facture);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Facture {FactureId} ({Numero}) supprimée", id, facture.Numero);
                }
                else
                {
                    _logger.LogWarning("Impossible de supprimer la facture {FactureId}: facture introuvable", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de la facture {FactureId}", id);
                throw;
            }
        }

        public async Task RecalculerTotauxAsync(int factureId)
        {
            _logger.LogInformation("Recalcul des totaux de la facture {FactureId}", factureId);
            try
            {
                var facture = await _context.Factures
                    .Include(f => f.Lignes)
                    .FirstOrDefaultAsync(f => f.Id == factureId);
                
                if (facture == null)
                {
                    _logger.LogWarning("Impossible de recalculer les totaux: facture {FactureId} introuvable", factureId);
                    return;
                }
                
                facture.CalculerTotaux();
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Totaux recalculés pour la facture {FactureId}: HT={TotalHT:C}, TTC={TotalTTC:C}", 
                    factureId, facture.TotalHT, facture.TotalTTC);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du recalcul des totaux de la facture {FactureId}", factureId);
                throw;
            }
        }

        public async Task ValiderFactureAsync(int id)
        {
            _logger.LogInformation("Validation de la facture {FactureId}", id);
            try
            {
                var facture = await _context.Factures
                    .Include(f => f.Lignes)
                    .FirstOrDefaultAsync(f => f.Id == id);
                
                if (facture == null)
                {
                    _logger.LogWarning("Impossible de valider la facture {FactureId}: facture introuvable", id);
                    throw new InvalidOperationException("Facture introuvable.");
                }

                facture.Valider();
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Facture {FactureId} ({Numero}) validée avec succès", id, facture.Numero);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la validation de la facture {FactureId}", id);
                throw;
            }
        }

        public async Task AnnulerFactureAsync(int id)
        {
            _logger.LogInformation("Annulation de la facture {FactureId}", id);
            try
            {
                var facture = await _context.Factures.FindAsync(id);
                
                if (facture == null)
                {
                    _logger.LogWarning("Impossible d'annuler la facture {FactureId}: facture introuvable", id);
                    throw new InvalidOperationException("Facture introuvable.");
                }

                facture.Annuler();
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Facture {FactureId} ({Numero}) annulée", id, facture.Numero);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'annulation de la facture {FactureId}", id);
                throw;
            }
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