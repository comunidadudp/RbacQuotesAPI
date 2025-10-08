using RbacApi.Data.Interfaces;
using System.Linq.Expressions;

namespace RbacApi.Data.Specifications
{
    public abstract class Specification<TEntity> : ISpecification<TEntity>
        where TEntity : class
    {
        public Expression<Func<TEntity, bool>> Criteria { get; private set; } = _ => true;

        public Expression<Func<TEntity, object>>? OrderBy { get; private set; }

        public Expression<Func<TEntity, object>>? OrderByDescending { get; private set; }

        public int Take { get; private set; }

        public int Skip { get; private set; }

        public bool IsPagingEnabled { get; private set; }


        public void AndAlso(Expression<Func<TEntity, bool>> criteria)
        {
            Criteria = SpecificationMethods<TEntity>.AndAlso(criteria, Criteria);
        }

        public void AddOrderBy(Expression<Func<TEntity, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
        }

        public void AddOrderByDescending(Expression<Func<TEntity, object>> orderByDescExpression)
        {
            OrderByDescending = orderByDescExpression;
        }

        public void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
        }
    }
}
