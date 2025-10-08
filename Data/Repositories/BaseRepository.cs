using MongoDB.Driver.Linq;
using RbacApi.Data.Evaluators;
using RbacApi.Data.Interfaces;

namespace RbacApi.Data.Repositories
{
    public static class BaseRepository<TEntity> where TEntity : class
    {
        public static async Task<int> CounAsync
            (IQueryable<TEntity> query, ISpecification<TEntity> spec)
        {
            return await ApplySpecification(query, spec).CountAsync();
        }

        public static async Task<IEnumerable<TEntity>> GetAllBySpecAsync
            (IQueryable<TEntity> query, ISpecification<TEntity> spec)
        {
            return await ApplySpecification(query, spec).ToListAsync();
        }

        private static IQueryable<TEntity> ApplySpecification
            (IQueryable<TEntity> inputQuery, ISpecification<TEntity> spec)
        {
            return SpecificationEvaluator<TEntity>.GetQuery(inputQuery, spec);
        }
    }
}
