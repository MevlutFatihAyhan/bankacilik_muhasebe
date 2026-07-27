using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BankAPI.Models
{
    // Angular'daki transfer formundan gelen istek gövdesi.
    // Alan adları wire formatında camelCase (senderIban, receiverIban, amount, description),
    // C# tarafında ise proje genelindeki Türkçe isimlendirmeyle tutuluyor.
    public class ParaTransferiRequest
    {
        [Required(ErrorMessage = "Gönderen IBAN zorunludur.")]
        [StringLength(34, MinimumLength = 26, ErrorMessage = "Gönderen IBAN 26 karakter olmalıdır.")]
        [JsonPropertyName("senderIban")]
        public string GonderenIban { get; set; }

        [Required(ErrorMessage = "Alıcı IBAN zorunludur.")]
        [StringLength(34, MinimumLength = 26, ErrorMessage = "Alıcı IBAN 26 karakter olmalıdır.")]
        [JsonPropertyName("receiverIban")]
        public string AliciIban { get; set; }

        [Range(0.0001, 999999999999.9999, ErrorMessage = "Transfer tutarı 0'dan büyük olmalıdır.")]
        [JsonPropertyName("amount")]
        public decimal Tutar { get; set; }

        // MVD_HESAPHAREKET.ACIKLAMA VARCHAR2(200)
        [StringLength(200, ErrorMessage = "Açıklama en fazla 200 karakter olabilir.")]
        [JsonPropertyName("description")]
        public string Aciklama { get; set; }
    }

    // PKG_HESAP.PRC_PARA_TRANSFERI OUT parametrelerinin karşılığı.
    // IslemKodu '0' ise işlem başarılıdır; diğer kodlar iş kuralı ihlalidir
    // (100–108) ya da veritabanı hatasıdır (500).
    public class ParaTransferiSonuc
    {
        [JsonPropertyName("islemKodu")]
        public string IslemKodu { get; set; }

        // Angular tarafı hem başarıda hem hatada "message" alanını okuyor.
        [JsonPropertyName("message")]
        public string Mesaj { get; set; }

        [JsonPropertyName("referansNo")]
        public string ReferansNo { get; set; }

        // Oluşan iki hesap hareketinin ISLEM_ID'leri — dekonttaki "detay"
        // bağlantısı bu ID'lerle /admin/islem-detayi sayfasına gider.
        [JsonPropertyName("gonderenIslemId")]
        public decimal? GonderenIslemId { get; set; }

        [JsonPropertyName("aliciIslemId")]
        public decimal? AliciIslemId { get; set; }

        [JsonPropertyName("basarili")]
        public bool Basarili => IslemKodu == "0";
    }
}
