using InvoiceManager.Data.Entities;
using InvoiceManager.Data.Repositories.Interfaces;
using InvoiceManager.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InvoiceManager.Services
{
    /// <summary>
    /// Service métier pour les factures (utilise le Repository)
    /// </summary>
    public class FactureService : IFactureService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FactureService> _logger;

        public FactureService(IUnitOfWork unitOfWork, ILogger<FactureService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<Facture>> GetAllAsync()
        {
            _logger.LogInformation("Récupération de toutes les factures");
            try
            {
                var factures = await _unitOfWork.Factures.GetAllWithDetailsAsync();
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
                var facture = await _unitOfWork.Factures.GetByIdWithDetailsAsync(id);

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
                var count = await _unitOfWork.Factures.GetNextFactureNumberAsync();
                facture.Numero = $"FAC-{DateTime.Now:yyyyMM}-{count:0000}";
                facture.DateFacture = DateTime.Now;
                facture.Statut = FactureStatut.Brouillon;

                facture.CalculerTotaux();

                await _unitOfWork.Factures.AddAsync(facture);
                await _unitOfWork.SaveChangesAsync();
                
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
                _unitOfWork.Factures.DetachTrackedEntities(facture.Id);

                var existing = await _unitOfWork.Factures.GetByIdWithDetailsAsync(facture.Id);

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
                foreach (var ligne in toRemove)
                {
                    existing.Lignes.Remove(ligne);
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
                _unitOfWork.Factures.Update(existing);
                await _unitOfWork.SaveChangesAsync();
                
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
                var facture = await _unitOfWork.Factures.GetByIdAsync(id);
                
                if (facture != null)
                {
                    if (!facture.PeutEtreModifiee())
                    {
                        _logger.LogWarning("Impossible de supprimer la facture {FactureId} avec statut {Statut}", 
                            id, facture.Statut);
                        throw new InvalidOperationException("Seules les factures brouillon peuvent être supprimées.");
                    }
                    
                    _unitOfWork.Factures.Remove(facture);
                    await _unitOfWork.SaveChangesAsync();
                    
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
                var facture = await _unitOfWork.Factures.GetByIdWithDetailsAsync(factureId);
                
                if (facture == null)
                {
                    _logger.LogWarning("Impossible de recalculer les totaux: facture {FactureId} introuvable", factureId);
                    return;
                }
                
                facture.CalculerTotaux();
                _unitOfWork.Factures.Update(facture);
                await _unitOfWork.SaveChangesAsync();
                
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
                var facture = await _unitOfWork.Factures.GetByIdWithDetailsAsync(id);
                
                if (facture == null)
                {
                    _logger.LogWarning("Impossible de valider la facture {FactureId}: facture introuvable", id);
                    throw new InvalidOperationException("Facture introuvable.");
                }

                facture.Valider();
                _unitOfWork.Factures.Update(facture);
                await _unitOfWork.SaveChangesAsync();
                
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
                var facture = await _unitOfWork.Factures.GetByIdAsync(id);
                
                if (facture == null)
                {
                    _logger.LogWarning("Impossible d'annuler la facture {FactureId}: facture introuvable", id);
                    throw new InvalidOperationException("Facture introuvable.");
                }

                facture.Annuler();
                _unitOfWork.Factures.Update(facture);
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("Facture {FactureId} ({Numero}) annulée", id, facture.Numero);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'annulation de la facture {FactureId}", id);
                throw;
            }
        }
    }
}
