using System.Linq.Expressions;
using ItoCase.Core.Entities.Common;

namespace ItoCase.Core.Interfaces
{
    // Design Pattern 1: Generic Repository Pattern
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task AddRangeAsync(IEnumerable<T> entities); // Toplu ekleme (Excel için)
        Task<List<T>> GetAllAsync(); // Listeleme için
        IQueryable<T> Where(Expression<Func<T, bool>> expression); // Filtreleme için
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
    }
}