using AzureXConnect.Models;
using Microsoft.Azure.Cosmos;

namespace AzureXConnect.Repositories;

public interface IMovieRepository : IRepository
{
    Task<Movie?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
}

public class MovieRepository(Container container) : BaseRepository, IMovieRepository
{
    public async Task<Movie?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _ = Guid.TryParse(id, out var movieId);
        var items = await ReadItemsAsync<Movie>(container, tr => tr.Id.Equals(movieId), cancellationToken)
            .ToListAsync(cancellationToken) ?? [];

        return items?.FirstOrDefault();
    }
}
