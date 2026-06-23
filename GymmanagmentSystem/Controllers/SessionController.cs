using GymMangment.BLL.Services.Interfaces;
using GymMangment.BLL.ViewModels.SessionsViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GymmanagmentSystem.PL.Controllers
{
    public class SessionsController : Controller
    {
        private readonly ISessionService _sessionService;

        public SessionsController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        // GET: Sessions/Index
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var result = await _sessionService.GetAllSessionsAsync(ct);
            return View(result.Data);
        }

        // GET: Sessions/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var result = await _sessionService.GetSessionByIdAsync(id, ct);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Error;
                return RedirectToAction(nameof(Index));
            }
            return View(result.Data);
        }

        // GET: Sessions/Create
        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var result = await _sessionService.GetCreateFormDataAsync(ct);
            return View(result.Data);
        }

        // POST: Sessions/Create
        [HttpPost]
        public async Task<IActionResult> Create(CreateSessionViewModel model, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
            {
                var formData = await _sessionService.GetCreateFormDataAsync(ct);
                model.Trainers = formData.Data!.Trainers;
                model.Categories = formData.Data!.Categories;
                return View(model);
            }

            var result = await _sessionService.CreateSessionAsync(model, ct);

            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"]
                = result.Succeeded ? "Session created successfully!" : result.Error;

            if (!result.Succeeded)
            {
                var formData = await _sessionService.GetCreateFormDataAsync(ct);
                model.Trainers = formData.Data!.Trainers;
                model.Categories = formData.Data!.Categories;
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Sessions/EditSession/5
        [HttpGet]
        public async Task<IActionResult> EditSession(int id, CancellationToken ct)
        {
            var result = await _sessionService.GetSessionForEditAsync(id, ct);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Error;
                return RedirectToAction(nameof(Index));
            }
            return View(result.Data);
        }

        // POST: Sessions/EditSession/5
        [HttpPost]
        public async Task<IActionResult> EditSession(int id, SessionToUpdateViewModel model, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
            {
                var formData = await _sessionService.GetSessionForEditAsync(id, ct);
                model.Trainers = formData.Data!.Trainers;
                return View(model);
            }

            var result = await _sessionService.UpdateSessionAsync(id, model, ct);

            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"]
                = result.Succeeded ? "Session updated successfully!" : result.Error;

            return RedirectToAction(nameof(Index));
        }

        // GET: Sessions/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var result = await _sessionService.GetSessionByIdAsync(id, ct);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Error;
                return RedirectToAction(nameof(Index));
            }
            return View(result.Data);
        }

        // POST: Sessions/DeleteConfirmed/5
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
        {
            var result = await _sessionService.DeleteSessionAsync(id, ct);

            TempData[result.Succeeded ? "WarningMessage" : "ErrorMessage"]
                = result.Succeeded ? "Session deleted successfully!" : result.Error;

            return RedirectToAction(nameof(Index));
        }
    }
}