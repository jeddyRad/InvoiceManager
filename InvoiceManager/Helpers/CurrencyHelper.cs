using System.Globalization;

namespace InvoiceManager.Helpers
{
    /// <summary>
    /// Helper pour le formatage des montants en Ariary Malgache
    /// </summary>
    public static class CurrencyHelper
    {
        private static readonly CultureInfo MalagasyCulture;

        static CurrencyHelper()
        {
            MalagasyCulture = new CultureInfo("mg-MG");
            MalagasyCulture.NumberFormat.CurrencySymbol = "Ar";
            MalagasyCulture.NumberFormat.CurrencyDecimalDigits = 0;
            MalagasyCulture.NumberFormat.CurrencyPositivePattern = 3; // n Ar
            MalagasyCulture.NumberFormat.CurrencyNegativePattern = 8; // -n Ar
            MalagasyCulture.NumberFormat.CurrencyGroupSeparator = " "; // Séparateur de milliers
        }

        /// <summary>
        /// Formate un montant en Ariary Malgache
        /// </summary>
        /// <param name="amount">Montant à formater</param>
        /// <returns>Montant formaté (ex: "25 000 Ar")</returns>
        public static string FormatCurrency(decimal amount)
        {
            return amount.ToString("C", MalagasyCulture);
        }

        /// <summary>
        /// Formate un montant en Ariary avec un format personnalisé
        /// </summary>
        /// <param name="amount">Montant à formater</param>
        /// <returns>Montant formaté (ex: "25 000 Ar")</returns>
        public static string FormatAriary(this decimal amount)
        {
            return amount.ToString("N0", MalagasyCulture) + " Ar";
        }

        /// <summary>
        /// Symbole de la devise
        /// </summary>
        public static string CurrencySymbol => "Ar";

        /// <summary>
        /// Nom de la devise
        /// </summary>
        public static string CurrencyName => "Ariary Malgache";
    }
}
