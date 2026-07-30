using Microsoft.AspNetCore.Mvc;
using BankAPI.Models;
using BankAPI.Services;

namespace BankAPI.Controllers{

    [ApiController]
    [Route("api/[controller]")]
    public class MusteriController : ControllerBase{
        private readonly MusteriService _musteriService;
        public MusteriController(MusteriService musteriService){
            _musteriService = musteriService;
        }

        // -------------------------------------------------------
        // Ortak yardımcı: Oracle hata mesajını Türkçe'ye çevirir.
        // -------------------------------------------------------
        private static IActionResult OracleHataYonet(ControllerBase ctrl, Exception ex, string islem = "işlem")
        {
            string msg = ex.Message;

            // --- Benzersizlik ihlalleri ---
            if (msg.Contains("ORA-20001") || msg.Contains("zaten var"))
                return ctrl.BadRequest(new { message = "Bu E-posta veya TCKN/VKN ile kayıtlı bir müşteri zaten mevcut!" });

            if (msg.Contains("UQ_MUSTERI_EMAIL") || msg.Contains("ORA-00001") && msg.Contains("EMAIL"))
                return ctrl.Conflict(new { message = "Bu E-posta adresi başka bir müşteride zaten kullanılıyor!" });

            if (msg.Contains("UQ_MUSTERI_KIMLIK_NO") || msg.Contains("ORA-00001") && msg.Contains("KIMLIK"))
                return ctrl.Conflict(new { message = "Bu TCKN/VKN numarası başka bir müşteride zaten kayıtlı!" });

            // --- Check kısıtlamaları ---
            if (msg.Contains("CHK_MUSTERI_TELEFON"))
                return ctrl.BadRequest(new { message = "Telefon numarası sadece rakamlardan oluşmalı ve 10-15 hane olmalıdır!" });

            if (msg.Contains("CHK_MUSTERI_EMAIL_FORMAT"))
                return ctrl.BadRequest(new { message = "Geçerli bir e-posta adresi biçimi giriniz! (örn: kullanici@domain.com)" });

            if (msg.Contains("CHK_MUSTERI_SOYAD"))
                return ctrl.BadRequest(new { message = "Bireysel müşteri eklerken soyad alanı zorunludur!" });

            if (msg.Contains("CHK_MUSTERI_KIMLIK_NO"))
                return ctrl.BadRequest(new { message = "TCKN 11 haneli, VKN ise yalnızca rakamlardan oluşan sayısal bir değer olmalıdır!" });

            if (msg.Contains("CHK_MUSTERI_AKTIF"))
                return ctrl.BadRequest(new { message = "Durum değeri yalnızca 1 (Aktif) veya 2 (Pasif) olabilir!" });

            // --- Sütun uzunluğu aşımı ---
            if (msg.Contains("ORA-12899") && msg.Contains("KIMLIK_NO"))
                return ctrl.BadRequest(new { message = "Kimlik numarası çok uzun! TCKN 11, VKN en fazla 10 hane olmalıdır." });

            if (msg.Contains("ORA-12899") && msg.Contains("EMAIL"))
                return ctrl.BadRequest(new { message = "E-posta adresi en fazla 150 karakter olabilir!" });

            if (msg.Contains("ORA-12899") && msg.Contains("TELEFON"))
                return ctrl.BadRequest(new { message = "Telefon numarası en fazla 20 karakter olabilir!" });

            // --- FK / NOT NULL ihlalleri ---
            if (msg.Contains("ORA-01400"))
                return ctrl.BadRequest(new { message = $"Zorunlu bir alan boş bırakıldı. Lütfen tüm gerekli alanları doldurunuz." });

            // --- Müşteri bulunamadı (UPDATE/DELETE için) ---
            if (msg.Contains("ORA-20002") || msg.Contains("Musteri bulunamadi") || msg.Contains("musteri bulunamadi"))
                return ctrl.NotFound(new { message = "Güncellenmek veya silinmek istenen müşteri sistemde bulunamadı!" });

            // --- Bağlantı sorunları ---
            if (msg.Contains("ORA-12541") || msg.Contains("ORA-12170") || msg.Contains("ORA-12154"))
                return ctrl.StatusCode(503, new { message = "Veritabanına bağlanılamadı. Lütfen daha sonra tekrar deneyiniz." });

            // --- Genel / bilinmeyen ---
            return ctrl.StatusCode(500, new { message = $"{islem} sırasında beklenmedik bir hata oluştu: {ex.Message}" });
        }

        [HttpPost]
        public IActionResult MusteriEkle([FromBody] Musteri musteri){
            try{
                _musteriService.MusteriEkle(musteri);
                return Ok(new { message = "Müşteri başarıyla eklendi." });
            }catch(Exception ex){
                return OracleHataYonet(this, ex, "Müşteri ekleme");
            }
        }

        [HttpGet]
        public IActionResult MusterileriGetir(){
            try{
                var musteriListesi = _musteriService.MusterileriGetir();
                return Ok(musteriListesi);
            }catch(Exception ex){
                return OracleHataYonet(this, ex, "Müşteri listesi");
            }
        }

        [HttpGet("ozet")]
        public IActionResult MusteriOzetGetir(){
            try{
                var ozetListesi = _musteriService.MusteriOzetGetir();
                return Ok(ozetListesi);
            }catch(Exception ex){
                return OracleHataYonet(this, ex, "Müşteri özet listesi");
            }
        }

        // Filtreye bağlı listeleme — arayüzde "Uygula" butonuna basılmadan çağrılmaz
        [HttpGet("filtre")]
        public IActionResult MusterileriFiltrele(
            [FromQuery] string searchTerm = null,
            [FromQuery] int? musteriTipi = null,
            [FromQuery] int? aktifMi = null,
            [FromQuery] string ad = null,
            [FromQuery] string soyad = null){
            try{
                var musteriListesi = _musteriService.MusterileriFiltrele(
                    searchTerm, musteriTipi, aktifMi, ad, soyad);
                return Ok(musteriListesi);
            }catch(Exception ex){
                return OracleHataYonet(this, ex, "Müşteri filtreleme");
            }
        }

        [HttpGet("{id}")]
        public IActionResult MusteriGetir(decimal id){
            try{
                var musteri = _musteriService.MusteriGetir(id);
                if (musteri == null){
                    return NotFound(new { message = $"ID {id} ile kayıtlı müşteri bulunamadı." });
                }
                return Ok(musteri);
            }catch(Exception ex){
                return OracleHataYonet(this, ex, "Müşteri getirme");
            }
        }

        [HttpPut]
        public IActionResult MusteriGuncelleme([FromBody] Musteri musteri){
            try{
                _musteriService.MusteriGuncelleme(musteri);
                return Ok(new { message = "Müşteri başarıyla güncellendi." });
            }catch(Exception ex){
                return OracleHataYonet(this, ex, "Müşteri güncelleme");
            }
        }

        [HttpDelete("{id}")]
        public IActionResult MusteriSil(decimal id){
            try{
                _musteriService.MusteriSil(id);
                return Ok(new { message = "Müşteri başarıyla silindi." });
            }catch(Exception ex){
                return OracleHataYonet(this, ex, "Müşteri silme");
            }
        }
   }
}