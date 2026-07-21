using System;
using System.Text.Json.Serialization;

namespace BankAPI.Models
{
    public class HesapHareket
    {
        [JsonPropertyName("islemId")]
        public decimal ISLEM_ID { get; set; }

        [JsonPropertyName("hesapNo")]
        public string HESAP_NO { get; set; }

        [JsonPropertyName("islemYonu")]
        public string ISLEM_YONU { get; set; }

        [JsonPropertyName("islemTutari")]
        public decimal ISLEM_TUTARI { get; set; }

        [JsonPropertyName("dovizCinsi")]
        public string DOVIZ_CINSI { get; set; }

        [JsonPropertyName("yeniBakiye")]
        public decimal YENI_BAKIYE { get; set; }

        [JsonPropertyName("islemTarihi")]
        public DateTime ISLEM_TARIHI { get; set; }

        [JsonPropertyName("aciklama")]
        public string ACIKLAMA { get; set; }

        [JsonPropertyName("islemKodu")]
        public string ISLEM_KODU { get; set; }

        [JsonPropertyName("referansNo")]
        public string REFERANS_NO { get; set; }
    }
}
