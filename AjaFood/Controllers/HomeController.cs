using AjaFood.Data;
using AjaFood.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AjaFood.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {            
            _logger = logger;
            _context = context;
        }


        //Metoda typu GET - zobrazení všech profilů poradců, kteří mají vyplněný profil
        public IActionResult Index()
        {            
            List<IdentityUser> userList = _context.Users.ToList();            
            List<IdentityUser> usersToDisplayList = new List<IdentityUser>();
            foreach (var user in userList) // ořezání dat poradců, aby se na stranu klienta nepředávaly osobní informace
            {
                IdentityUser simplifiedUser = new IdentityUser();
                simplifiedUser.PhoneNumber = user.PhoneNumber;
                simplifiedUser.UserName = user.UserName;
                simplifiedUser.Email = user.Email;
                simplifiedUser.Id = user.Id;
                usersToDisplayList.Add(simplifiedUser);
            }

            List<Profile> profiles = _context.Profiles.Where(x => x.Introduction != null).ToList();
            ViewBag.Profiles = profiles;
            return View(usersToDisplayList);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}