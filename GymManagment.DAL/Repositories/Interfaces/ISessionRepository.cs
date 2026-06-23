using GymManagment.DAL.Models;

namespace GymManagment.DAL.Repositories.Interfaces
{
    public interface ISessionRepository :IGenericRepository<Session>
    {
        Task<IEnumerable<Session>> GetAllSessionWithTrainerAndCategory(CancellationToken cancellationToken=default);
        

    }
}
