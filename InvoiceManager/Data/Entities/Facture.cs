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

        public string Numero { get; set; } = string.Empty;

        public DateTime DateFacture { get; set; } = DateTime.UtcNow;

        public FactureStatut Statut { get; set; } = FactureStatut.Brouillon;

        [Range(1, int.MaxValue, ErrorMessage = "Sélectionnez un client.")]
        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public decimal TotalHT { get; set; }
        public decimal TotalTTC { get; set; }

        public ICollection<LigneFacture> Lignes { get; set; } = new List<LigneFacture>();
    }
}
