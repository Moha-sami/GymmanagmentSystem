using GymManagment.DAL.DbContext;
using GymManagment.DAL.Models;
using GymManagment.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagment.DAL.Repositories.Class
{
    public class SessionRepository:GenericRepository<Session>, ISessionRepository
    {
        private readonly GymDbcontext _dbcontext;

        public SessionRepository(GymDbcontext dbcontext):base(dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<IEnumerable<Session>> GetAllSessionWithTrainerAndCategory(CancellationToken ct = default)
        {
            var Query= _dbcontext.Session.AsNoTracking().Include(S=>S.Category)
                .Include(s=>s.Trainer);
            return await Query.ToListAsync();
        }
    }
}
