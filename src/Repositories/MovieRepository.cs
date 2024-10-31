using AzureXConnect.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace AzureXConnect.Repositories;

public interface IMovieRepository : IRepository
{
    Task<Movie?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
}

public class MovieRepository(Container container) : IMovieRepository
{
    private async IAsyncEnumerable<TEntity> ReadItemsAsync<TEntity>(
        Expression<Func<TEntity, bool>> predicate,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var feedIterator = container
            .GetItemLinqQueryable<TEntity>()
            .Where(predicate)
            .ToFeedIterator();

        while (feedIterator.HasMoreResults)
        {
            foreach (var item in await feedIterator.ReadNextAsync(cancellationToken))
            {
                yield return item;
            }
        }
    }

    public async Task<Movie?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _ = Guid.TryParse(id, out var movieId);
        var items = await ReadItemsAsync<Movie>(tr => tr.Id.Equals(movieId), cancellationToken)
            .ToListAsync(cancellationToken) ?? [];

        return items?.FirstOrDefault();
    }
}
