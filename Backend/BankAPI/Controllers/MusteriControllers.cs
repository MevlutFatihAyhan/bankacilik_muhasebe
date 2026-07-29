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

        [HttpPost]
        public IActionResult MusteriEkle([FromBody] Musteri musteri){
            try{
                _musteriService.MusteriEkle(musteri);
                return Ok("Musteri basarıyla eklendi.");
            }catch(Exception ex){
                return StatusCode(500, new { message = $"Hata oluştu: {ex.Message}" });
            }
        }
        [HttpGet]
        public IActionResult MusterileriGetir(){
            try{
                var musteriListesi = _musteriService.MusterileriGetir();
                return Ok(musteriListesi);
            }catch(Exception ex){
                return StatusCode(500, new { message = $"Hata oluştu: {ex.Message}" });
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
                return StatusCode(500, new { message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public IActionResult MusteriGetir(decimal id){
            try{
                var musteri = _musteriService.MusteriGetir(id);
                if (musteri == null){
                    return NotFound(new { message = "Müşteri bulunamadı" });
                }
                return Ok(musteri);
            }catch(Exception ex){
                return StatusCode(500, new { message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpPut]
        public IActionResult MusteriGuncelleme([FromBody] Musteri musteri){
            try{
                _musteriService.MusteriGuncelleme(musteri);
                return Ok("Musteri basarıyla güncellendi.");
            }catch(Exception ex){
                return StatusCode(500, new { message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult MusteriSil(decimal id){
            try{
                _musteriService.MusteriSil(id);
                return Ok(new { message = "Müşteri başarıyla silindi" });
            }catch(Exception ex){
                return StatusCode(500, new { message = $"Hata oluştu: {ex.Message}" });
            }
        }
   }



}