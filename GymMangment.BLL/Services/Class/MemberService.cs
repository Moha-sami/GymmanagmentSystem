using AutoMapper;
using GymManagment.DAL.Models;
using GymManagment.DAL.Repositories.Interfaces;
using GymMangment.BLL.Common;
using GymMangment.BLL.Services.Interfaces;
using GymMangment.BLL.ViewModels.HealthRecordsViewModels;
using GymMangment.BLL.ViewModels.MemberViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace GymMangment.BLL.Services.Class
{
    public class MemberService : ImemberService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MemberService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result> CreateMemberAsync(CreateMemberViewModel model, CancellationToken ct = default)
        {
            var emailExists = await _unitOfWork.Members.AnyAsync(x => x.Email == model.Email, ct);
            var phoneExists = await _unitOfWork.Members.AnyAsync(x => x.Phone == model.Phone, ct);

            if (emailExists)
                return Result.Failure("A member with this email already exists.");

            if (phoneExists)
                return Result.Failure("A member with this phone number already exists.");

            var member = _mapper.Map<GymManagment.DAL.Models.Member>(model);
            member.Photo = null;
            member.Photo = model.PhotoPath; 
            var rows = await _unitOfWork.Members.AddAsync(member, ct);

            if (rows > 0)
            {
                await AddWeightProgressRecordAsync(
                    member.Id,
                    model.HealthRecordViewModel.Weight,
                    "Initial member health record",
                    ct);
            }

            return rows > 0
                ? Result.Success()
                : Result.Failure("Failed to create member. Please try again.");
        }


        public async Task<IEnumerable<MemberViewModel>> GetAllMembersAsync(CancellationToken ct = default)
        {
            var members = await _unitOfWork.Members.GetAllAsync(ct: ct);
            return _mapper.Map<IEnumerable<MemberViewModel>>(members);
        }

        public async Task<Result<MemberViewModel?>> GetMemberDetailsByIdAsync(int id, CancellationToken ct = default)
        {
            // 1. جلب العضو مع الـ Navigation Properties الخاصة به (Membership و Plan)
            // لاحظ استخدام الـ Includes عشان نتجنب الـ Lazy Loading
            var member = await _unitOfWork.Members.GetByIdAsync(
                id,
                ct,
                m => m.Memberships            
            );
            if (member == null)
            {
                return Result<MemberViewModel?>.Failure("Invaild Member");
            }
            // 2. التحويل الأساسي للـ ViewModel
            var model = _mapper.Map<MemberViewModel>(member);

            var activeMembership = member.Memberships
     .FirstOrDefault(m => m.EndDate >= DateTime.Now);


            // 2. دلوقتي استخدم activeMembership (اللي هو عنصر واحد من نوع Membership)
            if (activeMembership != null)
            {
                // هنا تقدر توصل للـ Plan والـ EndDate والـ CreatedAt
                model.PlanName = activeMembership.Plans?.Name ?? "No Plan";
                model.MembershipStartDate = activeMembership.CreatedAt.ToString("yyyy-MM-dd"); 
                model.MembershipEndDate = activeMembership.EndDate.ToString("yyyy-MM-dd");
            }
            else
            {
                model.PlanName = "No Plan";
            }

            // 4. إرجاع النتيجة الناجحة
            return Result<MemberViewModel?>.Success(model);
        }

        public async Task<Result<HealthRecordViewModel?>> GetMemberHealthRecordAsync(int memberID, CancellationToken ct = default)
        {
            var record = await _unitOfWork.HealthRecords.FirstOrDefaultAsync(x => x.MemberId == memberID, false, ct);
           
            if(record == null)
            {
                return Result<HealthRecordViewModel?>.Failure("No record for This member");
            }
            
            
                var model= _mapper.Map<HealthRecordViewModel>(record);
            model.WeightProgress = await GetWeightProgressLastSixMonthsAsync(memberID, record.Weight, record.CreatedAt, ct);
            
            return Result<HealthRecordViewModel?>.Success(model);
        }

        public async Task<Result<MemberToUpdateViewModel?>> GetMemberToUpdateAsync(int memberId, CancellationToken ct = default)
        {
            var result = await _unitOfWork.Members.GetByIdAsync(memberId, ct);
            if (result == null)
            {
                return Result<MemberToUpdateViewModel?>.Failure("No Member Available");
            }
            var model= _mapper.Map<MemberToUpdateViewModel>(result);
            return Result<MemberToUpdateViewModel?>.Success(model);
        }

        public async Task<Result?> UpdateMemberAsync(int id, MemberToUpdateViewModel model, CancellationToken ct = default)
        {
            var member = await _unitOfWork.Members.GetByIdAsync(id, ct);
            if (member == null)
            {
                return Result.Failure("No Member Available");
            }

            var emailExist = await _unitOfWork.Members.AnyAsync(m => m.Email == model.Email && m.Id != id);
            var phoneExist = await _unitOfWork.Members.AnyAsync(m => m.Phone == model.Phone && m.Id != id);
            if (emailExist || phoneExist)
            {
                return Result.Failure("Email or Phone already exists");
            }

            _mapper.Map(model, member);
            await _unitOfWork.Members.UpdateAsync(member, ct);
            return Result.Success();
        }
        public async Task<Result> DeleteMemberAsync(int memberId, CancellationToken ct = default)
        {
            var result = await _unitOfWork.Members.GetByIdAsync(memberId, ct);
            if (result == null)
            {
                return Result.Failure("No Member Available");
            }

            try
            {
                await _unitOfWork.Members.DeleteAsync(result, ct);
                return Result.Success();
            }
            catch (DbUpdateException)
            {
                return Result.Failure("Cannot delete this member — they have an active login account, bookings, or membership history linked to them.");
            }
        }

        private async Task AddWeightProgressRecordAsync(int memberId, decimal weight, string? note, CancellationToken ct)
        {
            var progressRecord = new WeightProgressRecord
            {
                MemberId = memberId,
                Weight = weight,
                RecordedAt = DateTime.Now,
                Note = note,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _unitOfWork.WeightProgressRecords.AddAsync(progressRecord, ct);
        }

        private async Task<IEnumerable<WeightProgressPointViewModel>> GetWeightProgressLastSixMonthsAsync(
            int memberId,
            decimal currentWeight,
            DateTime healthRecordCreatedAt,
            CancellationToken ct)
        {
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);
            var progressRecords = await _unitOfWork.WeightProgressRecords.GetAllAsync(ct: ct);
            var memberProgress = progressRecords
                .Where(r => r.MemberId == memberId && r.RecordedAt >= sixMonthsAgo)
                .OrderBy(r => r.RecordedAt)
                .ToList();

            if (!memberProgress.Any() && healthRecordCreatedAt >= sixMonthsAgo)
            {
                await AddWeightProgressRecordAsync(memberId, currentWeight, "Imported from current health record", ct);
                memberProgress.Add(new WeightProgressRecord
                {
                    MemberId = memberId,
                    Weight = currentWeight,
                    RecordedAt = healthRecordCreatedAt,
                    Note = "Imported from current health record"
                });
            }

            return memberProgress.Select(r => new WeightProgressPointViewModel
            {
                DateLabel = r.RecordedAt.ToString("MMM dd"),
                Weight = r.Weight
            });
        }
    }
}
