using Microsoft.EntityFrameworkCore;
using translation_app.Model;
using translation_app.Models;

namespace translation_app.Provider
{
    public class DbTranslationProvider : ITranslationProvider
    {
        private readonly ApplicationDbContext _context;

        public DbTranslationProvider(ApplicationDbContext context) => _context = context;

        public async Task<List<string>> GetAllSidsAsync()
            => await _context.TextResources.Select(r => r.Sid).ToListAsync();

        public async Task<TextResource?> GetDetailsAsync(string sid)
            => await _context.TextResources
                .Include(r => r.Translations)
                .FirstOrDefaultAsync(r => r.Sid == sid);

        public async Task CreateSidAsync(string sid, string defaultText)
        {
            var resource = new TextResource { Sid = sid };
            resource.Translations.Add(new Translation { LangId = "default", Text = defaultText });
            _context.TextResources.Add(resource);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTranslationAsync(string sid, string langId, string text)
        {
            var translation = await _context.Translations
                .FirstOrDefaultAsync(t => t.Sid == sid && t.LangId == langId);

            if (translation != null)
            {
                translation.Text = text;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteSidAsync(string sid)
        {
            var resource = await _context.TextResources.FindAsync(sid);
            if (resource != null)
            {
                _context.TextResources.Remove(resource);
                await _context.SaveChangesAsync();
            }
        }
    }

}
