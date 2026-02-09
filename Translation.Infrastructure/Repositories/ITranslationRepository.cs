using Translation.Domain.Entities;

namespace Translation.Infrastructure.Repositories
{
    public interface ITranslationRepository
    {
        // Queries
        Task<List<string>> GetAllSidsAsync();

        // Fetching the Aggregate Root with its children
        Task<TextResourceEntity?> GetBySidAsync(string sid);

        // Commands (Working with the Entity directly)
        Task AddAsync(TextResourceEntity resource);

        Task DeleteBySidAsync(string sid);

        // Unit of Work: Persists all changes to Postgres
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }

}
