using System.ComponentModel.DataAnnotations;

namespace InvoiceManager.Data.Entities
{
    public class Facture
    {
        public int Id { get; set; }

        [Required]
        public string Numero { get; set; } = string.Empty;

        public DateTime DateFacture { get; set; } = DateTime.UtcNow;

        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public decimal TotalHT { get; set; }
        public decimal TotalTTC { get; set; }

        public ICollection<LigneFacture> Lignes { get; set; } = new List<LigneFacture>();
    }
}
