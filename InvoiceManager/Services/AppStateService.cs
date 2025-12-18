using InvoiceManager.Data.Entities;
using InvoiceManager.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InvoiceManager.Services
{
    /// <summary>
    /// Service de gestion d'état centralisé pour l'application
    /// Permet de partager des données et des événements entre composants
    /// </summary>
    public class AppStateService : IAppStateService
    {
        private readonly IClientService _clientService;
        private readonly IFactureService _factureService;
        private readonly ILogger<AppStateService> _logger;

        public event Action? OnChange;

        public int TotalClients { get; private set; }
        public int TotalFactures { get; private set; }
        public decimal TotalFacturesValidees { get; private set; }

        public AppStateService(
            IClientService clientService,
            IFactureService factureService,
            ILogger<AppStateService> logger)
        {
            _clientService = clientService;
            _factureService = factureService;
            _logger = logger;
        }

        /// <summary>
        /// Notifie tous les composants abonnés qu'un changement d'état a eu lieu
        /// </summary>
        public void NotifyStateChanged()
        {
            _logger.LogDebug("Notification de changement d'état");
            OnChange?.Invoke();
        }

        /// <summary>
        /// Rafraîchit les statistiques de l'application
        /// </summary>
        public async Task RefreshStatisticsAsync()
        {
            try
            {
                _logger.LogInformation("Rafraîchissement des statistiques");

                var clients = await _clientService.GetAllAsync();
                var factures = await _factureService.GetAllAsync();

                TotalClients = clients.Count;
                TotalFactures = factures.Count;
                TotalFacturesValidees = factures
                    .Where(f => f.Statut == FactureStatut.Validee)
                    .Sum(f => f.TotalTTC);

                _logger.LogInformation(
                    "Statistiques: {Clients} clients, {Factures} factures, {Total:C} validées",
                    TotalClients, TotalFactures, TotalFacturesValidees);

                NotifyStateChanged();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du rafraîchissement des statistiques");
            }
        }
    }
}
