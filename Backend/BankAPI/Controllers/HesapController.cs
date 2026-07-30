using Microsoft.AspNetCore.Mvc;
using BankAPI.Models;
using BankAPI.Services;

namespace BankAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HesapController : ControllerBase
    {
        private readonly HesapService _hesapService;

        public HesapController(HesapService hesapService)
        {
            _hesapService = hesapService;
        }

        // -------------------------------------------------------
        // Ortak yardımcı: Oracle hata mesajını Türkçe'ye çevirir.
        // -------------------------------------------------------
        private static IActionResult OracleHataYonet(ControllerBase ctrl, Exception ex, string islem = "işlem")
        {
            string msg = ex.Message;

            // --- Benzersizlik ihlalleri ---
            if (msg.Contains("ORA-00001") && msg.Contains("HESAP_NO"))
                return ctrl.Conflict(new { message = "Bu hesap numarası sistemde zaten kayıtlı! Lütfen farklı bir hesap numarası kullanın." });

            if (msg.Contains("ORA-00001") && msg.Contains("IBAN"))
                return ctrl.Conflict(new { message = "Bu IBAN numarası sistemde zaten kayıtlı! Lütfen yeni bir IBAN oluşturun." });

            // --- Müşteri bulunamadı (FK ihlali) ---
            if (msg.Contains("ORA-02291") && msg.Contains("MUSTERI_ID"))
                return ctrl.BadRequest(new { message = "Belirtilen müşteri ID sistemde kayıtlı değil. Lütfen geçerli bir müşteri seçin!" });

            // --- Check kısıtlamaları ---
            if (msg.Contains("CHK_HESAP_DURUM") || msg.Contains("ORA-02290") && msg.Contains("DURUM"))
                return ctrl.BadRequest(new { message = "Hesap durumu geçersiz! Yalnızca 0 (Pasif), 1 (Aktif) veya 2 (Dondurulmuş) değerleri kabul edilir." });

            if (msg.Contains("CHK_HESAP_BAKIYE") || msg.Contains("ORA-02290") && msg.Contains("BAKIYE"))
                return ctrl.BadRequest(new { message = "Hesap bakiyesi negatif olamaz!" });

            if (msg.Contains("CHK_HESAP_TURU") || msg.Contains("ORA-02290") && msg.Contains("HESAP_TURU"))
                return ctrl.BadRequest(new { message = "Geçersiz hesap türü! Vadesiz, Vadeli veya Yatırım olmalıdır." });

            if (msg.Contains("CHK_HESAP_DOVIZ") || msg.Contains("ORA-02290") && msg.Contains("DOVIZ"))
                return ctrl.BadRequest(new { message = "Geçersiz döviz cinsi! TRY, USD, EUR veya XAU (Altın) değerlerinden biri olmalıdır." });

            // --- Transfer / bakiye yetersiz ---
            if (msg.Contains("ORA-20010") || msg.Contains("bakiye yetersiz") || msg.Contains("Bakiye yetersiz"))
                return ctrl.BadRequest(new { message = "Transfer başarısız: Gönderen hesabın bakiyesi yetersiz!" });

            if (msg.Contains("ORA-20011") || msg.Contains("kaynak hesap bulunamadi") || msg.Contains("Kaynak hesap bulunamadi"))
                return ctrl.NotFound(new { message = "Transfer başarısız: Kaynak hesap bulunamadı!" });

            if (msg.Contains("ORA-20012") || msg.Contains("hedef hesap bulunamadi") || msg.Contains("Hedef hesap bulunamadi"))
                return ctrl.NotFound(new { message = "Transfer başarısız: Hedef hesap bulunamadı!" });

            if (msg.Contains("ORA-20013") || msg.Contains("hesap pasif") || msg.Contains("Hesap pasif") || msg.Contains("dondurulmuş"))
                return ctrl.BadRequest(new { message = "Transfer başarısız: Kaynak veya hedef hesap aktif durumda değil!" });

            // --- NOT NULL ihlali ---
            if (msg.Contains("ORA-01400"))
                return ctrl.BadRequest(new { message = "Zorunlu bir alan boş bırakıldı. Lütfen tüm gerekli alanları doldurunuz." });

            // --- Sütun uzunluğu aşımı ---
            if (msg.Contains("ORA-12899"))
                return ctrl.BadRequest(new { message = "Girilen bir alan için izin verilen maksimum karakter sayısı aşıldı." });

            // --- Bağlantı sorunları ---
            if (msg.Contains("ORA-12541") || msg.Contains("ORA-12170") || msg.Contains("ORA-12154"))
                return ctrl.StatusCode(503, new { message = "Veritabanına bağlanılamadı. Lütfen daha sonra tekrar deneyiniz." });

            // --- Genel / bilinmeyen ---
            return ctrl.StatusCode(500, new { message = $"{islem} sırasında beklenmedik bir hata oluştu: {ex.Message}" });
        }

        [HttpPost]
        public IActionResult HesapEkle([FromBody] Hesap hesap)
        {
            try
            {
                _hesapService.HesapEkle(hesap);
                return Ok(new { message = "Hesap başarıyla açıldı" });
            }
            catch (Exception ex)
            {
                return OracleHataYonet(this, ex, "Hesap ekleme");
            }
        }

        [HttpGet]
        public IActionResult TumHesaplariGetir()
        {
            try
            {
                var hesapListesi = _hesapService.TumHesaplariGetir();
                return Ok(hesapListesi);
            }
            catch (Exception ex)
            {
                return OracleHataYonet(this, ex, "Hesap listeleme");
            }
        }

        // Filtreye bağlı listeleme — arayüzde "Uygula" butonuna basılmadan çağrılmaz
        [HttpGet("filtre")]
        public IActionResult HesaplariFiltrele(
            [FromQuery] string searchTerm = null,
            [FromQuery] int? musteriTipi = null,
            [FromQuery] string hesapTuru = null,
            [FromQuery] string dovizCinsi = null,
            [FromQuery] int? durum = null,
            [FromQuery] decimal? minBakiye = null,
            [FromQuery] decimal? maxBakiye = null,
            [FromQuery] string id = null,
            [FromQuery] string musteriAdi = null,
            [FromQuery] string musteriSoyadi = null)
        {
            try
            {
                var hesapListesi = _hesapService.HesaplariFiltrele(
                    searchTerm, musteriTipi, hesapTuru, dovizCinsi, durum, minBakiye, maxBakiye,
                    id, musteriAdi, musteriSoyadi);
                return Ok(hesapListesi);
            }
            catch (Exception ex)
            {
                return OracleHataYonet(this, ex, "Hesap filtreleme");
            }
        }

        [HttpGet("{hesapNo}")]
        public IActionResult HesapGetir(string hesapNo)
        {
            try
            {
                var hesap = _hesapService.HesapGetir(hesapNo);
                if (hesap == null)
                {
                    return NotFound(new { message = $"'{hesapNo}' hesap numarasına ait hesap bulunamadı." });
                }
                return Ok(hesap);
            }
            catch (Exception ex)
            {
                return OracleHataYonet(this, ex, "Hesap getirme");
            }
        }

        [HttpPut("{hesapNo}/durum")]
        public IActionResult HesapDurumGuncelle(string hesapNo, [FromBody] DurumGuncelleRequest request)
        {
            try
            {
                _hesapService.HesapDurumGuncelle(hesapNo, request.Durum);
                return Ok(new { message = "Hesap durumu güncellendi" });
            }
            catch (Exception ex)
            {
                return OracleHataYonet(this, ex, "Hesap durum güncelleme");
            }
        }

        [HttpGet("musteri/{musteriId}")]
        public IActionResult MusteriHesaplariGetir(decimal musteriId)
        {
            try
            {
                var hesapListesi = _hesapService.MusteriHesaplariGetir(musteriId);
                return Ok(hesapListesi);
            }
            catch (Exception ex)
            {
                return OracleHataYonet(this, ex, "Müşteri hesapları getirme");
            }
        }

        [HttpGet("generate-iban")]
        public IActionResult GenerateIban([FromQuery] string accountNo = null)
        {
            var accNo = string.IsNullOrWhiteSpace(accountNo) ? BankAPI.Helpers.IbanHelper.GenerateAccountNo() : accountNo;
            var iban = BankAPI.Helpers.IbanHelper.GenerateTrIban(accNo);
            return Ok(new { hesapNo = accNo, iban = iban });
        }

        // Para transferi — iş kuralları PKG_HESAP.PRC_PARA_TRANSFERI içinde çalışır.
        // İş kuralı ihlalinde (IBAN yok, hesap pasif, bakiye yetersiz vb.) 400 ve
        // islemKodu ile birlikte açıklayıcı mesaj döner.
        [HttpPost("transfer")]
        public IActionResult ParaTransferi([FromBody] ParaTransferiRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Transfer bilgileri gönderilmedi." });
            }

            // Model doğrulaması [ApiController] tarafından bu noktadan önce yapılır;
            // hata biçimi Program.cs'teki InvalidModelStateResponseFactory ile ayarlanır.
            try
            {
                var sonuc = _hesapService.ParaTransferi(request);

                if (sonuc.Basarili)
                {
                    return Ok(sonuc);
                }

                return BadRequest(sonuc);
            }
            catch (Exception ex)
            {
                return OracleHataYonet(this, ex, "Para transferi");
            }
        }
    }

    public class DurumGuncelleRequest
    {
        public int Durum { get; set; }
    }
}
