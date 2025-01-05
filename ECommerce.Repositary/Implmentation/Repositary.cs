using ECommerce.Data.DatabaseContext;
using ECommerce.Data.Entity;
using ECommerce.Repositary.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Repositary.Implmentation
{
    public class Repositary<T> : IRepositary<T> where T : class
    {
        private readonly ECommerceContext _context;

        public Repositary(ECommerceContext context)
        {
            _context = context;
        }

        public async Task AddAsync(T entity)
        {
             await _context.Set<T>().AddAsync(entity);
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>> filter,string? include = null, bool Istracked = false)
        {
            IQueryable<T> query;

            if (Istracked)
            {
                query = _context.Set<T>().Where(filter).AsTracking();
            }
            else
            {
                query = _context.Set<T>().Where(filter).AsNoTracking();
            }

            if (!string.IsNullOrEmpty(include))
            {
                foreach(var includeProp in include.Split(new char[] { ',' },StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return await query.FirstOrDefaultAsync();

        }

        public async Task<IEnumerable<T>> GetAllAsync(string? include = null)
        {
            IQueryable<T> query = _context.Set<T>();
            if (!string.IsNullOrEmpty(include))
            {
                foreach(var includeProp in include.Split(new char[] { ',' },StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            return await query.ToListAsync();
        }

        public void Remove(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _context.Set<T>().RemoveRange(entities);
        }

        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }

        public IEnumerable<T> GetAll(string? include = null)
        {
            IQueryable<T> query = _context.Set<T>();
            if (!string.IsNullOrEmpty(include))
            {
                foreach (var includeProp in include.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            return query.ToList();
        }

        public async Task<IEnumerable<T>> GetAllAsyncWithExpression(Expression<Func<T, bool>>? filter = null, string? include = null)
        {
            IQueryable<T> query = _context.Set<T>().Where(filter);
            if (!string.IsNullOrEmpty(include))
            {
                foreach (var includeProp in include.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            return await query.ToListAsync();
        }

        public IEnumerable<T> GetAllWithExpression(Expression<Func<T, bool>>? filter = null, string? include = null)
        {
            IQueryable<T> query = _context.Set<T>().Where(filter);
            if (!string.IsNullOrEmpty(include))
            {
                foreach (var includeProp in include.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            return  query.ToList();
        }
    }
}
