using Microsoft.AspNetCore.Mvc;
using GestionDeStock.API.Services;
using GestionDeStock.API.Dtos;

namespace GestionDeStock.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController(IInventoryService inventoryService) : ControllerBase
    {
        private readonly IInventoryService _inventoryService = inventoryService;

        [HttpPost("sessions")]
        public async Task<IActionResult> CreateSession([FromBody] CreateInventorySessionDto dto)
        {
            try
            {
                var session = await _inventoryService.CreateSessionAsync(dto);
                return CreatedAtAction(nameof(GetSession), new { id = session.Id }, session);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("sessions/{id}")]
        public async Task<IActionResult> GetSession(int id)
        {
            var session = await _inventoryService.GetSessionByIdAsync(id);
            if (session == null)
                return NotFound();

            return Ok(session);
        }

        [HttpGet("sessions")]
        public async Task<IActionResult> GetAllSessions()
        {
            var sessions = await _inventoryService.GetAllSessionsAsync();
            return Ok(sessions);
        }

        [HttpPost("lines/{lineId}/count")]
        public async Task<IActionResult> RecordCount(int lineId, [FromBody] RecordCountDto dto)
        {
            try
            {
                var line = await _inventoryService.RecordCountAsync(lineId, dto);
                return Ok(line);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("sessions/{id}/validate")]
        public async Task<IActionResult> ValidateSession(int id, [FromBody] string userId)
        {
            try
            {
                var session = await _inventoryService.ValidateSessionAsync(id, userId);
                return Ok(session);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("sessions/{id}/pending-lines")]
        public async Task<IActionResult> GetPendingLines(int id)
        {
            var lines = await _inventoryService.GetPendingLinesAsync(id);
            return Ok(lines);
        }
    }
}