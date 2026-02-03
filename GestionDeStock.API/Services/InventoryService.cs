using Microsoft.EntityFrameworkCore;
using GestionDeStock.API.Data;
using GestionDeStock.API.Models;
using GestionDeStock.API.Dtos;

namespace GestionDeStock.API.Services
{
    public class InventoryService(AppDbContext context) : IInventoryService
    {
        private readonly AppDbContext _context = context;

        public async Task<InventorySession> CreateSessionAsync(CreateInventorySessionDto dto)
        {
            var session = new InventorySession
            {
                Reference = GenerateReference(),
                CreatedDate = DateTime.UtcNow,
                Type = dto.Type,
                Status = InventoryStatus.InProgress,
                CreatedBy = dto.UserId,
                Notes = dto.Notes
            };

            // Récupérer les produits selon le périmètre
            var query = _context.Products.AsQueryable();

            if (dto.CategoryIds?.Any() == true)
                query = query.Where(p => dto.CategoryIds.Contains(p.CategoryId));

            var products = await query.ToListAsync();

            // Créer les lignes d'inventaire
            foreach (var product in products)
            {
                session.Lines.Add(new InventoryLine
                {
                    ProductId = product.Id,
                    ProductSku = product.Sku,
                    ProductName = product.Name,
                    Location = product.Location,
                    TheoreticalQuantity = product.Quantity
                });
            }

            _context.InventorySessions.Add(session);
            await _context.SaveChangesAsync();

            return session;
        }

        public async Task<InventorySession?> GetSessionByIdAsync(int sessionId)
        {
            return await _context.InventorySessions
                .Include(s => s.Lines)
                .ThenInclude(l => l.Product)
                .FirstOrDefaultAsync(s => s.Id == sessionId);
        }

        public async Task<InventoryLine> RecordCountAsync(int lineId, RecordCountDto dto)
        {
            var line = await _context.InventoryLines
                .Include(l => l.Session)
                .FirstOrDefaultAsync(l => l.Id == lineId);

            if (line == null)
                throw new KeyNotFoundException("Ligne d'inventaire non trouvée");

            if (line.Session.Status != InventoryStatus.InProgress)
                throw new InvalidOperationException("L'inventaire n'est plus en cours");

            line.CountedQuantity = dto.CountedQuantity;
            line.CountedAt = DateTime.UtcNow;
            line.CountedBy = dto.UserId;
            line.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            return line;
        }

        public async Task<InventorySession> ValidateSessionAsync(int sessionId, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var session = await _context.InventorySessions
                    .Include(s => s.Lines)
                    .ThenInclude(l => l.Product)
                    .FirstOrDefaultAsync(s => s.Id == sessionId);

                if (session == null)
                    throw new KeyNotFoundException("Session non trouvée");

                // Vérifications...
                var uncountedLines = session.Lines.Where(l => !l.CountedQuantity.HasValue).ToList();
                if (uncountedLines.Any())
                    throw new InvalidOperationException($"{uncountedLines.Count} lignes non comptées");

                // Pour chaque écart, créer un StockMovement ET ajuster le stock
                foreach (var line in session.Lines.Where(l => l.Variance != 0))
                {
                    var product = line.Product;
                    product.Quantity += (int)line.Variance;

                    // Créer le mouvement de stock (traçabilité)
                    _context.StockMovements.Add(new StockMovement
                    {
                        Type = StockMovementTypes.Adjustment,
                        Quantity = (int)line.Variance,
                        Reason = $"Ajustement inventaire {session.Reference}",
                        Reference = $"{session.Reference}-L{line.Id}",
                        ProductId = product.Id,
                        CreatedBy = userId,
                        RelatedMovementId = null // Pas de transaction commerciale
                    });

                    // PAS de Movement commercial car pas d'argent impliqué
                }

                session.Status = InventoryStatus.Validated;
                session.ValidatedDate = DateTime.UtcNow;
                session.ValidatedBy = userId;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return session;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<List<InventoryLine>> GetPendingLinesAsync(int sessionId)
        {
            return await _context.InventoryLines
                .Where(l => l.InventorySessionId == sessionId && !l.CountedQuantity.HasValue)
                .Include(l => l.Product)
                .ToListAsync();
        }

        public async Task<List<InventorySession>> GetAllSessionsAsync()
        {
            return await _context.InventorySessions
                .Include(s => s.Lines)
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();
        }

        private string GenerateReference()
        {
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var random = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
            return $"INV-{date}-{random}";
        }
    }
}
