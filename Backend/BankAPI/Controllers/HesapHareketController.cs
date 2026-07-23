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
                return StatusCode(500, new { message = $"Hata oluştu: {ex.Message}" });
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
                return StatusCode(500, new { message = $"Hata oluştu: {ex.Message}" });
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
                return StatusCode(500, new { message = $"Hata oluştu: {ex.Message}" });
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
                    return NotFound(new { message = "Hareket bulunamadı" });
                }
                return Ok(hareket);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Hata oluştu: {ex.Message}" });
            }
        }
    }
}
