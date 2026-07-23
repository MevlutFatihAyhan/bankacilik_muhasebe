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
                return StatusCode(500, new { message = $"Hata oluştu: {ex.Message}" });
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
                    return NotFound(new { message = "Hesap bulunamadı" });
                }
                return Ok(hesap);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Hata oluştu: {ex.Message}" });
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

    public class DurumGuncelleRequest
    {
        public int Durum { get; set; }
    }
}
