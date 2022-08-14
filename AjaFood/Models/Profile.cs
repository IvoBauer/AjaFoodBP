using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AjaFood.Models
{
    public class Profile
    {
        [Key]
        public int Id { get; set; }
        [ValidateNever]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public User User { get; set; }

        [ValidateNever]
        public string ImageName { get; set; }

        [NotMapped]
        [ValidateNever]
        [Display(Name = "Nahrát obrázek")]
        public IFormFile ImageFile { get; set; }

        [Display(Name = "Popis")]
        public string? Introduction { get; set; }
        [Display(Name = "Uživatelské jméno")]
        public string? Username { get; set; }        
    }
}
