using AutoMapper;
using GymManagment.DAL.Models;
using GymManagment.DAL.Repositories.Interfaces;
using GymMangment.BLL.Common;
using GymMangment.BLL.Services.Class;
using GymMangment.BLL.Services.Interfaces;
using GymMangment.BLL.ViewModels.MemberViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace GymmanagmentSystem.PL.Controllers
{
    
    public class MembersController : Controller
    {
        private readonly ImemberService memberservice;
        private readonly IFileService _fileService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;

        public MembersController(ImemberService _memberservice , IFileService fileService,IUnitOfWork unitOfWork, UserManager<AppUser> userManager)
        {
            memberservice = _memberservice;
            _fileService = fileService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        #region GetAllMembers
        //index list all members localhost:port/Members/Index(Get)
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var members = await memberservice.GetAllMembersAsync(ct);
            return View(members);
        }
        //details of member localhost:port/Members/MemberDetails/{Id}(Get)
        //Deatils of HealthRecord localhost:port/Members/HealthRecordDetails/{Id}(Get)
        #endregion

        #region MemberCreate
        // Create Shows member registration form localhost:port/Members/Create(Get)
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create() => View();
        //CreateMember Processes form submission localhost:port/Members/CreateMember(Post) 
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateMember(CreateMemberViewModel model, CancellationToken ct = default)
        {
            if (model.Photo == null || model.Photo.Length == 0)
                ModelState.AddModelError("Photo", "Profile photo is required.");

            if (!ModelState.IsValid) return View(nameof(Create), model);
            if (!ModelState.IsValid) return View(nameof(Create), model);

            // Handle photo upload in controller before calling service
            if (model.Photo != null)
            {
                var photoPath = await _fileService.SaveImageAsync(model.Photo, "uploads");
                if (photoPath == null)
                {
                    ModelState.AddModelError("Photo", "Invalid photo. Please upload a JPG, PNG or WebP image under 2MB.");
                    return View(nameof(Create), model);
                }
                model.PhotoPath = photoPath;
            }

            var result = await memberservice.CreateMemberAsync(model, ct);

            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"]
                = result.Succeeded ? "Member created successfully!" : result.Error;

            return RedirectToAction(nameof(Index));
        }






        #endregion

        // Member Details(int id) - Shows member details localhost:port/Members/MemberDetails/{Id}(Get)
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> MemberDetailsAsync(int id, CancellationToken ct)
        {
            
            var result = await memberservice.GetMemberDetailsByIdAsync(id, ct);

            if (!result.Succeeded)
            {
                return NotFound(result.Error);
            }

            return View(result.Data);
        }

        // HealthRecord Details(int id) - Shows health record details localhost:port/Members/HealthRecordDetails/{Id}(Get)
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> HealthRecordDetails(int id, CancellationToken ct)
        {
            //Get HealthRecord details using id and pass to view
            var result= await memberservice.GetMemberHealthRecordAsync(id, ct);
            //Check if HealthRecord exists, if not return NotFound()
            // IF HealthRecord not Null Return view with HealthRecord details
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Error;
                return RedirectToAction(nameof(Index));

            }
            return View(result.Data);
        }

        #region MemberEdit
        //MemberEdit(int id) - Displays edit form localhost:port/Members/MemberEdit/{Id}(Get)
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> EditMember(int id, CancellationToken ct)
        {
            var result = await memberservice.GetMemberToUpdateAsync(id, ct);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Error;
                return RedirectToAction(nameof(Index));
            }

            return View(result.Data);

        }

        //MemberEdit() - Processes update submission localhost:port/Members/MemberEdit(Post)
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> EditMember(int id, MemberToUpdateViewModel model, CancellationToken ct = default)
        {
            if(!ModelState.IsValid)return View(model);
            var result= await memberservice.UpdateMemberAsync(id, model, ct);

            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"]
                = result.Succeeded ? "Member Updated successfully!" : result.Error;

            return RedirectToAction(nameof(Index));


        }

        #endregion

        #region MemberDelete
        //Delete(int id) - Shows deletion confirmation page localhost:port/Members/Delete/{Id}(Get)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id,CancellationToken ct)
        {
            var member =await memberservice.GetMemberToUpdateAsync(id, ct);
            if (!member.Succeeded)
            {
                TempData["ErrorMessage"] = member.Error;
                return RedirectToAction(nameof(Index));
            }

            return View(member.Data);

        }
        //DeleteConfirmed(int id) - Processes deletion localhost:port/Members/DeleteConfirmed/{Id}(Post)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
        {
            var result = await memberservice.DeleteMemberAsync(id, ct);

            TempData[result.Succeeded ? "WarningMessage" : "ErrorMessage"]
                = result.Succeeded ? "Member deleted successfully!" : result.Error;

            return RedirectToAction(nameof(Index));
        }

        #endregion



        // GET: Members/MyProfile
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> MyProfile(CancellationToken ct)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user?.MemberId == null)
            {
                TempData["ErrorMessage"] = "Your account is not linked to a member profile.";
                return RedirectToAction("Index", "Home");
            }

            var result = await memberservice.GetMemberToUpdateAsync(user.MemberId.Value, ct);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Error;
                return RedirectToAction("Index", "Home");
            }

            return View(result.Data);
        }

        // POST: Members/MyProfile
        [HttpPost]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> MyProfile(MemberToUpdateViewModel model, CancellationToken ct = default)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user?.MemberId == null)
            {
                TempData["ErrorMessage"] = "Your account is not linked to a member profile.";
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid) return View(model);

            var result = await memberservice.UpdateMemberAsync(user.MemberId.Value, model, ct);

            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"]
                = result.Succeeded ? "Profile updated successfully!" : result.Error;

            return RedirectToAction(nameof(MyProfile));
        }

    }
}
