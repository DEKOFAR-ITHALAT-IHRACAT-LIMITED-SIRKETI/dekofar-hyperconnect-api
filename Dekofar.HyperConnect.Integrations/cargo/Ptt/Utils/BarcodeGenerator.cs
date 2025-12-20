using System;

namespace Dekofar.HyperConnect.Integrations.Kargo.Ptt.Utils
{
    public static class BarcodeGenerator
    {
        /// <summary>
        /// Verilen 12 haneli barkod kökünden 13. haneyi hesaplayarak tam barkod döner.
        /// </summary>
        public static string Generate(string base12)
        {
            if (string.IsNullOrWhiteSpace(base12) || base12.Length != 12)
                throw new ArgumentException("Barkod kökü 12 haneli olmalı.");

            int sum = 0;
            for (int i = 0; i < base12.Length; i++)
            {
                int digit = int.Parse(base12[i].ToString());
                sum += digit * ((i % 2 == 0) ? 1 : 3);
            }

            int checkDigit = (10 - (sum % 10)) % 10;

            return base12 + checkDigit;
        }

        /// <summary>
        /// Belirtilen aralıkta rastgele 13 haneli barkod üretir.
        /// </summary>
        public static string GenerateRandomInRange(long start, long end)
        {
            var random = new Random();
            long number = random.NextInt64(start, end); // .NET 6+
            var base12 = number.ToString().PadLeft(12, '0');

            return Generate(base12);
        }
    }
}
