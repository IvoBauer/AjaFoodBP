using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AjaFood.Models
{
    public class Client
    {
        [Key]
        public int Id { get; set; }

        [ValidateNever]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public User User { get; set; }

        [Required]
        [Display(Name = "Jméno")]
        [MaxLength(50)]
        public string FirstName { get; set; } = "FirstName";
        [Required]
        [MaxLength(50)]
        [Display(Name = "Příjmení")]
        public string LastName { get; set; } = "LastName";
        [Display(Name = "E-mail")]
        [MaxLength(256)]
        public string? Email { get; set; }
        [Display(Name = "Telefonní číslo")]
        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Věk")]
        public int? Age { get; set; }
        
        [Display(Name = "Pohlaví")]
        [MaxLength(10)]
        public string? Gender { get; set; }
        [Display(Name = "Životní styl")]
        [MaxLength(1000)]
        public string? LifeStyle { get; set; }
        [Display(Name = "Váha")]       
        public int? Weight { get; set; }
        [Display(Name = "Výška")]
        public int? Height { get; set; }
        [Display(Name = "Oblíbená jídla")]
        [MaxLength(1000)]
        public string? FavouriteFoods { get; set; }
        [Display(Name = "Alergie")]
        [MaxLength(1000)]
        public string? Allergies { get; set; }        
    }
}
