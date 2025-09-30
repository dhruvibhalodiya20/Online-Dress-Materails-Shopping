using iText.Commons.Actions.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mypro.Models;
using System.Security.Claims;

namespace Mypro.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext abc;

        public UserController(ApplicationDbContext context)
        {
            abc = context;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                ViewBag.UserName = User.Identity.Name;
            }
            var queries = abc.ContactMessages.ToList(); 
            return View(queries);
           
        }

        public IActionResult EditProfile()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToAction("Login", "Account");

            var customer = abc.Customers.Include(c => c.City)
                                        .FirstOrDefault(c => c.Email == userEmail);

            if (customer == null)
                return NotFound();

            ViewBag.Cities = abc.Cities.ToList();
            return View(customer);
        }



        [HttpPost]
        public IActionResult EditProfile(Customer c, IFormFile? ProfilePhoto)
        {

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var old =abc.Customers.FirstOrDefault(x => x.Email == userEmail);

            if (old == null)
            {
                return NotFound();
            }

            old.CustomerName = c.CustomerName;
            old.ContactNumber = c.ContactNumber;
            old.Gender = c.Gender;
            old.Address = c.Address;
            old.CityId = c.CityId;

            if (ProfilePhoto != null && ProfilePhoto.Length > 0)
            {
                var ext = Path.GetExtension(ProfilePhoto.FileName).ToLower();
                if(ext !=".jpg" &&  ext !=".jpeg" && ext !=".png")
                {
                    ModelState.AddModelError("Profilephoto", "Only JPG,JPEG and PNG Allowed. ");
                    return View(c);
                }

                String path = Path.Combine("wwwroot/uploads",Guid.NewGuid()+ext);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    ProfilePhoto.CopyTo(stream);
                }
                old.PhotoPath= Path.GetFileName(path);
            }
            abc.Customers.Update(old);
            abc.SaveChanges();

            TempData["SucessMessage"] = "Profile updated succesfully !";
            return RedirectToAction("Index");
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactMessage model)
        {
            if (ModelState.IsValid)
            {
                abc.ContactMessages.Add(model);
                await abc.SaveChangesAsync();

                return Json(new { success = true, message = "Your message has been sent successfully!" });
            }

            return Json(new { success = false, message = "Please fill all required fields correctly." });
        }


    }
}
