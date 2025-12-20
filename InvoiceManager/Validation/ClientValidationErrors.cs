using InvoiceManager.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace InvoiceManager.Validation
{
    /// <summary>
    /// Classe pour stocker les erreurs de validation personnalisées
    /// </summary>
    public class ClientValidationErrors
    {
        public string? NomError { get; set; }
        public string? EmailError { get; set; }
        public string? TelephoneError { get; set; }

        public bool HasErrors => !string.IsNullOrEmpty(NomError) 
                               || !string.IsNullOrEmpty(EmailError) 
                               || !string.IsNullOrEmpty(TelephoneError);

        public void Clear()
        {
            NomError = null;
            EmailError = null;
            TelephoneError = null;
        }
    }
}
