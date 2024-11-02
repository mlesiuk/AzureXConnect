using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace AzureXConnect.Repositories;

public interface IRepository;

public abstract class BaseRepository
{
    public static async IAsyncEnumerable<TEntity> ReadItemsAsync<TEntity>(
        Container container,
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
}
