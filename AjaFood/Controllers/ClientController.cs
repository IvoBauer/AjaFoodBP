using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AjaFood.Data;
using AjaFood.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace AjaFood.Controllers
{
    public class ClientController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        public ClientController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        //Metoda typu GET - zobrazení všech klientů
        [Authorize]
        public IActionResult Index()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier); //nalezení Id přihlášeného uživatele           
            IEnumerable<Client> clients =  _context.Clients.Where(d => d.UserId == userId).ToList();
            return View(clients);
        }


        //Metoda typu GET - zobrazení detailu klienta
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Clients == null)
            {
                return NotFound();
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = await _context.Clients
                .FirstOrDefaultAsync(m => m.Id == id);
            if (client == null || client.UserId != userId)
            {
                return NotFound();
            }

            return View(client);
        }

        //Metoda typu GET - vytváření nového klienta (zobrazí formulář)
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }


        //Metoda typu POST - vytvoření nového klienta (data obdrží z formuláře)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Email,PhoneNumber,Gender," +
                    "LifeStyle,Age,Weight,Height,FavouriteFoods,Allergies,DateOfCreation")] Client client)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            client.UserId = userId;
            if (ModelState.IsValid)
            {
                _context.Add(client);               
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        //Metoda typu GET - úprava klienta (zobrazí formulář)
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Clients == null)
            {
                return NotFound();
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = await _context.Clients.FindAsync(id);
            if (client == null || client.UserId != userId)
            {
                return NotFound();
            }
            return View(client);
        }

        //Metoda typu POST - upraví klienta (data obdrží z formuláře)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Email,PhoneNumber,Gender,LifeStyle,Age,Weight,Height,FavouriteFoods,Allergies,DateOfCreation")] Client client)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            client.UserId = userId;

            if (id != client.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(client);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(client.Id))
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
            return View(client);
        }

        //Metoda typu GET - smazání klienta (zobrazení dialogu)
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Clients == null)
            {
                return NotFound();
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = await _context.Clients
                .FirstOrDefaultAsync(m => m.Id == id);
            if (client == null || client.UserId != userId)
            {
                return NotFound();
            }

            return View(client);
        }

        //Metoda typu POST - smazání klienta (po potvrzení smaže klienta dle jeho id)       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (_context.Clients == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Clients'  is null.");
            }
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
            }

            var applicationDbContext = _context.MealPlans.Where(d => d.ClientId == id);
            IEnumerable<MealPlan> objMealPlans = applicationDbContext.ToList();

            foreach (var MealPlan in objMealPlans)
            {
                MealPlan.ClientId = null;
                _context.Update(MealPlan);
            }  

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClientExists(int id)
        {
          return (_context.Clients?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
