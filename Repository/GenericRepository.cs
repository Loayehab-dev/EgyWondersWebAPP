using System.Linq.Expressions;
using EgyWonders.Data;
using EgyWonders.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace EgyWonders.Repository
{
    public class GenericRepository<T>:IGenericRepository<T> where T : class
    {
        protected readonly TravelDbContext _context;
        internal DbSet<T> dbSet;
        public GenericRepository(TravelDbContext context)
        {
            _context = context;
            this.dbSet = _context.Set<T>();
        }
        public async Task<T> GetByIdAsync(int id)=> await dbSet.FindAsync(id);
        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter = null,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter);

            }
            if (includes != null)
            {
                foreach (var includeProperty in includes)
                {
                    query = query.Include(includeProperty);
                }
            }
            return await query.ToListAsync();
        }
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)=>
            await dbSet.Where(predicate).ToListAsync();
        public void Add(T entity)=> dbSet.Add(entity);
        public void Remove(T entity)=> dbSet.Remove(entity);
        public void Update(T entity)=> dbSet.Update(entity);

    }
}
