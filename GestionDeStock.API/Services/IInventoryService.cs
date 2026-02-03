using Microsoft.EntityFrameworkCore;
using GestionDeStock.API.Data;
using GestionDeStock.API.Models;
using GestionDeStock.API.Dtos;

namespace GestionDeStock.API.Services
{
    public interface IInventoryService
    {
        Task<InventorySession> CreateSessionAsync(CreateInventorySessionDto dto);
        Task<InventorySession?> GetSessionByIdAsync(int sessionId);
        Task<InventoryLine> RecordCountAsync(int lineId, RecordCountDto dto);
        Task<InventorySession> ValidateSessionAsync(int sessionId, string userId);
        Task<List<InventoryLine>> GetPendingLinesAsync(int sessionId);
        Task<List<InventorySession>> GetAllSessionsAsync();
    }
}