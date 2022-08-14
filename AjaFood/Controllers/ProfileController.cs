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
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProfileController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        //Metoda typu GET - zobrazení profilu uživatele
        [Authorize]
        public async Task<IActionResult> Index()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            IEnumerable<Profile> userProfile =  _context.Profiles.Where(x => x.UserId == userId);

            if (userId != null)
            {                
                userProfile = _context.Profiles.Where(x => x.UserId == userId);

                if (userProfile.Count() == 0)
                {
                    Profile initProfile = new Profile();
                    initProfile.UserId = userId;
                    initProfile.ImageName = "profileImage.jpg";
                    _context.Profiles.Add(initProfile);
                    _context.SaveChanges();

                }

                IdentityUser? currentUser = await _context.Users.FindAsync(userId);
                IdentityUser simplifiedUser = new IdentityUser();
                simplifiedUser.PhoneNumber = currentUser.PhoneNumber;
                simplifiedUser.UserName = currentUser.UserName;
                simplifiedUser.Email = currentUser.Email;

                ViewBag.User = simplifiedUser;
            }

            return View(userProfile.First());
        }

        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Set<User>(), "Id", "Id");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,ImageName,Introduction")] Profile profile)
        {
            if (ModelState.IsValid)
            {
                _context.Add(profile);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Set<User>(), "Id", "Id", profile.UserId);
            return View(profile);
        }

        //Metoda typu GET - úprava profilu (zobrazí formulář)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Profiles == null)
            {
                return NotFound();
            }

            var profile = await _context.Profiles.FindAsync(id);
            if (profile == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Set<User>(), "Id", "Id", profile.UserId);
            return View(profile);
        }

        //Metoda typu POST - upraví profil (data obdrží z formuláře)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ImageFile,Introduction, Username")] Profile profile)
        {
            string oldImageName = _context.Profiles.AsNoTracking().ToList().Find(x => x.Id == id).ImageName;

            if (id != profile.Id)
            {
                return NotFound();
            }
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            profile.UserId = userId;


            if (ModelState.IsValid)
            {
                try
                {
                    if (profile.ImageFile != null)
                    {
                        //ukládání obrázků do wwwrootu
                        string wwwrootPath = _hostEnvironment.WebRootPath;
                        string imageName = Path.GetFileNameWithoutExtension(profile.ImageFile.FileName);
                        string imageExtension = Path.GetExtension(profile.ImageFile.FileName);
                        imageName = imageName + "_" + DateTime.Now.ToString("yymmssfff") + imageExtension;
                        profile.ImageName = imageName;

                        string imagePath = Path.Combine(wwwrootPath + "/ProfileImages/", imageName);

                        //nahrání nového obrázku
                        using (var filestream = new FileStream(imagePath, FileMode.Create))
                        {
                            await profile.ImageFile.CopyToAsync(filestream);
                        }

                        //smazání původního obrázku
                        if (oldImageName != "profileimage.jpg")
                        {
                            string oldImagePath = Path.Combine(wwwrootPath + "/ProfileImages/", oldImageName);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }
                    } else
                    {
                        profile.ImageName = "profileimage.jpg";
                    }
                    _context.Update(profile);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProfileExists(profile.Id))
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
            ViewData["UserId"] = new SelectList(_context.Set<User>(), "Id", "Id", profile.UserId);
            return View(profile);
        }

        private bool ProfileExists(int id)
        {
            return (_context.Profiles?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
