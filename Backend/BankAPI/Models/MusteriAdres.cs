using System;
using System.Text.Json.Serialization;

namespace BankAPI.Models
{
    public class MusteriAdres
    {
        [JsonPropertyName("adresId")]
        public decimal AdresID { get; set; }

        [JsonPropertyName("musteriId")]
        public decimal MusteriID { get; set; }

        [JsonPropertyName("adresBaslik")]
        public string ADRES_BASLIK { get; set; }

        [JsonPropertyName("ulke")]
        public string ULKE { get; set; }

        [JsonPropertyName("sehir")]
        public string SEHIR { get; set; }

        [JsonPropertyName("ilce")]
        public string ILCE { get; set; }

        [JsonPropertyName("postaKodu")]
        public string POSTA_KODU { get; set; }

        [JsonPropertyName("acikAdres")]
        public string ACIK_ADRES { get; set; }

        [JsonPropertyName("olusturmaTarihi")]
        public DateTime? OLUSTURMA_TARIHI { get; set; }

        [JsonPropertyName("guncellemeTarihi")]
        public DateTime? GUNCELLEME_TARIHI { get; set; }
    }
}
