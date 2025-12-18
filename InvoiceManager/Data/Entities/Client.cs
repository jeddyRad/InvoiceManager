using System.ComponentModel.DataAnnotations;

namespace InvoiceManager.Data.Entities
{
    public class Client
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        public string? Telephone { get; set; }
        public string? Adresse { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        public ICollection<Facture> Factures { get; set; } = new List<Facture>();
    }
}
