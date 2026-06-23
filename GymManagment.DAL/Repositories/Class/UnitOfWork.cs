using GymManagment.DAL.DbContext;
using GymManagment.DAL.Models;
using GymManagment.DAL.Repositories.Interfaces;

namespace GymManagment.DAL.Repositories.Class
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly GymDbcontext _context;

        private IGenericRepository<Member>? _members;
        private IGenericRepository<HealthRecord>? _healthRecords;
        private IGenericRepository<Plans>? _plans;
        private IGenericRepository<Session>? _sessions;
        private IGenericRepository<Booking>? _bookings;
        private IGenericRepository<Category>? _categories;
        private IGenericRepository<Membership>? _memberships;
        private IGenericRepository<Trainer>? _trainers;

        public UnitOfWork(GymDbcontext context, ISessionRepository sessionRepository)
        {
            _context = context;
            SessionRepository = sessionRepository;
        }

        public IGenericRepository<Member> Members =>
            _members ??= new GenericRepository<Member>(_context);

        public IGenericRepository<HealthRecord> HealthRecords =>
            _healthRecords ??= new GenericRepository<HealthRecord>(_context);

        public IGenericRepository<Plans> Plans =>
            _plans ??= new GenericRepository<Plans>(_context);

        public IGenericRepository<Session> Sessions =>
            _sessions ??= new GenericRepository<Session>(_context);

        public IGenericRepository<Booking> Bookings =>
            _bookings ??= new GenericRepository<Booking>(_context);

        public IGenericRepository<Category> Categories =>
            _categories ??= new GenericRepository<Category>(_context);

        public IGenericRepository<Membership> Memberships =>
            _memberships ??= new GenericRepository<Membership>(_context);

        public IGenericRepository<Trainer> Trainers =>
            _trainers ??= new GenericRepository<Trainer>(_context);

        public ISessionRepository SessionRepository {  get;  }

        public async Task<int> CompleteAsync(CancellationToken ct = default) =>
            await _context.SaveChangesAsync(ct);

        public void Dispose() => _context.Dispose();
    }
}