using AutoMapper;
using GymManagment.DAL.Models;
using GymManagment.DAL.Models.Enum;
using GymManagment.DAL.Repositories.Interfaces;
using GymMangment.BLL.Common;
using GymMangment.BLL.Services.Interfaces;
using GymMangment.BLL.ViewModels.SessionsViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymMangment.BLL.Services.Class
{
    public class SessionService : ISessionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SessionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        private static readonly Dictionary<Specialty, Categories> _specialtyToCategoryMap = new()
        {



    { Specialty.GeneralFitness, Categories.Training },
    { Specialty.Yoga, Categories.Yoga },
    { Specialty.Boxing, Categories.Cardio },
    { Specialty.CrossFit, Categories.Strength }
            };


        public async Task<Result<IEnumerable<SessionViewModel>>> GetAllSessionsAsync(CancellationToken ct = default)
        {
            var sessions = await _unitOfWork.Sessions.GetAllAsync(
                tracking: false,
                ct: ct,
                 s => s.Trainer,
                s => s.Category,
                s => s.SessionMembers);

            var model = _mapper.Map<IEnumerable<SessionViewModel>>(sessions);
            return Result<IEnumerable<SessionViewModel>>.Success(model);
        }

        public async Task<Result<SessionViewModel?>> GetSessionByIdAsync(int id, CancellationToken ct = default)
        {
            var sessions = await _unitOfWork.Sessions.GetAllAsync(
                tracking: false,
                ct: ct,
                 s => s.Trainer,
                s => s.Category,
                s => s.SessionMembers);

            var session = sessions.FirstOrDefault(s => s.Id == id);
            if (session == null)
                return Result<SessionViewModel?>.Failure("Session not found");

            var model = _mapper.Map<SessionViewModel>(session);
            return Result<SessionViewModel?>.Success(model);
        }

        public async Task<Result<CreateSessionViewModel>> GetCreateFormDataAsync(CancellationToken ct = default)
        {
            var trainers = await _unitOfWork.Trainers.GetAllAsync(ct: ct);
            var categories = await _unitOfWork.Categories.GetAllAsync(ct: ct);

            var model = new CreateSessionViewModel
            {
                Trainers = trainers.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                }),
                Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.CategoryName.ToString()
                })
            };

            return Result<CreateSessionViewModel>.Success(model);
        }

        public async Task<Result> CreateSessionAsync(CreateSessionViewModel model, CancellationToken ct = default)
        {
            if (model.EndDate <= model.StartDate)
                return Result.Failure("End date must be after start date.");

            var trainer = await _unitOfWork.Trainers.GetByIdAsync(model.TrainerId, ct);
            if (trainer == null)
                return Result.Failure("Trainer not found.");

            var category = await _unitOfWork.Categories.GetByIdAsync(model.CategoryId, ct);
            if (category == null)
                return Result.Failure("Category not found.");

            if (!_specialtyToCategoryMap.TryGetValue(trainer.Specialty, out var expectedCategory)
                || expectedCategory != category.CategoryName)
                return Result.Failure($"Trainer specializes in {trainer.Specialty} — please select the matching category: {_specialtyToCategoryMap[trainer.Specialty]}.");

            var session = _mapper.Map<Session>(model);
            var rows = await _unitOfWork.Sessions.AddAsync(session, ct);

            return rows > 0
                ? Result.Success()
                : Result.Failure("Failed to create session. Please try again.");
        }

        public async Task<Result<SessionToUpdateViewModel?>> GetSessionForEditAsync(int id, CancellationToken ct = default)
        {
            var sessions = await _unitOfWork.Sessions.GetAllAsync(
                tracking: false,
                ct: ct,
                 s => s.Trainer,
                s => s.Category);

            var session = sessions.FirstOrDefault(s => s.Id == id);
            if (session == null)
                return Result<SessionToUpdateViewModel?>.Failure("Session not found");

            var trainers = await _unitOfWork.Trainers.GetAllAsync(ct: ct);

            var model = _mapper.Map<SessionToUpdateViewModel>(session);
            model.Trainers = trainers.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.Name,
                Selected = t.Id == session.TrainerId
            });

            return Result<SessionToUpdateViewModel?>.Success(model);
        }

        public async Task<Result> UpdateSessionAsync(int id, SessionToUpdateViewModel model, CancellationToken ct = default)
        {
            if (model.EndDate <= model.StartDate)
                return Result.Failure("End date must be after start date");

            var session = await _unitOfWork.Sessions.GetByIdAsync(id, ct);
            if (session == null)
                return Result.Failure("Session not found");

            _mapper.Map(model, session);
            await _unitOfWork.Sessions.UpdateAsync(session, ct);
            return Result.Success();
        }

        public async Task<Result> DeleteSessionAsync(int id, CancellationToken ct = default)
        {
            var session = await _unitOfWork.Sessions.GetByIdAsync(id, ct);
            if (session == null)
                return Result.Failure("Session not found");

            await _unitOfWork.Sessions.DeleteAsync(session, ct);
            return Result.Success();
        }
    }
}