using Microsoft.EntityFrameworkCore.Storage;
using PTEducation.Data.DTO.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PTEducation.Data.Repositories.GenericRepositories
{
    public interface IGenericRepositories<T> where T : class
    {
        Task<IEnumerable<T>> GetList(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "",
            int? pageIndex = null,
            int? pageSize = null);

        Task<PagedListDataResultModel<T>> GetPagedList(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "",
            int pageIndex = 1,
            int pageSize = 10
        );

        Task<T> GetSingle(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "");

        Task Insert(T entity, bool saveChanges = true);

        Task InsertRange(List<T> entity, bool saveChanges = true);

        Task Update(T entity, bool saveChanges = true);

        Task UpdateRange(List<T> entities, bool saveChanges = true);

        Task Delete(T entity, bool saveChanges = true);

        Task DeleteRange(List<T> entities, bool saveChanges = true);

        Task SaveChangesAsync();

        // Transaction helpers
        Task<IDbContextTransaction> BeginTransactionAsync();

        Task CommitTransactionAsync();

        Task RollbackTransactionAsync();
    }
}
