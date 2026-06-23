using GymManagment.DAL.Models;

namespace GymManagment.DAL.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Member> Members { get; }
        IGenericRepository<HealthRecord> HealthRecords { get; }
        IGenericRepository<Plans> Plans { get; }
        IGenericRepository<Session> Sessions { get; }
        IGenericRepository<Booking> Bookings { get; }
        IGenericRepository<Category> Categories { get; }
        IGenericRepository<Membership> Memberships { get; }
        IGenericRepository<Trainer> Trainers { get; }
        public ISessionRepository SessionRepository { get; }

        Task<int> CompleteAsync(CancellationToken ct = default);
    }
}