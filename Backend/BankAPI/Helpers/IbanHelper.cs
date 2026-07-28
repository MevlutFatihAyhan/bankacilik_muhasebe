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

        public static string GenerateAccountNo(int musteriTipi = 1)
        {
            string prefix = musteriTipi == 2 ? "TRT" : "TRB";
            Random random = new Random();
            string randomDigits = "";
            for (int i = 0; i < 13; i++)
            {
                randomDigits += random.Next(0, 10).ToString();
            }
            return prefix + randomDigits;
        }

        public static string GenerateTrIban(string accountNo)
        {
            if (string.IsNullOrWhiteSpace(accountNo))
            {
                accountNo = GenerateAccountNo();
            }

            // Ensure accountNo is exactly 16 characters
            if (accountNo.Length > 16)
            {
                accountNo = accountNo.Substring(accountNo.Length - 16);
            }
            else if (accountNo.Length < 16)
            {
                accountNo = accountNo.PadLeft(16, '0');
            }

            // The actual BBAN that will appear in the IBAN
            string bban = BANK_CODE + "0" + accountNo;

            // Convert BBAN letters to numbers for Mod 97 calculation
            string numericBban = "";
            foreach (char c in bban)
            {
                if (char.IsDigit(c))
                {
                    numericBban += c;
                }
                else if (char.IsLetter(c))
                {
                    numericBban += (char.ToUpperInvariant(c) - 'A' + 10).ToString();
                }
            }

            // Add TR (2927) and 00 for check digit calculation
            string modString = numericBban + "292700";

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

            // Extract parts
            string kk = cleanIban.Substring(2, 2);
            string bban = cleanIban.Substring(4, 22);

            // Convert BBAN letters to numbers
            string numericBban = "";
            foreach (char c in bban)
            {
                if (char.IsDigit(c))
                {
                    numericBban += c;
                }
                else if (char.IsLetter(c))
                {
                    numericBban += (char.ToUpperInvariant(c) - 'A' + 10).ToString();
                }
                else
                {
                    return false; // Invalid character
                }
            }

            string modString = numericBban + "2927" + kk;

            int remainder = 0;
            foreach (char c in modString)
            {
                remainder = (remainder * 10 + (c - '0')) % 97;
            }

            return remainder == 1;
        }
    }
}
