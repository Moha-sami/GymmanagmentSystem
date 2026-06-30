using GymManagment.DAL.Models;
using GymManagment.DAL.Models.Enum;
using GymManagment.DAL.Repositories.Interfaces;
using GymMangment.BLL.Services.Interfaces;
using GymMangment.BLL.ViewModels.AccountViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GymmanagmentSystem.PL.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,IUnitOfWork unitOfWork,IFileService fileService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
            _fileService = fileService;
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login() => View();

        // POST: Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
                return RedirectToAction("Index", "Home");

            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register() => View();

        // POST: Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var emailExists = await _unitOfWork.Members.AnyAsync(x => x.Email == model.Email);
            var phoneExists = await _unitOfWork.Members.AnyAsync(x => x.Phone == model.Phone);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "A member with this email already exists.");
                return View(model);
            }

            if (phoneExists)
            {
                ModelState.AddModelError("Phone", "A member with this phone number already exists.");
                return View(model);
            }

            var photoPath = await _fileService.SaveImageAsync(model.Photo, "uploads");
            if (photoPath == null)
            {
                ModelState.AddModelError("Photo", "Invalid photo. Please upload a JPG, PNG or WebP image under 2MB.");
                return View(model);
            }

            var member = new Member
            {
                Name = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                DateOFBirth = model.DateOfBirth,
                Gender = model.Gender,
                Photo = photoPath,
                Address = new Address
                {
                    BuildingNumber = model.BuildingNumber,
                    Street = model.Street,
                    City = model.City
                },
                HealthRecord = new HealthRecord
                {
                    Height = 0,
                    Weight = 0,
                    BloodType = "Unknown",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _unitOfWork.Members.AddAsync(member, default);

            // Auto-assign Basic Plan membership
            var basicPlan = (await _unitOfWork.Plans.GetAllAsync())
                .FirstOrDefault(p => p.Name == "Basic Plan" && p.IsActive);

            if (basicPlan != null)
            {
                var membership = new Membership
                {
                    MemberID = member.Id,
                    PlansID = basicPlan.Id,
                    EndDate = DateTime.Now.AddDays(basicPlan.DurationInDays),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                await _unitOfWork.Memberships.AddAsync(membership, default);
            }

            var user = new AppUser
            {
                FullName = model.FullName,
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                MemberId = member.Id
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Member");
                TempData["SuccessMessage"] = "Account created successfully! You can now login.";
                return RedirectToAction(nameof(Login));
            }

            await _unitOfWork.Members.DeleteAsync(member, default);

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }
        // POST: Account/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}