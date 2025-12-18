namespace InvoiceManager.Services.Interfaces
{
    /// <summary>
    /// Service de gestion d'état centralisé pour l'application
    /// </summary>
    public interface IAppStateService
    {
        /// <summary>
        /// Événement déclenché lorsque les données changent
        /// </summary>
        event Action? OnChange;

        /// <summary>
        /// Notifie les abonnés qu'un changement a eu lieu
        /// </summary>
        void NotifyStateChanged();

        /// <summary>
        /// Obtient le nombre total de clients
        /// </summary>
        int TotalClients { get; }

        /// <summary>
        /// Obtient le nombre total de factures
        /// </summary>
        int TotalFactures { get; }

        /// <summary>
        /// Obtient le montant total des factures validées
        /// </summary>
        decimal TotalFacturesValidees { get; }

        /// <summary>
        /// Met à jour les statistiques
        /// </summary>
        Task RefreshStatisticsAsync();
    }
}
