﻿using System.Linq.Expressions;

namespace Chibest.Repository.Base
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        IQueryable<TEntity> GetAll();

        Task<TEntity?> GetByIdAsync(object id);
        IQueryable<TEntity> GetByWhere(Expression<Func<TEntity, bool>> predicate);

        bool HasChanges(TEntity newEntity, TEntity trackedEntity);
        void Attach(TEntity entity);

        Task AddAsync(TEntity entity);

        void Update(TEntity entity);

        void Delete(TEntity entity);

        Task AddRangeAsync(IEnumerable<TEntity> entities);

        void UpdateRange(IEnumerable<TEntity> entities);

        void RemoveRange(IEnumerable<TEntity> entities);

        Task<int> CountAsync();

        Task<IEnumerable<TEntity>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null);
    }
}
