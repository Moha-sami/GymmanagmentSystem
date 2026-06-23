using GymManagment.DAL.Models;
using GymManagment.DAL.Repositories.Interfaces;
using GymMangment.BLL.Services.Interfaces;
using GymMangment.BLL.ViewModels;

namespace GymMangment.BLL.Services.Class
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AnalyticsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync(CancellationToken ct = default)
        {
            var members = await _unitOfWork.Members.GetAllAsync(ct: ct);
            var trainers = await _unitOfWork.Trainers.GetAllAsync(ct: ct);
            var sessions = await _unitOfWork.Sessions.GetAllAsync(ct: ct);
            var memberships = await _unitOfWork.Memberships.GetAllAsync(ct: ct);

            var now = DateTime.Now;

            return new DashboardViewModel
            {
                TotalMembers = members.Count(),
                ActiveMembers = memberships.Count(m => m.IsActive),
                TotalTrainers = trainers.Count(),
                UpcomingSessions = sessions.Count(s => s.StartDate > now),
                OngoingSessions = sessions.Count(s => s.StartDate <= now && s.EndDate >= now),
                CompletedSessions = sessions.Count(s => s.EndDate < now)
            };
        }
    }
}