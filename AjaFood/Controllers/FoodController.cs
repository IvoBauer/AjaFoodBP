#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AjaFood.Data;
using AjaFood.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace AjaFood.Controllers
{
    public class FoodController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;        

        public FoodController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }


        //Metoda typu GET - zobrazení všech receptů
        [Authorize]
        public IActionResult Index()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier); //nalezení Id přihlášeného uživatele
            var applicationDbContext = _context.Foods.Include(f => f.FoodCategory).Where(d => d.FoodCategory.UserId == userId);
            IEnumerable<Food> objFoods = applicationDbContext.ToList();
            
            if (userId != null)
            {
                IEnumerable<FoodCategory> objFoodCategories;
                objFoodCategories = _context.FoodCategories.Where(x => x.UserId == userId);

                if (objFoodCategories.Count() == 0) // pokud uživatel smaže všechny své kategorie, vytvoří se výchozí kategorie (recept nelze vytvořit bez žádné kategorie).
                {
                    FoodCategory initCategory = new FoodCategory();
                    initCategory.CategoryName = "-----";
                    initCategory.UserId = userId;
                    _context.FoodCategories.Add(initCategory);
                    _context.SaveChanges();
                }
            }

            return View(objFoods);
        }


        //Metoda typu GET - zobrazení detailu receptu
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var food = await _context.Foods
                .Include(f => f.FoodCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (food == null)
            {
                return NotFound();
            }

            return View(food);
        }


        //Metoda typu GET - vytváření nového receptu (zobrazí formulář)
        [Authorize]
        public IActionResult Create()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewData["FoodCategories"] = new SelectList(_context.FoodCategories.Where(f => f.UserId == userId), "Id", "CategoryName");
            return View();
        }


        //Metoda typu POST - vytvoření nového receptu (data obdrží z formuláře)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,FoodCategoryId,Note,Fats,Carbohydrates,Proteins,ImageFile")] Food food)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier); //nalezení Id přihlášeného uživatele

            if (ModelState.IsValid)
            {        
                if (food.ImageFile == null) //pokud nebyl nahrán obrázek jídla, nastaví se mu výchozí
                {
                    food.ImageName = "defaultImage.png";
                }
                else {                          
                    string wwwrootPath = _hostEnvironment.WebRootPath;
                    string imageName = Path.GetFileNameWithoutExtension(food.ImageFile.FileName);
                    string imageExtension = Path.GetExtension(food.ImageFile.FileName);
                    imageName = imageName + "_" + DateTime.Now.ToString("yymmssfff") + imageExtension; //obrázek bude mít v názvu i časový údaj nahrátí na server
                    food.ImageName = imageName;
    
                    string imagePath = Path.Combine(wwwrootPath + "/images/", imageName);
    
                    using (var filestream = new FileStream(imagePath, FileMode.Create)) //uložení obrázku do wwwroot/images
                    {
                        await food.ImageFile.CopyToAsync(filestream); 
                    }
                }

                _context.Add(food);
                _context.SaveChanges();                
                return RedirectToAction("Index");
            }
            ViewData["FoodCategoryId"] = new SelectList(_context.FoodCategories.Where(f => f.UserId == userId), "Id", "CategoryName", food.FoodCategoryId);
            return View(food);
        }


        //Metoda typu GET - úprava receptu (zobrazí formulář)
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var food = await _context.Foods.FindAsync(id);
            if (food == null)
            {
                return NotFound();
            }
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewData["FoodCategoryId"] = new SelectList(_context.FoodCategories.Where(f => f.UserId == userId), "Id", "CategoryName", food.FoodCategoryId);
            ViewData["imageFood"] = food.ImageName;
            return View(food);
        }

        //Metoda typu POST - upraví recept (data obdrží z formuláře)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,FoodCategoryId,Note,Fats,Carbohydrates,Proteins,ImageFile")] Food food)
        {            
            string oldImageName = _context.Foods.AsNoTracking().ToList().Find(x => x.Id == id).ImageName; //načtení jména souboru původního obrázku
            
                
            if (id != food.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (food.ImageFile != null)
                    {
                        //ukládání obrázků do wwwrootu
                        string wwwrootPath = _hostEnvironment.WebRootPath;
                        string imageName = Path.GetFileNameWithoutExtension(food.ImageFile.FileName);
                        string imageExtension = Path.GetExtension(food.ImageFile.FileName);
                        imageName = imageName + "_" + DateTime.Now.ToString("yymmssfff") + imageExtension;
                        food.ImageName = imageName;

                        string imagePath = Path.Combine(wwwrootPath + "/images/", imageName);

                        //vytvoření nového obrázku
                        using (var filestream = new FileStream(imagePath, FileMode.Create))
                        {
                            await food.ImageFile.CopyToAsync(filestream);
                        }

                        //odstranění původního obrázku

                        if (oldImageName != "defaultImage.png")
                        {
                            string oldImagePath = Path.Combine(wwwrootPath + "/images/", oldImageName);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }
                    }
                    else
                    {

                    }
                    _context.Update(food); //aktualizace databáze
                    if (food.ImageFile == null)
                    {
                        _context.Entry(food).Property(x => x.ImageName).IsModified = false;
                    }
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FoodExists(food.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["FoodCategoryId"] = new SelectList(_context.FoodCategories, "Id", "CategoryName", food.FoodCategoryId);
            return View(food);
        }


        //Metoda typu GET - smazání receptu (zobrazení dialogu)
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var food = await _context.Foods.FindAsync(id);

            if (food == null)
            {
                return NotFound();
            }

            return View(food);
        }


        //Metoda typu POST - smazání receptu (po potvrzení smaže recept dle jeho id)  
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int? id)
        {
            var food = _context.Foods.Find(id);

            //smazání obrázku z wwwroot
            if (food.ImageName != "defaultImage.png")
            {
                string wwwrootPath = _hostEnvironment.WebRootPath;
                string imagePath = Path.Combine(wwwrootPath + "/images/", food.ImageName);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }          

            //smazání objektu food z databáze
            _context.Foods.Remove(food);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
        //DELETE ------------------------

        private bool FoodExists(int id)
        {
            return _context.Foods.Any(e => e.Id == id);
        }
    }
}
