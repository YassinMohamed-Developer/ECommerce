using ECommerce.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Repositary.Interface
{
    public interface IRepositary<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(string? include = null);
        Task<T> GetAsync(Expression<Func<T,bool>> filter,string? include = null,bool Istracked = false);

        IEnumerable<T> GetAll(string? include = null);
        Task AddAsync(T entity);
        void Update(T entity);

        void Remove(T entity);

        void RemoveRange(IEnumerable<T> entities);

        Task<IEnumerable<T>> GetAllAsyncWithExpression(Expression<Func<T, bool>>? filter = null, string? include = null);

        IEnumerable<T> GetAllWithExpression(Expression<Func<T, bool>>? filter = null, string? include = null);
    }
}
