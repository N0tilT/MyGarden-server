using Microsoft.EntityFrameworkCore;

namespace AssistantAPI.Service
{
    public interface IHasUserIdService<TSource> : IDataEntityService<TSource> where TSource : IdentifiableEntity
    {
        public Task<List<TSource>> Get(DbSet<TSource> dbSet, string userId, List<int>? ids = null);
    }
}
