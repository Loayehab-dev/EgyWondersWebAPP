using EgyWonders.Data;
using EgyWonders.Interfaces;
using System.Collections;

namespace EgyWonders.Repository
{
    public class UnitOfWorkRepository : IUnitOfWork
    {
        private readonly TravelDbContext _context;
        private Hashtable _repsitories;
        public UnitOfWorkRepository(TravelDbContext context)
        {
            _context = context;
        }
        public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();
        public void Dispose() => _context.Dispose();
        public IGenericRepository<T> Repository<T>() where T : class
        {
            if (_repsitories == null)
                _repsitories = new Hashtable();
            var type = typeof(T).Name;
            if (!_repsitories.ContainsKey(type))
            {
                var repositoryType = typeof(GenericRepository<>);
                var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _context);
                _repsitories.Add(type, repositoryInstance);
            }
            return (IGenericRepository<T>)_repsitories[type];
        }

    }
}
