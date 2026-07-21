using System;
using System.Text.Json.Serialization;

namespace BankAPI.Models
{
    public class Hesap
    {
        [JsonPropertyName("hesapNo")]
        public string HESAP_NO { get; set; }

        [JsonPropertyName("musteriId")]
        public decimal MUSTERI_ID { get; set; }

        [JsonPropertyName("iban")]
        public string IBAN { get; set; }

        [JsonPropertyName("hesapTuru")]
        public string HESAP_TURU { get; set; }

        [JsonPropertyName("dovizCinsi")]
        public string DOVIZ_CINSI { get; set; }

        [JsonPropertyName("bakiye")]
        public decimal BAKIYE { get; set; }

        [JsonPropertyName("durum")]
        public int DURUM { get; set; }
    }
}
