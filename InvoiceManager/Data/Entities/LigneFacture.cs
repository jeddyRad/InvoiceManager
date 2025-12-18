using System.ComponentModel.DataAnnotations;

namespace InvoiceManager.Data.Entities
{
    public class LigneFacture
    {
        public int Id { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int Quantite { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PrixUnitaire { get; set; }

        public decimal TotalLigne { get; set; }

        public int FactureId { get; set; }
        public Facture Facture { get; set; } = null!;
    }
}
