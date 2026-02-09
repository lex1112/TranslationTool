using Microsoft.EntityFrameworkCore;
using Translation.Domain.Entities;
using Translation.Infrastructure.Data;

namespace Translation.Infrastructure.Repositories
{
    public sealed class DbTranslationRepository : ITranslationRepository
    {
        private readonly ApplicationDbContext _context;

        public DbTranslationRepository(ApplicationDbContext context) => _context = context;

        // 1. Get all SIDs (Business Keys)
        public async Task<List<string>> GetAllSidsAsync()
            => await _context.TextResources
                .Select(r => r.Sid)
                .ToListAsync();

        // 2. Get the full Aggregate (Resource + Translations)
        public async Task<TextResourceEntity?> GetBySidAsync(string sid)
            => await _context.TextResources
                .Include(r => r.Translations) // Load child entities
                .FirstOrDefaultAsync(r => r.Sid == sid);

        // 3. Add a new Aggregate Root
        public async Task AddAsync(TextResourceEntity resource)
        {
            await _context.TextResources.AddAsync(resource);
        }

        // 4. Update via the Aggregate Root
        public async Task UpdateTranslationAsync(string sid, string langId, string text)
        {
            // In DDD, we always fetch the Aggregate Root first
            var resource = await GetBySidAsync(sid);

            if (resource != null)
            {
                // Use the DOMAIN method we created to ensure business rules
                resource.AddOrUpdateTranslation(langId, text);
            }
        }

        // 5. Delete the Aggregate Root
        public async Task DeleteBySidAsync(string sid)
        {
            var resource = await GetBySidAsync(sid);
            if (resource != null)
            {
                _context.TextResources.Remove(resource);
            }
        }

        // 6. Unit of Work: Persist all changes
        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _context.SaveChangesAsync(ct);
        }
    }


}
