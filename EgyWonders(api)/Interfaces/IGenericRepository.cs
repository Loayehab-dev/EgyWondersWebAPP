using System.Linq.Expressions;
namespace EgyWonders.Interfaces
{
    public interface  IGenericRepository<T> where T : class
    {
        
        //filter optional ex: x=>x.id=5 maaped to where clause
        // includes optional ex: x=>x.Category, x=>x.ProductType mapped to join clause
        Task<IEnumerable<T>>GetAllAsync(Expression<Func<T,bool>>filter=null,params Expression<Func<T, object>>[]includes
            );
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T> GetByIdAsync(int id);
        void Add(T entity);
        void Remove(T entity);
        void Update(T entity);
    }
}
