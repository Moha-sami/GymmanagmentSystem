using GymMangment.BLL.ViewModels;

namespace GymMangment.BLL.Services.Interfaces
{
    public interface IAnalyticsService
    {
        Task<DashboardViewModel> GetDashboardDataAsync(CancellationToken ct = default);
    }
}