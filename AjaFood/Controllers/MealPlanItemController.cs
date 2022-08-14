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
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace AjaFood.Controllers
{
    public class MealPlanItemController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MealPlanItemController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Metoda typu GET - zobrazení všech jídel jídelníčku dle id
        [Authorize]
        public IActionResult Index(int? id)
        {
            var MealPlanItems = from f in _context.MealPlanItems.Include(m => m.Food).Include(m => m.MealPlan).Where(m => id == m.MealPlan.Id)
                            select f;
            var meowId = id;
                            
            MealPlanItems = MealPlanItems.OrderBy(f => f.Food.Name);
            MealPlanItems = MealPlanItems.OrderBy(f => f.MealPlanId);
            ViewBag.Title =  _context.MealPlans.FindAsync(id).Result.Name;
            ViewBag.ID = id;

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            IEnumerable<Client> objClients;
            objClients = _context.Clients.Where(x => x.UserId == userId);
            objClients = objClients.OrderBy(x => x.FirstName);

            List<int> clientIds = new List<int>();
            List<string> clientNames = new List<string>();
            foreach (Client client in objClients)
            {
                string clientName = client.FirstName + " " + client.LastName;
                clientNames.Add(clientName);
                clientIds.Add(client.Id);
            }
            int? clientId = _context.MealPlans.FindAsync(id).Result.ClientId;

            string selectedClientName = null;
            int? selectedClientId = null;
            if (clientId != null)
            {
                Client tempClient = _context.Clients.Find(clientId);
                if (tempClient != null)
                {
                    selectedClientName = _context.Clients.Find(clientId).FirstName + " " + _context.Clients.Find(clientId).LastName;
                    selectedClientId = clientId;
                }
            }
            else
            {
                selectedClientName = "---";
            }


            ViewData["ClientName"] = selectedClientName;
            ViewData["ClientId"] = selectedClientId;
            ViewData["ClientNames"] = clientNames;
            ViewData["ClientIds"] = clientIds;
            return View(MealPlanItems);
        }



        //Metoda typu GET - přidání jídla do jídelníčku (zobrazí formulář)
        [Authorize]
        public IActionResult Create(int? id, int dayNumber)
        {
            var dest = dayNumber;


            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.FoodId = new SelectList(_context.Foods.Where(t => t.FoodCategory.UserId == userId), "Id", "Name");
            ViewBag.ID = id;
            ViewBag.Day = dayNumber;            
            return View();
        }

        //Metoda typu POST - přidá jídlo do jídelníčku (data obdrží z formuláře)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MealPlanItem createdMealPlanItem, int dayNumber)
        {

            MealPlanItem MealPlanItem = new MealPlanItem();

            if (createdMealPlanItem.MealPlanItemNote == null)
            {
                createdMealPlanItem.MealPlanItemNote = "";
            }
            else
            {
                MealPlanItem.MealPlanItemNote = createdMealPlanItem.MealPlanItemNote;
            }
            MealPlanItem.NumberOfDay = dayNumber;
            MealPlanItem.FoodId = createdMealPlanItem.FoodId;
            MealPlanItem.MealPlanId = createdMealPlanItem.Id;
            MealPlanItem.Quantity = createdMealPlanItem.Quantity;

            _context.MealPlans.First(m => m.Id == MealPlanItem.MealPlanId).DateOfLastModification = DateTime.Now;            
            _context.MealPlanItems.Add(MealPlanItem);
            _context.SaveChanges();
            return Redirect("/MealPlanItem/Index/" + MealPlanItem.MealPlanId);
        }


        //Metoda typu GET - úprava jídla v jídelníčku (zobrazí formulář)
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            var MealPlanItem = await _context.MealPlanItems.FindAsync(id);
            if (id == null || MealPlanItem == null)
            {
                return NotFound();
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewData["FoodId"] = new SelectList(_context.Foods.Where(t => t.FoodCategory.UserId == userId), "Id", "Name", MealPlanItem.FoodId);
            ViewData["MealPlanId"] = new SelectList(_context.MealPlans, "Id", "Id", MealPlanItem.MealPlanId);
            ViewBag.Day = MealPlanItem.NumberOfDay;
            ViewBag.MealPlanId = MealPlanItem.MealPlanId;
            return View(MealPlanItem);

        }

        //Metoda typu POST - úprava jídla v jídelníčku (data obdrží z formuláře)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MealPlanItemNote,Quantity,FoodId,MealPlanId")] MealPlanItem MealPlanItem, int dayNumber, int MealPlanId)
        {
            if (id != MealPlanItem.Id)
            {
                return NotFound();
            }

            if (MealPlanItem.MealPlanItemNote == null)
            {
                MealPlanItem.MealPlanItemNote = "";
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.MealPlans.Find(MealPlanItem.MealPlanId).DateOfLastModification = DateTime.Now;                    
                    MealPlanItem.NumberOfDay = dayNumber;
                    _context.Update(MealPlanItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MealPlanItemExists(MealPlanItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }                
                return Redirect("/MealPlanItem/Index/" + MealPlanItem.MealPlanId);
            }
            ViewData["FoodId"] = new SelectList(_context.Foods, "Id", "Name", MealPlanItem.FoodId);
            ViewData["MealPlanId"] = new SelectList(_context.MealPlans, "Id", "Id", MealPlanItem.MealPlanId);
            return View(MealPlanItem);
        }

        //Metoda typu GET - smazání jídla z jídelníčku (zobrazení dialogu)
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var MealPlanItem = await _context.MealPlanItems
                .Include(m => m.Food)
                .Include(m => m.MealPlan)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (MealPlanItem == null)
            {
                return NotFound();
            }
            ViewBag.ID = id;
            return View(MealPlanItem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]




        //Metoda typu POST - smazání jídla z jídelníčku (dle jeho id)    
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var MealPlanItem = await _context.MealPlanItems.FindAsync(id);

            List<MealPlanItem> MealPlanItemList = new List<MealPlanItem>();
            MealPlanItemList = _context.MealPlanItems.ToList();
            MealPlanItemList.Remove(MealPlanItem);
            _context.MealPlanItems.Remove(MealPlanItem);

            bool isDayEmpty = true;
            foreach (var MealPlanItemik in MealPlanItemList)
            {
                if ((MealPlanItemik.NumberOfDay == MealPlanItem.NumberOfDay))
                {
                    isDayEmpty = false;
                }
            }

            if (isDayEmpty)
            {
                foreach (var MealPlanItemik in _context.MealPlanItems)
                {
                    if (MealPlanItemik.NumberOfDay > MealPlanItem.NumberOfDay)
                    {
                        int newNumberOfDay = MealPlanItemik.NumberOfDay - 1;
                        MealPlanItemik.NumberOfDay = newNumberOfDay;
                        _context.MealPlanItems.Update(MealPlanItemik);
                    }
                }
            }

            int pocetVyskytu = MealPlanItemList.FindAll(t => t.NumberOfDay == MealPlanItem.NumberOfDay).Count - 1;
            await _context.SaveChangesAsync();
            return Redirect("/MealPlanItem/Index/" + MealPlanItem.MealPlanId);
        }

        private bool MealPlanItemExists(int id)
        {
            return _context.MealPlanItems.Any(e => e.Id == id);
        }
    }
}
