using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AjaFood.Models
{
    public class MealPlanItem
    {
        [Key]
        public int Id { get; set; }
        
        [ValidateNever]
        [Display(Name = "Poznámka")]
        [MaxLength(50)]
        public string MealPlanItemNote { get; set; } = "";
        
        [Required]
        [Display(Name = "Jméno jídla")]
        public int FoodId { get; set; }
        [ForeignKey("FoodId")]
        [ValidateNever]
        
        public Food Food { get; set; }
        [Display(Name = "Množství v gramech")]
        [Range(1, Int32.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Display(Name = "MealPlan")]
        public int MealPlanId { get; set; }
        [ForeignKey("MealPlanId")]
        [ValidateNever]
        public MealPlan MealPlan { get; set; }

        public int NumberOfDay { get; set; } = 0;

    }
}
