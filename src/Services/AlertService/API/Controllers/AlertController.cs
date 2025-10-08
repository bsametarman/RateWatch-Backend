using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RateWatch.AlertService.Application.DTOs;
using RateWatch.AlertService.Application.Services;
using System.Security.Claims;

namespace RateWatch.AlertService.API.Controllers
{
    [Authorize]
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
        [Authorize(Roles = "Admin")]
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
        public async Task<ActionResult<AlertDto?>> GetAlertByUserId()
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized();
            }

            var alert = await _alertService.GetAlertByUserIdAsync(userId.Value);

            if (alert != null)
            {
                return Ok(alert);
            }
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<AlertDto>> AddAlert(AlertForCreationDto alertForCreationDto)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized();
            }

            var alert = await _alertService.AddAlertAsync(alertForCreationDto with { UserId = userId.Value });
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
        private int? GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
