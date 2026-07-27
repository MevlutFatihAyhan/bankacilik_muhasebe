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

        // Filtreye bağlı listeleme — arayüzde "Uygula" butonuna basılmadan çağrılmaz
        [HttpGet("filtre")]
        public IActionResult HesaplariFiltrele(
            [FromQuery] string searchTerm = null,
            [FromQuery] int? musteriTipi = null,
            [FromQuery] string hesapTuru = null,
            [FromQuery] string dovizCinsi = null,
            [FromQuery] int? durum = null,
            [FromQuery] decimal? minBakiye = null,
            [FromQuery] decimal? maxBakiye = null)
        {
            try
            {
                var hesapListesi = _hesapService.HesaplariFiltrele(
                    searchTerm, musteriTipi, hesapTuru, dovizCinsi, durum, minBakiye, maxBakiye);
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

        [HttpGet("generate-iban")]
        public IActionResult GenerateIban([FromQuery] string accountNo = null)
        {
            var accNo = string.IsNullOrWhiteSpace(accountNo) ? BankAPI.Helpers.IbanHelper.GenerateAccountNo() : accountNo;
            var iban = BankAPI.Helpers.IbanHelper.GenerateTrIban(accNo);
            return Ok(new { hesapNo = accNo, iban = iban });
        }

        [HttpPost("transfer")]
        public IActionResult ParaTransferi([FromBody] ParaTransferiRequest request)
        {
            try
            {
                var sonuc = _hesapService.ParaTransferi(request.SenderIban, request.ReceiverIban, request.Amount, request.Description);
                
                if (sonuc.IslemKodu == "0")
                {
                    return Ok(new { message = sonuc.HataMesaji });
                }
                else
                {
                    return BadRequest(new { code = sonuc.IslemKodu, message = sonuc.HataMesaji });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Transfer sırasında hata oluştu: {ex.Message}" });
            }
        }
    }

    public class DurumGuncelleRequest
    {
        public int Durum { get; set; }
    }

    public class ParaTransferiRequest
    {
        public string SenderIban { get; set; }
        public string ReceiverIban { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }
}
