using Microsoft.AspNetCore.Mvc;
using BankAPI.Models;
using BankAPI.Services;

namespace BankAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdresController : ControllerBase
    {
        private readonly AdresService _adresService;

        public AdresController(AdresService adresService)
        {
            _adresService = adresService;
        }

        [HttpPost]
        public IActionResult AdresEkle([FromBody] MusteriAdres adres)
        {
            try
            {
                _adresService.AdresEkle(adres);
                return Ok(new { message = "Adres başarıyla eklendi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpPut]
        public IActionResult AdresGuncelle([FromBody] MusteriAdres adres)
        {
            try
            {
                _adresService.AdresGuncelle(adres);
                return Ok(new { message = "Adres başarıyla güncellendi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult AdresSil(decimal id)
        {
            try
            {
                _adresService.AdresSil(id);
                return Ok(new { message = "Adres başarıyla silindi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpGet("musteri/{musteriId}")]
        public IActionResult MusteriAdresleriGetir(decimal musteriId)
        {
            try
            {
                var adresListesi = _adresService.MusteriAdresleriGetir(musteriId);
                return Ok(adresListesi);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Hata oluştu: {ex.Message}" });
            }
        }
    }
}
