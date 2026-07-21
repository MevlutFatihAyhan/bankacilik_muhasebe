using System;
using System.Text.Json.Serialization;

namespace BankAPI.Models
{
    // MST_MUSTERI tablosunun DTO karşılığı
    public class Musteri
    {
        [JsonPropertyName("musteriId")]
        public decimal MusteriID { get; set; }

        [JsonPropertyName("ad")]
        public string AD { get; set; }

        [JsonPropertyName("soyad")]
        public string SOYAD { get; set; }

        [JsonPropertyName("musteriTipi")]
        public int MUSTERI_TIPI { get; set; } // 1: Bireysel, 2: Tüzel

        [JsonPropertyName("kimlikNo")]
        public string KIMLIK_NO { get; set; }

        [JsonPropertyName("email")]
        public string EMAIL { get; set; }

        [JsonPropertyName("telefon")]
        public string TELEFON { get; set; }

        [JsonPropertyName("aktifMi")]
        public int AKTIF_MI { get; set; } // 1: Aktif, 2: Pasif

        [JsonPropertyName("olusturmaTarihi")]
        public DateTime? OLUSTURMA_TARIHI { get; set; }

        [JsonPropertyName("guncellemeTarihi")]
        public DateTime? GUNCELLEME_TARIHI { get; set; }
    }
}
