using System;
using System.Text.RegularExpressions;

namespace BankAPI.Helpers
{
    public static class IbanHelper
    {
        // Standard Bank Code for BankAPI (5 digits)
        private const string BANK_CODE = "00062";

        public static string NormalizeDovizCinsi(string dovizCinsi)
        {
            if (string.IsNullOrWhiteSpace(dovizCinsi))
                return "TRY";

            string clean = dovizCinsi.Trim().ToUpperInvariant();

            return clean switch
            {
                "EURO" or "EUR" => "EUR",
                "DOLAR" or "USD" or "DOLLAR" => "USD",
                "ALTIN" or "XAU" or "ALT" => "XAU",
                "TL" or "TRY" or "TURK LIRASI" => "TRY",
                _ => clean.Length > 3 ? clean.Substring(0, 3) : clean
            };
        }

        public static string GenerateAccountNo()
        {
            Random random = new Random();
            string randomDigits = "";
            for (int i = 0; i < 12; i++)
            {
                randomDigits += random.Next(0, 10).ToString();
            }
            return "1000" + randomDigits;
        }

        public static string GenerateTrIban(string accountNo)
        {
            if (string.IsNullOrWhiteSpace(accountNo))
            {
                accountNo = GenerateAccountNo();
            }

            string digitsOnly = Regex.Replace(accountNo, @"[^\d]", "");
            if (string.IsNullOrEmpty(digitsOnly))
            {
                digitsOnly = "1000000000000001";
            }

            if (digitsOnly.Length > 16)
            {
                digitsOnly = digitsOnly.Substring(digitsOnly.Length - 16);
            }
            else
            {
                digitsOnly = digitsOnly.PadLeft(16, '0');
            }

            string bban = BANK_CODE + "0" + digitsOnly;
            string modString = bban + "292700";

            int remainder = 0;
            foreach (char c in modString)
            {
                remainder = (remainder * 10 + (c - '0')) % 97;
            }

            int checkDigits = 98 - remainder;
            string kk = checkDigits.ToString("D2");

            return "TR" + kk + bban;
        }

        public static bool ValidateTrIban(string iban)
        {
            if (string.IsNullOrWhiteSpace(iban))
                return false;

            string cleanIban = Regex.Replace(iban.Trim().ToUpperInvariant(), @"\s+", "");

            if (cleanIban.Length != 26 || !cleanIban.StartsWith("TR"))
                return false;

            string numericPart = cleanIban.Substring(2);
            if (!Regex.IsMatch(numericPart, @"^\d{24}$"))
                return false;

            string bban = cleanIban.Substring(4, 22);
            string kk = cleanIban.Substring(2, 2);

            string modString = bban + "2927" + kk;

            int remainder = 0;
            foreach (char c in modString)
            {
                remainder = (remainder * 10 + (c - '0')) % 97;
            }

            return remainder == 1;
        }
    }
}
