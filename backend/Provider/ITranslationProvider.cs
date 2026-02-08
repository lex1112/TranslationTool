using translation_app.Model;
using translation_app.Models;

namespace translation_app.Provider
{
    public interface ITranslationProvider
    {
        Task<List<string>> GetAllSidsAsync();
        Task<TextResource?> GetDetailsAsync(string sid);
        Task CreateSidAsync(string sid, string defaultText);
        Task UpdateTranslationAsync(string sid, string langId, string text);
        Task DeleteSidAsync(string sid);
    }
}
