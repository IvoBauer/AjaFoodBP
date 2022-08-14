using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AjaFood.Models
{
    public class FoodCategory
    {        
        [Key]
        public int Id { get; set; }

        [ValidateNever]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public User User { get; set; }

        [Required(ErrorMessage="Je nutné zadat jméno kategorie.")]
        [DisplayName("Název kategorie")]
        [MaxLength(50)]
        public string CategoryName { get; set; } = "category name";

       
    }
}
