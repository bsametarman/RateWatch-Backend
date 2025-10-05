using Microsoft.AspNetCore.Mvc;
using RateWatch.AlertService.Application.DTOs;
using RateWatch.AlertService.Application.Services;

namespace RateWatch.AlertService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertController : Controller
    {
        private readonly IAlertService _alertService;

        public AlertController(IAlertService alertService)
        {
            _alertService = alertService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AlertDto?>> GetAlertById(int id)
        {
            var alert = await _alertService.GetAlertByIdAsync(id);

            if(alert != null)
            {
                return Ok(alert);
            }
            return NoContent();
        }

        [HttpGet("api/Alert/user-id/{userId}")]
        public async Task<ActionResult<AlertDto?>> GetAlertByUserId(int userId)
        {
            var alert = await _alertService.GetAlertByIdAsync(userId);

            if (alert != null)
            {
                return Ok(alert);
            }
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<AlertDto>> AddAlert(AlertForCreationDto alertForCreationDto)
        {
            var alert = await _alertService.AddAlertAsync(alertForCreationDto);
            return Ok(alert);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAlert(int id, AlertForUpdateDto alertForUpdateDto)
        {
            await _alertService.UpdateAlertAsync(id, alertForUpdateDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlert(int id)
        {
            await _alertService.DeleteAlertAsync(id);
            return NoContent();
        }
    }
}
