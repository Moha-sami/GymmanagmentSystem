using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace GymMangment.BLL.ViewModels.SessionsViewModels
{
    public class CreateSessionViewModel
    {
        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Trainer is required")]
        public int TrainerId { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, MinimumLength = 5)]
        public string Description { get; set; } = default!;

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, 500, ErrorMessage = "Capacity must be between 1 and 500")]
        public int Capacity { get; set; } = 25;

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }

        // Dropdowns — populated from DB
        public IEnumerable<SelectListItem> Trainers { get; set; } = [];
        public IEnumerable<SelectListItem> Categories { get; set; } = [];
    }
}