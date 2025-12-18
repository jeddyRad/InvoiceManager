using System.ComponentModel.DataAnnotations;

namespace InvoiceManager.Data.Entities
{
    public class LigneFacture
    {
        public int Id { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int Quantite { get; set; } = 1;

        [Range(0, double.MaxValue)]
        public decimal PrixUnitaire { get; set; } = 0;

        public decimal TotalLigne => Quantite * PrixUnitaire;

        public int FactureId { get; set; }
        public Facture Facture { get; set; } = null!;
    }

}
