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
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using iTextSharp.text;
using iTextSharp.text.pdf;
//using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace AjaFood.Controllers
{
    public class MealPlanController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MealPlanController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Metoda typu GET - zobrazení všech jídelníčků
        [Authorize]
        public async Task<IActionResult> Index()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            List<string> clientNameList = new List<string>();
            IEnumerable<MealPlan> objMealPlans;
            objMealPlans = await _context.MealPlans.Where(x => x.UserId == userId).ToListAsync(); //získání všech jídelníčků uživatele
            objMealPlans = objMealPlans.OrderByDescending(f => f.DateOfLastModification); //seřazení dle data poslední úpravy

            foreach (var MealPlan in objMealPlans)
            {

                Client client = await _context.Clients.FirstOrDefaultAsync(m => m.Id == MealPlan.ClientId);
                string clientName = "";
                if (client != null)
                {
                    clientName = client.FirstName + " " + client.LastName;
                }
                clientNameList.Add(clientName);

            }
            ViewBag.ClientList = clientNameList;


            if (userId != null)
            {
                IEnumerable<FoodCategory> objFoodCategories;
                objFoodCategories = _context.FoodCategories.Where(x => x.UserId == userId);

                if (objFoodCategories.Count() == 0)
                {
                    FoodCategory initCategory = new FoodCategory();
                    initCategory.CategoryName = "-----";
                    initCategory.UserId = userId;
                    _context.FoodCategories.Add(initCategory);
                    _context.SaveChanges();
                }

            }

            return View(objMealPlans);
        }

        public List<Food> GetDataForPDF(string userId) //získá a vrátí list receptů vytvořených přihlášeným uživatelem
        {
            List<Food> foodList = _context.Foods.Where(x => x.FoodCategory.UserId == userId).ToList();
            return foodList;
        }

        //Metoda typu GET - zobrazení všech klientů
        [Authorize]
        public IActionResult GeneratePDF(int id)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier); //získání Id přihlášeného uživatele
            List<Food> foodList = GetDataForPDF(userId); //načtení všech receptů uživatele

            using (MemoryStream ms = new MemoryStream())
            {
                MealPlan MealPlan = _context.MealPlans.Find(id);                
                IEnumerable<MealPlanItem> MealPlanItemList = _context.MealPlanItems.Where(x => x.MealPlanId == id);
                MealPlanItemList = MealPlanItemList.OrderBy(x => x.NumberOfDay);
                Document document = new Document(PageSize.A4, 25, 25, 25, 25); //vytvoření dokumentu, nastavení na formát A4 a odsazení ze všech stran na 25
                PdfWriter writter = PdfWriter.GetInstance(document, ms);
                document.Open(); //otevření dokumentu


                //nastavení fontu Arial
                string FontsPath = Path.Combine(System.Environment.GetEnvironmentVariable("windir"), "Fonts");
                BaseFont fontArial = BaseFont.CreateFont(Environment.GetEnvironmentVariable("windir") + @"\fonts\arial.TTF", BaseFont.IDENTITY_H, true);

                Paragraph pdfTitle = new Paragraph(MealPlan.Name, new Font(fontArial, 25)); //vytvoření nadpisu souboru s názvem jídelníčku
                pdfTitle.Alignment = Element.ALIGN_CENTER;
                document.Add(pdfTitle);

                Client client = _context.Clients.Find(MealPlan.ClientId); //pokud má jídelníček přiřazeného klienta, tak se v nadpise objeví i jeho jméno
                if (client != null)
                {
                    Paragraph pdfClientName = new Paragraph(client.FirstName + " " + client.LastName, new Font(fontArial, 15));
                    pdfClientName.Alignment = Element.ALIGN_CENTER;
                    document.Add(pdfClientName);
                }

                int lastDay = 0;
                PdfPTable table = new PdfPTable(4); //vytvoření nové tabulky
                string lastNote = "";

                //projde všechny položky jídelníčku a postará se o zapsání do dokumentu.
                foreach (MealPlanItem MealPlanItem in MealPlanItemList)
                {
                    Paragraph MealPlanItemNote = null;
                    
                    var mealImage = Image.GetInstance("wwwroot/images/" + MealPlanItem.Food.ImageName);

                    float imageWidth = 60;
                    float imageHeight = (60 / mealImage.Width) * mealImage.Height;
                    mealImage.ScaleAbsolute(imageWidth, imageHeight);


                    PdfPCell cellImage = new PdfPCell(mealImage);
                    PdfPCell cellNote = new PdfPCell();
                    PdfPCell cellQuantity = new PdfPCell();
                    PdfPCell cellFoodName = new PdfPCell();
                    PdfPCell cellFoodNote = new PdfPCell();
                    PdfPCell cellSpace = new PdfPCell();

                    cellImage.BorderWidth = 0f;                    
                    cellNote.BorderWidth = 0f;
                    cellQuantity.BorderWidth = 0f;
                    cellFoodName.BorderWidth = 0f;
                    cellFoodNote.BorderWidth = 0f;
                    cellSpace.BorderWidth = 0f;
                    
                    cellNote.PaddingTop = 10f;
                    cellFoodName.PaddingTop = 10f;
                    cellQuantity.PaddingTop = 10f;
                    cellImage.PaddingTop = 10f;

                    //vytvoření nového dne v PDF
                    if (MealPlanItem.NumberOfDay > lastDay)
                    {
                        document.Add(table);
                        Paragraph oddelovac = new Paragraph("     ", new Font(fontArial, 15));
                        oddelovac.Alignment = Element.ALIGN_LEFT;
                        document.Add(oddelovac);

                        lastDay = MealPlanItem.NumberOfDay;
                        string newDay = MealPlanItem.NumberOfDay.ToString() + ". den";
                        Paragraph newDayText = new Paragraph(newDay, new Font(fontArial, 20));
                        newDayText.Alignment = Element.ALIGN_CENTER;                        
                        document.Add(newDayText);

                        Paragraph space = new Paragraph("\n", new Font(fontArial, 10));
                        document.Add(space);

                        table = new PdfPTable(4);

                        MealPlanItemNote = new Paragraph(MealPlanItem.MealPlanItemNote, new Font(fontArial, 15));
                        cellNote.Phrase = new Phrase(MealPlanItem.MealPlanItemNote, new Font(fontArial, 15));
                        cellImage.BorderWidthTop = 2f;
                        cellNote.BorderWidthTop = 2f;
                        cellQuantity.BorderWidthTop = 2f;
                        cellFoodName.BorderWidthTop = 2f;
                        lastNote = MealPlanItem.MealPlanItemNote;


                    } else
                    {
                        if (lastNote != MealPlanItem.MealPlanItemNote)
                        {
                            cellNote.Phrase = new Phrase(MealPlanItem.MealPlanItemNote, new Font(fontArial, 15));
                            cellImage.BorderWidthTop = 2f;
                            cellNote.BorderWidthTop = 2f;
                            cellQuantity.BorderWidthTop = 2f;
                            cellFoodName.BorderWidthTop = 2f;
                            lastNote = MealPlanItem.MealPlanItemNote;
                        } else
                        {
                            cellNote.Phrase = new Phrase(" ");
                            lastNote = null;
                        }
                    }

                    table.AddCell(cellNote);
                    table.AddCell(cellImage);
                    cellQuantity.Phrase = new Phrase(MealPlanItem.Quantity.ToString() + " g");                                        
                    table.AddCell(cellQuantity);

                    foreach (Food food in foodList)
                    {
                        if (food.Id == MealPlanItem.FoodId)
                        {
                            cellFoodName.Phrase = new Phrase(food.Name, new Font(fontArial, 15));
                            table.AddCell(cellFoodName);                            
                        }
                    }
                    cellSpace.Phrase = new Phrase("");
                    table.AddCell(cellSpace);
                    cellFoodNote.Phrase = new Phrase(MealPlanItem.Food.Note, new Font(fontArial, 15));
                    cellFoodNote.Colspan = 3;
                    cellFoodNote.HorizontalAlignment = 0;
                    cellFoodNote.PaddingBottom = 15f;
                    table.AddCell(cellFoodNote);                    
                }
                document.Add(table);

                document.Close(); //zavření dokumentu
                writter.Close();
                var constant = ms.ToArray();
                string pdfName = MealPlan.Name + ".pdf";
                return File(constant, "application/vnd", pdfName); //na straně klienta se stáhne generovaný PDF soubor, který nese v názvu jméno jídelníčku a příponu PDF.
            }            
        }


        //Metoda typu GET - vytváření nového jídelníčku (zobrazí formulář)
        [Authorize]
        public IActionResult Create()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier); //načte Id přihlášeného uživatele
            IEnumerable<Client> objClients;
            objClients = _context.Clients.Where(x => x.UserId == userId); //získá všechny klienty uživatele
            objClients = objClients.OrderBy(x => x.FirstName);  //seřadí dle jména

            List<int> clientIds = new List<int>();
            List<string> clientNames = new List<string>();
            foreach (Client client in objClients)
            {
                string clientName = client.FirstName + " " + client.LastName;
                clientNames.Add(clientName);
                clientIds.Add(client.Id);
            }

            ViewData["ClientNames"] = clientNames;
            ViewData["ClientIds"] = clientIds;
            return View();
        }


        //Metoda typu POST - vytvoření nového jídelníčku (data obdrží z formuláře)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ClientId,Name")] MealPlan MealPlan)
        {
            if (ModelState.IsValid)
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                MealPlan.UserId = userId;
                MealPlan.DateOfLastModification = DateTime.Now;
                _context.Add(MealPlan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(MealPlan);
        }


        //Metoda typu POST - úprava názvu jídelníčku / klienta (data obdrží z formuláře)
        [HttpPost]
        public IActionResult EditMealPlanItemInfo(int? id, string changePlanName, int? newClientId)
        {

            MealPlan MealPlan = _context.MealPlans.Find(id);
            MealPlan.Name = changePlanName;
            MealPlan.ClientId = newClientId;            
            if (MealPlan.Name == null)
            {
                return RedirectToAction("Index", "MealPlanItem", new { id });
            }
            _context.Update(MealPlan);
            _context.SaveChanges();
            return RedirectToAction("Index", "MealPlanItem", new { id });
        }


        //Metoda typu GET - smazání jídelníčku (zobrazení dialogu)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var MealPlan = await _context.MealPlans
                .FirstOrDefaultAsync(m => m.Id == id);
            if (MealPlan == null)
            {
                return NotFound();
            }

            return View(MealPlan);
        }


        //Metoda typu POST - smazání jídelníčku (po potvrzení smaže jídelníček dle jeho id)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var MealPlan = await _context.MealPlans.FindAsync(id);
            _context.MealPlans.Remove(MealPlan);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        

        private bool MealPlanExists(int id)
        {
            return _context.MealPlans.Any(e => e.Id == id);
        }
    }
}
