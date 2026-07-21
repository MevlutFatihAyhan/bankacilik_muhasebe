using Microsoft.AspNetCore.Mvc;
using BankAPI.Services;

namespace BankAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService _dashboardService;

        public DashboardController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // Dashboard özet istatistikleri — PKG_DASHBOARD.PRC_GET_SUMMARY
        [HttpGet("summary")]
        public IActionResult GetSummary()
        {
            try
            {
                var summary = _dashboardService.GetSummary();
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Hata oluştu: {ex.Message}" });
            }
        }
    }
}
