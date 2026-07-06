using PTEducation.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Azure;
using PTEducation.Data.DTO.ResponseModel;

namespace PTEducation.Data.Repositories.GenericRepositories
{
    public class GenericRepositories<T> : IGenericRepositories<T>, IDisposable where T : class
    {
        protected readonly PteducationContext context;
        protected readonly DbSet<T> dbSet;
        private bool disposed = false;

        public GenericRepositories(PteducationContext context)
        {
            this.context = context;
            dbSet = context.Set<T>();
        }

        private IQueryable<T> GetQueryable(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "",
            int? pageIndex = null,
            int? pageSize = null)
        {
            IQueryable<T> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split(
                new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty.Trim());
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (pageIndex.HasValue && pageSize.HasValue)
            {
                int validPageIndex = pageIndex.Value > 0 ? pageIndex.Value - 1 : 0;
                int validPageSize = pageSize.Value > 0 ? pageSize.Value : 10;

                query = query.Skip(validPageIndex * validPageSize).Take(validPageSize);
            }

            return query;
        }

        public async Task<IEnumerable<T>> GetList(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "",
            int? pageIndex = null,
            int? pageSize = null)
        {
            var query = GetQueryable(filter, orderBy, includeProperties, pageIndex, pageSize);
            return await query.ToListAsync();
        }

        public async Task<PagedListDataResultModel<T>> GetPagedList(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "",
            int pageIndex = 1,
            int pageSize = 10
        )
        {
            var query = GetQueryable(filter, orderBy, includeProperties);

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var data = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedListDataResultModel<T>
            {
                Data = data,
                TotalPages = totalPages,
                PageNumber = pageIndex,
                PageSize = pageSize
            };
        }

        public async Task<T> GetSingle(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "")
        {
            var query = GetQueryable(filter, orderBy, includeProperties, null, null);
            if (orderBy != null)
                query = orderBy(query);
            return await query.FirstOrDefaultAsync();
        }

        public async Task Insert(T entity, bool saveChanges = true)
        {
            await dbSet.AddAsync(entity);
            if (saveChanges)
                await context.SaveChangesAsync();
        }

        public async Task InsertRange(List<T> entity, bool saveChanges = true)
        {
            await dbSet.AddRangeAsync(entity);
            if (saveChanges)
                await context.SaveChangesAsync();
        }

        public async Task Update(T entity, bool saveChanges = true)
        {
            dbSet.Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
            if (saveChanges)
                await context.SaveChangesAsync();
        }

        public async Task UpdateRange(List<T> entities, bool saveChanges = true)
        {
            foreach (var entity in entities)
            {
                dbSet.Attach(entity);
                context.Entry(entity).State = EntityState.Modified;
            }
            if (saveChanges)
                await context.SaveChangesAsync();
        }

        public async Task Delete(T entity, bool saveChanges = true)
        {
            dbSet.Remove(entity);
            if (saveChanges)
                await context.SaveChangesAsync();
        }

        public async Task DeleteRange(List<T> entities, bool saveChanges = true)
        {
            dbSet.RemoveRange(entities);
            if (saveChanges)
                await context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }

        // Transaction helpers
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            await context.Database.CommitTransactionAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await context.Database.RollbackTransactionAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
                disposed = true;
            }
        }

        ~GenericRepositories()
        {
            Dispose(false);
        }
    }
}
