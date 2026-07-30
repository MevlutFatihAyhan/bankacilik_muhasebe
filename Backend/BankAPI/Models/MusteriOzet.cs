using System;
using System.Text.Json.Serialization;

namespace BankAPI.Models
{
    public class MusteriOzet
    {
        [JsonPropertyName("musteriId")]
        public decimal MusteriId { get; set; }

        [JsonPropertyName("ad")]
        public string Ad { get; set; } = string.Empty;

        [JsonPropertyName("soyad")]
        public string Soyad { get; set; } = string.Empty;
    }
}
