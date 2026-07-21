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
                return StatusCode(500, new { message = $"Hata oluştu: {ex.Message}" });
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
                return StatusCode(500, new { message = $"Hata oluştu: {ex.Message}" });
            }
        }
    }
}
