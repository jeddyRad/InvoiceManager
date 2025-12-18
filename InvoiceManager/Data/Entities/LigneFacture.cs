using System.ComponentModel.DataAnnotations;

namespace InvoiceManager.Data.Entities
{
    public class LigneFacture
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La description est obligatoire.")]
        public string Description { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "La quantité doit être supérieure à 0.")]
        public int Quantite { get; set; } = 1;

        [Range(0.01, double.MaxValue, ErrorMessage = "Le prix unitaire doit être positif.")]
        public decimal PrixUnitaire { get; set; } = 0.01m;

        public decimal TotalLigne => Quantite * PrixUnitaire;

        public int FactureId { get; set; }
        public Facture Facture { get; set; } = null!;
    }

}
