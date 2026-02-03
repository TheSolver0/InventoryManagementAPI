using GestionDeStock.API.Models;

namespace GestionDeStock.API.Services
{
    public interface IStockMovementService
    {
        Task<List<StockMovement>> GetAllMovementsAsync();
        Task<List<StockMovement>> GetMovementsByProductIdAsync(int productId);
        Task<StockMovement?> GetMovementByIdAsync(int id);
        Task<StockMovement> CreateMovementAsync(StockMovement movement);
    }
}