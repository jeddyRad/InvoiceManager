using System.ComponentModel.DataAnnotations;

namespace InvoiceManager.Data.Entities
{
    public enum FactureStatut
    {
        Brouillon,
        Validee,
        Annulee
    }

    public class Facture
    {
        public int Id { get; set; }

        public string Numero { get; set; } = "TEMP";

        public DateTime DateFacture { get; set; } = DateTime.UtcNow;

        public FactureStatut Statut { get; set; } = FactureStatut.Brouillon;

        [Range(1, int.MaxValue, ErrorMessage = "Sélectionnez un client.")]
        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        // Totaux calculés automatiquement - setters privés
        public decimal TotalHT { get; private set; }
        public decimal TotalTTC { get; private set; }
        public decimal TVA { get; private set; }

        public ICollection<LigneFacture> Lignes { get; set; } = new List<LigneFacture>();

        /// <summary>
        /// Recalcule les totaux de la facture en fonction des lignes
        /// </summary>
        public void CalculerTotaux(decimal tauxTVA = 0.20m)
        {
            TotalHT = Lignes.Sum(l => l.TotalLigne);
            TVA = TotalHT * tauxTVA;
            TotalTTC = TotalHT + TVA;
        }

        /// <summary>
        /// Valide la facture (change le statut à Validée)
        /// </summary>
        public void Valider()
        {
            if (Statut != FactureStatut.Brouillon)
                throw new InvalidOperationException("Seules les factures en brouillon peuvent être validées.");

            if (!Lignes.Any())
                throw new InvalidOperationException("Impossible de valider une facture sans ligne.");

            if (ClientId == 0)
                throw new InvalidOperationException("Un client doit être sélectionné.");

            Statut = FactureStatut.Validee;
        }

        /// <summary>
        /// Annule la facture (change le statut à Annulée)
        /// </summary>
        public void Annuler()
        {
            if (Statut != FactureStatut.Validee)
                throw new InvalidOperationException("Seules les factures validées peuvent être annulées.");

            Statut = FactureStatut.Annulee;
        }

        /// <summary>
        /// Vérifie si la facture peut être modifiée
        /// </summary>
        public bool PeutEtreModifiee() => Statut == FactureStatut.Brouillon;
    }
}
