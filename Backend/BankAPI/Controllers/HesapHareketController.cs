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

        [HttpGet("{hesapNo}")]
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
    }
}
