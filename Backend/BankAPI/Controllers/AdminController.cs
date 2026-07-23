using Microsoft.AspNetCore.Mvc;
using BankAPI.Models;
using BankAPI.Services;
using System.Collections.Generic;
using System.Linq; // LINQ sorguları için gerekli (FirstOrDefault kullanacağız)

namespace BankAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AdminService _adminService;

        public AdminController(AdminService adminService)
        {
            _adminService = adminService;
        }

        // 1. Zaten var olan AdminleriGetir metodunu çağıran GET isteği
        [HttpGet("getall")]
        public ActionResult<List<Admin>> AdminleriGetir()
        {
            var adminler = _adminService.AdminleriGetir();

            if (adminler == null || adminler.Count == 0)
            {
                return NotFound("Sistemde kayıtlı admin bulunamadı.");
            }

            return Ok(adminler);
        }

        // 2. MEYCUT Admin MODELİNİ KULLANAN LOGIN ENDPOINT'İ
        [HttpPost("login")]
        public IActionResult Login([FromBody] Admin girisYapanAdmin)
        {
            // İstemciden gelen veri boş mu kontrolü
            if (girisYapanAdmin == null || string.IsNullOrEmpty(girisYapanAdmin.AdminKullaniciAdi) || string.IsNullOrEmpty(girisYapanAdmin.AdminSifre))
            {
                return BadRequest("Kullanıcı adı ve şifre boş olamaz.");
            }

            // MEYCUT SERVİS METODUNU ÇAĞIRIYORUZ: Tüm listeyi çek
            List<Admin> tumAdminler = _adminService.AdminleriGetir();

            // C# LINQ ile liste içinde kullanıcı adı ve şifre eşleşen var mı diye bakıyoruz
            Admin eslesenAdmin = tumAdminler.FirstOrDefault(a => 
                a.AdminKullaniciAdi == girisYapanAdmin.AdminKullaniciAdi && 
                a.AdminSifre == girisYapanAdmin.AdminSifre
            );

            // Eşleşen kayıt yoksa (Giriş Başarısız)
            if (eslesenAdmin == null)
            {
                return Unauthorized("Kullanıcı adı veya şifre hatalı!");
            }

            // Eşleşen kayıt varsa (Giriş Başarılı)
            return Ok(new 
            { 
                Message = "Giriş başarılı!", 
                AdminId = eslesenAdmin.AdminId,
                KullaniciAdi = eslesenAdmin.AdminKullaniciAdi
            });
        }
    }
}