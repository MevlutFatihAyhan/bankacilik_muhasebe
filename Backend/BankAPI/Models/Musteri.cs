using System;
using System.Net;

using System.Text.Json.Serialization;

namespace BankAPI.Models
{
    public class Musteri
    {
        [JsonPropertyName("musteriId")]
        public decimal MusteriID { get; set; }
        public string AD { get; set; }
        public string SOYAD { get; set; }
        public int MUSTERI_TIPI { get; set; }
        public string TCKN_VKN { get; set; }
        public string EMAIL { get; set; }
        public string TELEFON { get; set; }
        public int AKTIFMI { get; set; }
        public DateTime OLUSTURMA_TARIHI { get; set; }
        public DateTime GUNCELLENME_TARIHI { get; set; }
    }
}
