using GymManagment.DAL.Models;
using System.Linq.Expressions;

namespace GymManagment.DAL.Repositories.Interfaces
{
    public interface IGenericRepository<TEntity> where TEntity : BaseEntity , new()
    {
        Task<IEnumerable<TEntity>> GetAllAsync(bool tracking = false, CancellationToken ct = default);
        Task<TEntity?> GetByIdAsync(int id, CancellationToken ct);

        Task<int> AddAsync(TEntity Entity, CancellationToken ct);
        Task<int> DeleteAsync(TEntity Entity, CancellationToken ct);
        Task<int> UpdateAsync(TEntity Entity, CancellationToken ct);
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> Predicate, CancellationToken ct = default); 
        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> Predicate,bool tracking=false, CancellationToken ct = default); 
        Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default, params Expression<Func<TEntity, object>>[] includes);
        Task<IEnumerable<TEntity>> GetAllAsync(bool tracking = false,CancellationToken ct = default,params Expression<Func<TEntity, object>>[] includes);
    }
}
