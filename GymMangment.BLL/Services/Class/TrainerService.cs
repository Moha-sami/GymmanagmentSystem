using AutoMapper;
using GymManagment.DAL.Models;
using GymManagment.DAL.Repositories.Interfaces;
using GymMangment.BLL.Common;
using GymMangment.BLL.Services.Interfaces;
using GymMangment.BLL.ViewModels.TrainerViewModels;
using Microsoft.EntityFrameworkCore;

namespace GymMangment.BLL.Services.Class
{
    public class TrainerService : ITrainerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TrainerService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<TrainerViewModel>>> GetAllTrainersAsync(CancellationToken ct = default)
        {
            var trainers = await _unitOfWork.Trainers.GetAllAsync(ct: ct);
            var model = _mapper.Map<IEnumerable<TrainerViewModel>>(trainers);
            return Result<IEnumerable<TrainerViewModel>>.Success(model);
        }

        public async Task<Result<TrainerViewModel?>> GetTrainerByIdAsync(int id, CancellationToken ct = default)
        {
            var trainer = await _unitOfWork.Trainers.GetByIdAsync(id, ct);
            if (trainer == null)
                return Result<TrainerViewModel?>.Failure("No trainer found with this id");

            var model = _mapper.Map<TrainerViewModel>(trainer);
            return Result<TrainerViewModel?>.Success(model);
        }

        public async Task<Result> CreateTrainerAsync(CreateTrainerViewModel model, CancellationToken ct = default)
        {
            var emailExists = await _unitOfWork.Trainers.AnyAsync(x => x.Email == model.Email, ct);
            var phoneExists = await _unitOfWork.Trainers.AnyAsync(x => x.Phone == model.Phone, ct);

            if (emailExists)
                return Result.Failure("A trainer with this email already exists.");

            if (phoneExists)
                return Result.Failure("A trainer with this phone number already exists.");

            var trainer = _mapper.Map<Trainer>(model);
            var rows = await _unitOfWork.Trainers.AddAsync(trainer, ct);

            return rows > 0
                ? Result.Success()
                : Result.Failure("Failed to create trainer. Please try again.");
        }

        public async Task<Result<TrainerToUpdateViewModel?>> GetTrainerForEditAsync(int id, CancellationToken ct = default)
        {
            var trainer = await _unitOfWork.Trainers.GetByIdAsync(id, ct);
            if (trainer == null)
                return Result<TrainerToUpdateViewModel?>.Failure("No trainer found with this id");

            var model = _mapper.Map<TrainerToUpdateViewModel>(trainer);
            return Result<TrainerToUpdateViewModel?>.Success(model);
        }

        public async Task<Result> UpdateTrainerAsync(int id, TrainerToUpdateViewModel model, CancellationToken ct = default)
        {
            var trainer = await _unitOfWork.Trainers.GetByIdAsync(id, ct);
            if (trainer == null)
                return Result.Failure("No trainer found with this id");

            var emailExists = await _unitOfWork.Trainers.AnyAsync(x => x.Email == model.Email && x.Id != id, ct);
            var phoneExists = await _unitOfWork.Trainers.AnyAsync(x => x.Phone == model.Phone && x.Id != id, ct);

            if (emailExists || phoneExists)
                return Result.Failure("Email or Phone already exists");

            _mapper.Map(model, trainer);
            await _unitOfWork.Trainers.UpdateAsync(trainer, ct);
            return Result.Success();
        }

        public async Task<Result> DeleteTrainerAsync(int id, CancellationToken ct = default)
        {
            var trainer = await _unitOfWork.Trainers.GetByIdAsync(id, ct);
            if (trainer == null)
                return Result.Failure("No trainer found with this id");

            try
            {
                await _unitOfWork.Trainers.DeleteAsync(trainer, ct);
                return Result.Success();
            }
            catch (DbUpdateException)
            {
                return Result.Failure("Cannot delete this trainer — they have sessions, login account, or other records linked to them.");
            }
        }
    }
}