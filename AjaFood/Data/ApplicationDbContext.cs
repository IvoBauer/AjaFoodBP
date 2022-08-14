using AjaFood.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AjaFood.Data
{
    public class ApplicationDbContext :IdentityDbContext
    {       
        public DbSet<Food> Foods { get; set; }
        public DbSet<FoodCategory> FoodCategories { get; set; }
        public DbSet<MealPlanItem> MealPlanItems { get; set; }
        public DbSet<MealPlan> MealPlans { get; set; }
        public DbSet<Client> Clients { get; set; }        
        public DbSet<Profile> Profiles { get; set; }        



        //Nastaveni
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
    }
}
