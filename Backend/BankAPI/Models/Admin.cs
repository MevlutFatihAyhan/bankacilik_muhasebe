using System;
using System.Text.Json.Serialization;

namespace BankAPI.Models
{
    // ADM_ADMIN tablosunun DTO karşılığı
    public class Admin
    {
        [JsonPropertyName("adminId")]
        public int AdminId { get; set; }

        [JsonPropertyName("adminKullaniciAdi")]
        public string AdminKullaniciAdi { get; set; }

        [JsonPropertyName("adminSifre")]
        public string AdminSifre { get; set; }
    }
}