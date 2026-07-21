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
                return Ok(new { message = "Müşteri başarıyla eklendi" });
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
                return Ok(new { message = "Müşteri başarıyla güncellendi" });
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