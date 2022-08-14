using Microsoft.AspNetCore.Mvc;
using AjaFood.Data;
using AjaFood.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace AjaFood.Controllers
{
    public class FoodCategoryController : Controller
    {

        private readonly ApplicationDbContext _context;

        public FoodCategoryController(ApplicationDbContext context)
        {
            _context = context;
        }


        //Metoda typu GET - zobrazení všech jídelních kategorií
        [Authorize]
        public IActionResult Index()
        {
            IEnumerable<FoodCategory> objFoodCategories;
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier); //nalezení Id přihlášeného uživatele
            objFoodCategories = _context.FoodCategories.Where(x => x.UserId == userId);       
            IEnumerable<Food> objFoods = _context.Foods.ToList();
            return View(objFoodCategories);
        }

        //Metoda typu GET - vytváření nové jídelní kategorie (zobrazí formulář)
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }


        //Metoda typu POST - vytvoření jídelní kategorie (data obdrží z formuláře)
        [HttpPost]
        public IActionResult Create(FoodCategory foodCategory)
        {            
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);            
            if (ModelState.IsValid)
            {
                foodCategory.UserId = userId;
                _context.FoodCategories.Add(foodCategory);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                return View(foodCategory);
            }
        }
        
        //Metoda typu GET - úprava jídelní kategorie (načte formulář)
        [Authorize]
        public IActionResult Edit(int? id)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var foodCategoryFromDb = _context.FoodCategories.Find(id);
            if (foodCategoryFromDb == null || userId != foodCategoryFromDb.UserId)
            {
                return NotFound();
            }

            return View(foodCategoryFromDb);
        }

        //Metoda typu POST - upraví kategorii (data obdrží z formuláře)
        [HttpPost]
        public IActionResult Edit(FoodCategory obj)
        {
            if (ModelState.IsValid)
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                obj.UserId = userId;
                _context.FoodCategories.Update(obj);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                return View(obj);
            }
        }

        //Metoda typu GET - smazání kategorie jídel (zobrazení dialogu)
        [Authorize]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var foodCategoryFromDb = _context.FoodCategories.Find(id);
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (foodCategoryFromDb == null || userId != foodCategoryFromDb.UserId)
            {
                return NotFound();
            }

            return View(foodCategoryFromDb);            
        }

        //Metoda typu POST - smazání kategorie jídel (po potvrzení smaže kategorii jídel dle id)
        [HttpPost]
        public IActionResult Delete(FoodCategory obj)
        {
            if (ModelState.IsValid)
            {
                _context.FoodCategories.Remove(obj);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                return View(obj);
            }
        }        
    }
}

