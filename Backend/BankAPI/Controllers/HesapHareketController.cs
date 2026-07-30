using Microsoft.AspNetCore.Mvc;
using BankAPI.Models;
using BankAPI.Services;

namespace BankAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HesapHareketController : ControllerBase
    {
        private readonly HesapHareketService _hareketService;

        public HesapHareketController(HesapHareketService hareketService)
        {
            _hareketService = hareketService;
        }

        // -------------------------------------------------------
        // Ortak yardımcı: Oracle hata mesajını Türkçe'ye çevirir.
        // -------------------------------------------------------
        private static IActionResult OracleHataYonet(ControllerBase ctrl, Exception ex, string islem = "işlem")
        {
            string msg = ex.Message;

            // --- FK ihlalleri ---
            if (msg.Contains("ORA-02291") && msg.Contains("HESAP_NO"))
                return ctrl.BadRequest(new { message = "Belirtilen hesap numarası sistemde kayıtlı değil. Lütfen geçerli bir hesap seçin!" });

            if (msg.Contains("ORA-02291") && msg.Contains("MUSTERI"))
                return ctrl.BadRequest(new { message = "Belirtilen müşteri sistemde kayıtlı değil!" });

            // --- Check kısıtlamaları ---
            if (msg.Contains("CHK_HAREKET_ISLEM_YONU") || msg.Contains("ORA-02290") && msg.Contains("ISLEM_YONU"))
                return ctrl.BadRequest(new { message = "Geçersiz işlem yönü! 'GİREN' veya 'ÇIKAN' değerlerinden biri olmalıdır." });

            if (msg.Contains("CHK_HAREKET_TUTAR") || msg.Contains("ORA-02290") && msg.Contains("TUTAR"))
                return ctrl.BadRequest(new { message = "İşlem tutarı sıfırdan büyük olmalıdır!" });

            if (msg.Contains("CHK_HAREKET_DOVIZ") || msg.Contains("ORA-02290") && msg.Contains("DOVIZ"))
                return ctrl.BadRequest(new { message = "Geçersiz döviz cinsi! TRY, USD, EUR veya XAU (Altın) değerlerinden biri olmalıdır." });

            // --- Hesap bakiye yetersiz (stored procedure'den gelen) ---
            if (msg.Contains("ORA-20010") || msg.Contains("bakiye yetersiz") || msg.Contains("Bakiye yetersiz"))
                return ctrl.BadRequest(new { message = "İşlem başarısız: Hesabın mevcut bakiyesi bu işlem için yetersiz!" });

            // --- Hesap pasif / dondurulmuş ---
            if (msg.Contains("ORA-20013") || msg.Contains("hesap aktif degil") || msg.Contains("Hesap aktif değil"))
                return ctrl.BadRequest(new { message = "İşlem başarısız: Hesap aktif durumda değil. Pasif veya dondurulmuş hesaplara işlem yapılamaz!" });

            // --- Hareket bulunamadı ---
            if (msg.Contains("ORA-20020") || msg.Contains("hareket bulunamadi") || msg.Contains("Hareket bulunamadi"))
                return ctrl.NotFound(new { message = "Belirtilen işlem kaydı sistemde bulunamadı!" });

            // --- NOT NULL ihlali ---
            if (msg.Contains("ORA-01400"))
                return ctrl.BadRequest(new { message = "Zorunlu bir alan boş bırakıldı. Lütfen tüm gerekli alanları doldurunuz." });

            // --- Tarih aralığı hatası (filtre) ---
            if (msg.Contains("ORA-01858") || msg.Contains("ORA-01843"))
                return ctrl.BadRequest(new { message = "Geçersiz tarih formatı! Tarih alanları 'GG.AA.YYYY' biçiminde olmalıdır." });

            // --- Bağlantı sorunları ---
            if (msg.Contains("ORA-12541") || msg.Contains("ORA-12170") || msg.Contains("ORA-12154"))
                return ctrl.StatusCode(503, new { message = "Veritabanına bağlanılamadı. Lütfen daha sonra tekrar deneyiniz." });

            // --- Genel / bilinmeyen ---
            return ctrl.StatusCode(500, new { message = $"{islem} sırasında beklenmedik bir hata oluştu: {ex.Message}" });
        }

        [HttpPost]
        public IActionResult HareketEkle([FromBody] HesapHareket hareket)
        {
            try
            {
                _hareketService.HareketEkle(hareket);
                return Ok(new { message = "İşlem başarıyla kaydedildi" });
            }
            catch (Exception ex)
            {
                return OracleHataYonet(this, ex, "Hareket ekleme");
            }
        }

        [HttpGet]
        public IActionResult TumHareketleriGetir()
        {
            try
            {
                var hareketListesi = _hareketService.TumHareketleriGetir();
                return Ok(hareketListesi);
            }
            catch (Exception ex)
            {
                return OracleHataYonet(this, ex, "Hareket listeleme");
            }
        }

        // Filtreye bağlı listeleme — arayüzde "Uygula" butonuna basılmadan çağrılmaz
        [HttpGet("filtre")]
        public IActionResult HareketleriFiltrele(
            [FromQuery] string searchTerm = null,
            [FromQuery] string islemYonu = null,
            [FromQuery] string dovizCinsi = null,
            [FromQuery] DateTime? baslangicTarihi = null,
            [FromQuery] DateTime? bitisTarihi = null,
            [FromQuery] decimal? minTutar = null,
            [FromQuery] decimal? maxTutar = null,
            [FromQuery] string hesapNo = null,
            [FromQuery] string musteriAdi = null,
            [FromQuery] string musteriSoyadi = null)
        {
            try
            {
                var hareketListesi = _hareketService.HareketleriFiltrele(
                    searchTerm, islemYonu, dovizCinsi, baslangicTarihi, bitisTarihi, minTutar, maxTutar,
                    hesapNo, musteriAdi, musteriSoyadi);
                return Ok(hareketListesi);
            }
            catch (Exception ex)
            {
                return OracleHataYonet(this, ex, "Hareket filtreleme");
            }
        }

        [HttpGet("hesap/{hesapNo}")]
        public IActionResult HesapHareketleriGetir(string hesapNo)
        {
            try
            {
                var hareketListesi = _hareketService.HesapHareketleriGetir(hesapNo);
                return Ok(hareketListesi);
            }
            catch (Exception ex)
            {
                return OracleHataYonet(this, ex, "Hesap hareketleri getirme");
            }
        }

        [HttpGet("{islemId}")]
        public IActionResult HareketGetir(decimal islemId)
        {
            try
            {
                var hareket = _hareketService.HareketGetir(islemId);
                if (hareket == null)
                {
                    return NotFound(new { message = $"İşlem ID {islemId} ile eşleşen bir hareket kaydı bulunamadı." });
                }
                return Ok(hareket);
            }
            catch (Exception ex)
            {
                return OracleHataYonet(this, ex, "Hareket getirme");
            }
        }
    }
}
