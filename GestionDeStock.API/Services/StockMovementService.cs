using Microsoft.EntityFrameworkCore;
using GestionDeStock.API.Data;
using GestionDeStock.API.Models;

namespace GestionDeStock.API.Services
{
    public class StockMovementService : IStockMovementService
    {
        private readonly AppDbContext _context;

        public StockMovementService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<StockMovement>> GetAllMovementsAsync()
        {
            return await _context.StockMovements
                .Include(sm => sm.Product)
                .OrderByDescending(sm => sm.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<StockMovement>> GetMovementsByProductIdAsync(int productId)
        {
            return await _context.StockMovements
                .Include(sm => sm.Product)
                .Where(sm => sm.ProductId == productId)
                .OrderByDescending(sm => sm.CreatedAt)
                .ToListAsync();
        }

        public async Task<StockMovement?> GetMovementByIdAsync(int id)
        {
            return await _context.StockMovements
                .Include(sm => sm.Product)
                .Include(sm => sm.RelatedMovement)
                .FirstOrDefaultAsync(sm => sm.Id == id);
        }

        public async Task<StockMovement> CreateMovementAsync(StockMovement movement)
        {
            _context.StockMovements.Add(movement);
            await _context.SaveChangesAsync();
            return movement;
        }
    }
}