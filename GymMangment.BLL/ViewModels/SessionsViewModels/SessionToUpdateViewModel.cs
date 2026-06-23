using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymMangment.BLL.ViewModels.SessionsViewModels
{
    public class SessionToUpdateViewModel
    {
        public int Id { get; set; }

        // Locked — display only
        public string? CategoryName { get; set; }
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Trainer is required")]
        public int TrainerId { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, MinimumLength = 5)]
        public string Description { get; set; } = default!;

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }

        // Dropdown — populated from DB
        public IEnumerable<SelectListItem> Trainers { get; set; } = [];
    }
}