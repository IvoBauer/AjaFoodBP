using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AjaFood.Models
{
    public class MealPlan
    {            
        [Key]
        public int Id { get; set; }

        [ValidateNever]
        public string UserId { get; set; }

        [ValidateNever]
        [Display(Name = "Jméno klienta")]
        public int? ClientId { get; set; }

        [Display(Name = "Jméno jídelníčku")]

        [MaxLength(50)]
        public string Name { get; set; } = "Meal plan name";

        [Display(Name = "Datum poslední úpravy")]
        public DateTime DateOfLastModification { get; set; } = DateTime.Now;

    }
}
