using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AjaFood.Models
{

    //Food bude mít povinné: id, název, míra (ml / gramy?), kcal,
    //              volitelné: Tuky, sacharidy, bílkoviny, sůl FOTKA?
    public class Food
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Jméno jídla")]
        [MaxLength(50)]
        public string Name { get; set; } = "foodDefault";

        [Display(Name = "Kategorie jídla")]
        public int FoodCategoryId { get; set; }
        [ForeignKey("FoodCategoryId")]
        [ValidateNever]
        public FoodCategory FoodCategory { get; set; }


        [Display(Name = "Poznámky")]
        [MaxLength(1000)]
        public string Note { get; set; } = "";

        [Display(Name = "Tuky")]
        public double Fats { get; set; } = 0;

        [Display(Name = "Sacharidy")]
        public double Carbohydrates { get; set; } = 0;
        
        [Display(Name = "Bílkoviny")]
        public double Proteins { get; set; } = 0;

        [ValidateNever]
        [Display(Name = "Obrázek")]
        [MaxLength(255)]
        public string ImageName { get; set; }

        [NotMapped]
        [ValidateNever]
        [Display(Name = "Nahrát obrázek")]
        public IFormFile ImageFile { get; set; }

        public static implicit operator Food(string v)
        {
            throw new NotImplementedException();
        }    
    }
}
