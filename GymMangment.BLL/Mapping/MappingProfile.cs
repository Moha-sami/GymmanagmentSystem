using AutoMapper;
using GymManagment.DAL.Models;
using GymMangment.BLL.ViewModels.HealthRecordsViewModels;
using GymMangment.BLL.ViewModels.MemberViewModels;
using GymMangment.BLL.ViewModels.PlansViewModels;
using GymMangment.BLL.ViewModels.SessionsViewModels;
using GymMangment.BLL.ViewModels.TrainerViewModels;

namespace GymMangment.BLL.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Member -> MemberViewModel (list display)
            CreateMap<Member, MemberViewModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.ToString()))
                .ForMember(dest => dest.photo, opt => opt.MapFrom(src => src.Photo));

            // CreateMemberViewModel -> Member (creation)
            CreateMap<CreateMemberViewModel, Member>()
                .ForMember(dest => dest.DateOFBirth, opt => opt.MapFrom(src => src.DateOfBirth))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => new Address
                {
                    BuildingNumber = src.BuildingNumber,
                    City = src.City,
                    Street = src.Street
                }))
                .ForMember(dest => dest.HealthRecord, opt => opt.MapFrom(src => src.HealthRecordViewModel));
            // Member-> MemberDetails
            CreateMap<Member, MemberViewModel>()
     .ForMember(dest => dest.Address, opt => opt.MapFrom(src => $"{src.Address.Street} - {src.Address.City}- {src.Address.BuildingNumber}"))
     .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.ToString()))
     .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOFBirth.ToString()));
     
    
     

            // HealthRecordViewModel -> HealthRecord
            CreateMap<HealthRecordViewModel, HealthRecord>();

            // HealthRecord -> HealthRecordViewModel (for details/edit later)
            CreateMap<HealthRecord, HealthRecordViewModel>();


            // Member -> MemberToUpdateViewModel (for pre-filling the edit form)
            CreateMap<Member, MemberToUpdateViewModel>()
                .ForMember(dest => dest.BuildingNumber, opt => opt.MapFrom(src => src.Address.BuildingNumber))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Address.City))
                .ForMember(dest => dest.Street, opt => opt.MapFrom(src => src.Address.Street));

            // MemberToUpdateViewModel -> Member (for saving changes)
            CreateMap<MemberToUpdateViewModel, Member>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => new Address
                {
                    BuildingNumber = src.BuildingNumber,
                    City = src.City,
                    Street = src.Street
                }))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.Name, opt => opt.Ignore())   // not editable
                .ForMember(dest => dest.Photo, opt => opt.Ignore());  // not editable

                // Plans -> PlanViewModel (list & details)
                 CreateMap<Plans, PlanViewModel>();

             // Plans -> EditPlanViewModel (pre-fill edit form)
             CreateMap<Plans, EditPlanViewModel>();

            // EditPlanViewModel -> Plans (saving changes, Name is locked)
            CreateMap<EditPlanViewModel, Plans>()
                .ForMember(dest => dest.Name, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.Now));

            // Trainer -> TrainerViewModel (Index + Details)
            CreateMap<Trainer, TrainerViewModel>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src =>
                    $"{src.Address.BuildingNumber} - {src.Address.Street} - {src.Address.City}"))
                .ForMember(dest => dest.Specialty, opt => opt.MapFrom(src => src.Specialty.ToString()));

            // CreateTrainerViewModel -> Trainer (creation)
            CreateMap<CreateTrainerViewModel, Trainer>()
                .ForMember(dest => dest.DateOFBirth, opt => opt.MapFrom(src => src.DateOfBirth))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => new Address
                {
                    BuildingNumber = src.BuildingNumber,
                    City = src.City,
                    Street = src.Street
                }));

            // Trainer -> TrainerToUpdateViewModel (pre-fill edit form)
            CreateMap<Trainer, TrainerToUpdateViewModel>()
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOFBirth))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.ToString()))
                .ForMember(dest => dest.BuildingNumber, opt => opt.MapFrom(src => src.Address.BuildingNumber))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Address.City))
                .ForMember(dest => dest.Street, opt => opt.MapFrom(src => src.Address.Street));

            // TrainerToUpdateViewModel -> Trainer (saving edits, Name/DOB/Gender locked)
            CreateMap<TrainerToUpdateViewModel, Trainer>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => new Address
                {
                    BuildingNumber = src.BuildingNumber,
                    City = src.City,
                    Street = src.Street
                }))
                .ForMember(dest => dest.Name, opt => opt.Ignore())
                .ForMember(dest => dest.DateOFBirth, opt => opt.Ignore())
                .ForMember(dest => dest.Gender, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.Now));

            // Session -> SessionViewModel
            CreateMap<Session, SessionViewModel>()
                .ForMember(dest => dest.TrainerName, opt => opt.MapFrom(src => src.Trainer.Name))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName.ToString()))
                .ForMember(dest => dest.AvailableSlots, opt => opt.MapFrom(src => src.Capacity - src.SessionMembers.Count));

            // CreateSessionViewModel -> Session
            CreateMap<CreateSessionViewModel, Session>()
                .ForMember(dest => dest.Trainer, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.SessionMembers, opt => opt.Ignore());

            // Session -> SessionToUpdateViewModel
            CreateMap<Session, SessionToUpdateViewModel>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName.ToString()));

            // SessionToUpdateViewModel -> Session
            CreateMap<SessionToUpdateViewModel, Session>()
                .ForMember(dest => dest.Trainer, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.CategoryId, opt => opt.Ignore())
                .ForMember(dest => dest.Capacity, opt => opt.Ignore())
                .ForMember(dest => dest.SessionMembers, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.Now));
        }
    }
}