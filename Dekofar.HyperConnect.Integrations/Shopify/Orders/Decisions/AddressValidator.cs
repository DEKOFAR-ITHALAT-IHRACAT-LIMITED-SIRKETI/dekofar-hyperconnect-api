using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text.RegularExpressions;

namespace Dekofar.HyperConnect.Integrations.Shopify.Orders.Decisions
{
    public static class AddressValidator
    {
        private static readonly string[] RiskKeywords =
        {
            "avm",
            "şube",
            "sube",
            "kargo",
            "teslim al",
            "hastane",
            "köy",
            "koyu",
            "kasaba"
        };

        private static readonly Regex DigitRegex =
            new(@"\d", RegexOptions.Compiled);

        public static AddressValidationResult Validate(JObject order)
        {
            var result = new AddressValidationResult();

            var address =
                order["shipping_address"]?["address1"]?
                    .ToString()?.ToLowerInvariant();

            var phone =
                order["shipping_address"]?["phone"]?.ToString();

            // 1️⃣ Telefon kontrolü
            if (string.IsNullOrWhiteSpace(phone))
            {
                result.Reasons.Add("Telefon numarası eksik");
            }

            // 2️⃣ Adres boş
            if (string.IsNullOrWhiteSpace(address))
            {
                result.Reasons.Add("Adres bilgisi boş");
                result.IsValid = false;
                return result;
            }

            // 3️⃣ Uzunluk kontrolü
            if (address.Length < 30)
            {
                result.Reasons.Add("Adres 30 karakterden kısa");
            }

            // 4️⃣ Rakam kontrolü
            if (!DigitRegex.IsMatch(address))
            {
                result.Reasons.Add("Adres içinde bina numarası yok");
            }

            // 5️⃣ Riskli kelimeler
            if (RiskKeywords.Any(k => address.Contains(k)))
            {
                result.Reasons.Add(
                    "Adres riskli kelime içeriyor (şube/köy/teslim noktası)");
            }

            result.IsValid = result.Reasons.Count == 0;
            return result;
        }
    }
}
