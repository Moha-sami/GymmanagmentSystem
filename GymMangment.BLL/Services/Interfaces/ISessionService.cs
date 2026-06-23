using GymMangment.BLL.Common;
using GymMangment.BLL.ViewModels.SessionsViewModels;

namespace GymMangment.BLL.Services.Interfaces
{
    public interface ISessionService
    {
        Task<Result<IEnumerable<SessionViewModel>>> GetAllSessionsAsync(CancellationToken ct = default);
        Task<Result<SessionViewModel?>> GetSessionByIdAsync(int id, CancellationToken ct = default);
        Task<Result<CreateSessionViewModel>> GetCreateFormDataAsync(CancellationToken ct = default);
        Task<Result> CreateSessionAsync(CreateSessionViewModel model, CancellationToken ct = default);
        Task<Result<SessionToUpdateViewModel?>> GetSessionForEditAsync(int id, CancellationToken ct = default);
        Task<Result> UpdateSessionAsync(int id, SessionToUpdateViewModel model, CancellationToken ct = default);
        Task<Result> DeleteSessionAsync(int id, CancellationToken ct = default);
    }
}