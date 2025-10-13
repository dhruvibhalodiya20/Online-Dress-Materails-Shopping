using iText.Commons.Actions.Contexts;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using MailKit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Mypro.Models;
using Mypro.Models; 
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using X.PagedList;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Claims;



namespace Mypro.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _abc;

        public AccountController(ApplicationDbContext context)
        {
            _abc = context;

            var defaultCity = _abc.Cities.FirstOrDefault(c => c.CityName == "Default");
            if (defaultCity == null)
            {
                defaultCity = new City { CityName = "Default" };
                _abc.Cities.Add(defaultCity);
                _abc.SaveChanges();
            }

           
            if (!_abc.Customers.Any(c => c.Email == "dhruvibhalodiya20@gmail.com"))
            {
                var admin = new Customer
                {
                    CustomerName = "Admin",
                    Email = "dhruvibhalodiya20@gmail.com",
                    Password = HashPassword("Dhruvi@20"),
                    Role = "Admin",
                    CityId = defaultCity.CityId,
                    Address = "Default",
                    Gender = "Other",
                    ContactNumber = "0000000000"
                    //Hobby = "None"
                };
                _abc.Customers.Add(admin);
                _abc.SaveChanges();
            }
        }

        // GET: Register
        public IActionResult Register()
        {
            ViewBag.Cities = _abc.Cities.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(
            string CustomerName, string Email, string Password, string ContactNumber,
            string Gender, string Address, int CityId)
        {
            ViewBag.Cities = _abc.Cities.ToList();

            // Validation: Email duplicate
            if (_abc.Customers.Any(u => u.Email == Email))
            {
                ViewBag.Error = "Email already registered.";
                return View();
            }
            // Validation: Contact number duplicate
            if (_abc.Customers.Any(u => u.ContactNumber == ContactNumber))
            {
                ViewBag.Error = "Contact number already registered.";
                return View();
            }
            // Validation: Name is required
            if (string.IsNullOrWhiteSpace(CustomerName))
            {
                ViewBag.Error = "Please enter your name.";
                return View();
            }

            TempData["CustomerName"] = CustomerName;
            TempData["Email"] = Email;
            TempData["Password"] = HashPassword(Password);
            TempData["ContactNumber"] = ContactNumber;
           // TempData["Hobby"] = string.Join(",", Hobby);
            TempData["Gender"] = Gender;
            TempData["Address"] = Address;
            TempData["CityId"] = CityId;
            TempData["Role"] = "User";

            var city = await _abc.Cities.FirstOrDefaultAsync(c => c.CityId == CityId);
            string cityName = city?.CityName ?? "Unknown";

            // OTP generation
            Random rnd = new Random();
            var generatedOtp = rnd.Next(100000, 999999).ToString();
            TempData["OTP"] = generatedOtp;

            // Email OTP
            var message = new MailMessage();
            message.To.Add(Email);
            message.From = new MailAddress("dhruvibhalodiya321@gmail.com", "Customer Registration OTP");
            message.Subject = "OTP for Registration";
            message.IsBodyHtml = true;
            message.Body = $@"
        <div style='font-family: Arial, sans-serif; color: #333;'>
            <h2 style='color: #2E86C1;'>Welcome, {CustomerName}!</h2>
            <p>Thank you for registering!</p>
            <p style='font-size: 18px;'>
                <strong>Your OTP is:</strong> 
                <span style='color: #E74C3C;'>{generatedOtp}</span>
            </p>
            <hr style='border: 1px solid #ddd;'/>
            <h4>Your Registration Details:</h4>
            <ul style='list-style-type: none; padding: 0;'>
                <li><strong>Name:</strong> {CustomerName}</li>
                <li><strong>Email:</strong> {Email}</li>
               
                <li><strong>Gender:</strong> {Gender}</li>
                <li><strong>City:</strong> {cityName}</li>
                <li><strong>Address:</strong> {Address}</li>
            </ul>
            <p style='margin-top: 20px; color: #555;'>
                Please enter this OTP to complete your registration. This OTP is valid for 10 minutes.
            </p>
        </div>";

            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("dhruvibhalodiya321@gmail.com", "oauq saqy ohid shxd"),
                EnableSsl = true
            };
            await client.SendMailAsync(message);

            return RedirectToAction("VerifyOtp");
        }


        // GET: OTP Page
        public IActionResult VerifyOtp()
        {
            TempData.Keep();
            return View();
        }

        // POST: OTP Verification
        [HttpPost]
        public IActionResult VerifyOtp(string enteredOtp)
        {
            TempData.Keep();

            if (TempData["OTP"] == null || TempData["Email"] == null || TempData["Password"] == null)
            {
                ViewBag.Error = "Session expired or invalid access. Please register again.";
                return RedirectToAction("Register");
            }

            var savedOtp = TempData["OTP"]?.ToString();
            if (enteredOtp == savedOtp)
            {
                var customer = new Customer
                {
                    CustomerName = TempData["CustomerName"]?.ToString() ?? "Unknown",
                    Email = TempData["Email"]?.ToString(),
                    Password = TempData["Password"]?.ToString(),
                    ContactNumber = TempData["ContactNumber"]?.ToString(),
                  //  Hobby = TempData["Hobby"]?.ToString(),
                    Gender = TempData["Gender"]?.ToString(),
                    Address = TempData["Address"]?.ToString(),
                    CityId = TempData["CityId"] != null ? Convert.ToInt32(TempData["CityId"]) : 0,
                    Role = TempData["Role"].ToString()
                };

                _abc.Customers.Add(customer);
                _abc.SaveChanges();

               
                var pdfBytes = GenerateWelcomePdf(customer);
                SendWelcomeEmail(customer, pdfBytes);

                TempData.Clear();
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Error = "Invalid OTP.";
            return View();
        }



        // Generate PDF
        private byte[] GenerateWelcomePdf(Customer customer)
        {
            using (var ms = new MemoryStream())
            using (var writer = new PdfWriter(ms))
            using (var pdf = new PdfDocument(writer))
            {
                var doc = new Document(pdf);

               
                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

               
                doc.Add(new Paragraph("Welcome to Ajay Fashion")
                    .SetFont(boldFont)
                    .SetFontSize(20)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetMarginBottom(20));

               
                doc.Add(new Paragraph($"\nDear {customer.CustomerName},\n\n")
                    .SetFontSize(14));

               
                doc.Add(new Paragraph("We are pleased to inform you that your registration has been successfully completed.\n\n"));

               
                doc.Add(new Paragraph($"Email: {customer.Email}"));
                doc.Add(new Paragraph($"Contact: {customer.ContactNumber}"));
                doc.Add(new Paragraph($"Gender: {customer.Gender}"));
                doc.Add(new Paragraph($"Address: {customer.Address}"));

               
                doc.Add(new Paragraph("\nOur Shop Gallery")
                    .SetFont(boldFont)
                    .SetFontSize(16)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetMarginTop(20)
                    .SetMarginBottom(10));

                
                var tableTop = new iText.Layout.Element.Table(3); 
                tableTop.SetWidth(iText.Layout.Properties.UnitValue.CreatePercentValue(100));

                string[] topImagePaths = {
                   Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/template/assets/img/shop1.jpg"),
                   Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/template/assets/img/shop2.jpg"),
                   Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/template/assets/img/shop3.jpg")
};

                foreach (var path in topImagePaths)
                {
                    if (System.IO.File.Exists(path))
                    {
                        var img = new iText.Layout.Element.Image(iText.IO.Image.ImageDataFactory.Create(path))
                            .SetAutoScale(true)
                            .SetBorder(new iText.Layout.Borders.SolidBorder(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY, 1));

                        var cell = new iText.Layout.Element.Cell()
                            .Add(img)
                            .SetPadding(5)
                            .SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                        tableTop.AddCell(cell);
                    }
                }

                doc.Add(tableTop);

               
                string cardImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/template/assets/img/card.jpg");
                if (System.IO.File.Exists(cardImagePath))
                {
                    var cardImage = new iText.Layout.Element.Image(iText.IO.Image.ImageDataFactory.Create(cardImagePath))
                        .SetAutoScale(true)
                        .SetBorder(new iText.Layout.Borders.SolidBorder(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY, 1));

                    var fullWidthTable = new iText.Layout.Element.Table(1); 
                    fullWidthTable.SetWidth(iText.Layout.Properties.UnitValue.CreatePercentValue(100));

                    var cell = new iText.Layout.Element.Cell()
                        .Add(cardImage)
                        .SetPadding(5)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                    fullWidthTable.AddCell(cell);

                    doc.Add(fullWidthTable);
                }



                doc.Add(new Paragraph("\nOur Address:\n2/25, Ajay Fashion\nGujarat Housing Board,\nBombay Market, Umarwada,\nSurat, Gujarat.\n")
                    .SetFontSize(12));

             
                doc.Add(new Paragraph("Thank you for choosing us. We look forward to serving you!\n"));

                doc.Close();
                return ms.ToArray();
            }
        }

      
        private void SendWelcomeEmail(Customer customer, byte[] pdfAttachment)
        {
            var message = new MailMessage();
            message.From = new MailAddress("dhruvibhalodiya321@gmail.com", "Ajay Fashion");
            message.To.Add(customer.Email);
            message.Subject = "Welcome to Ajay Fashion – Registration Successful";
            message.Body = $@"Dear {customer.CustomerName},

            Welcome to Ajay Fashion! We are pleased to inform you that your registration has been successfully completed.


            Thank you for choosing us.

             Best regards,
             Team Ajay Fashion";
            message.Attachments.Add(new Attachment(new MemoryStream(pdfAttachment), "AjayFashion_Welcome.pdf", "application/pdf"));

            using (var smtp = new SmtpClient("smtp.gmail.com", 587))
            {
                smtp.Credentials = new NetworkCredential("dhruvibhalodiya321@gmail.com", "oauq saqy ohid shxd");
                smtp.EnableSsl = true;
                smtp.Send(message);
            }
        }

        // GET: Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            string hashedPassword = HashPassword(Password);

            var user = _abc.Customers
                .Include(c => c.City)
                .FirstOrDefault(c => c.Email.ToLower() == Email.ToLower() && c.Password == hashedPassword);

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            TempData["UserName"] = user.CustomerName;
            TempData.Keep("UserName");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.CustomerName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return user.Role == "Admin"
                ? RedirectToAction("AdminDashboard")
                : RedirectToAction("Index", "User");
        }

        // Forgot password
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string Email)
        {
            var user = _abc.Customers.FirstOrDefault(u => u.Email == Email);
            if (user == null)
            {
                ViewBag.Error = "Email not registered.";
                return View();
            }

            Random rnd = new Random();
            var generatedOtp = rnd.Next(100000, 999999).ToString();
            TempData["ForgotPasswordOTP"] = generatedOtp;
            TempData["ForgotPasswordEmail"] = Email;

            var message = new MailMessage();
            message.To.Add(Email);
            message.From = new MailAddress("dhruvibhalodiya321@gmail.com", "Password Reset OTP");
            message.Subject = "OTP for Password Reset";
            message.IsBodyHtml = true;
            message.Body = $"<h2>Password Reset OTP</h2><p>Your OTP is: <strong>{generatedOtp}</strong></p>";

            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("dhruvibhalodiya321@gmail.com", "oauq saqy ohid shxd"),
                EnableSsl = true
            };
            await client.SendMailAsync(message);

            return RedirectToAction("ResetPasswordOtp");
        }

        public IActionResult ResetPasswordOtp()
        {
            TempData.Keep();
            return View();
        }

        [HttpPost]
        public IActionResult ResetPasswordOtp(string enteredOtp, string NewPassword)
        {
            TempData.Keep();

            if (TempData["ForgotPasswordOTP"] == null || TempData["ForgotPasswordEmail"] == null)
            {
                ViewBag.Error = "Session expired. Please try again.";
                return RedirectToAction("ForgotPassword", "Account");
            }

            var savedOtp = TempData["ForgotPasswordOTP"]?.ToString();
            if (enteredOtp != savedOtp)
            {
                ViewBag.Error = "Invalid OTP.";
                return View();
            }

            var email = TempData["ForgotPasswordEmail"]?.ToString();
            var user = _abc.Customers.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                user.Password = HashPassword(NewPassword);
                _abc.SaveChanges();
            }

            TempData.Clear();
            return RedirectToAction("Login", "Account");
        }

        [Authorize(Roles = "Admin")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult AdminDashboard()
        {

            if (TempData["UserName"] != null)
            {
                ViewBag.UserName = TempData["UserName"].ToString();
                TempData.Keep("UserName");
            }
            else
            {
                var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var customer = _abc.Customers.FirstOrDefault(c => c.Email == email);
                ViewBag.UserName = customer?.CustomerName ?? "Unknown";
            }

            ViewBag.TotalCustomers = _abc.Customers.Count();
            ViewBag.TotalCities = _abc.Cities.Count();
            return View();
        }

        [Authorize(Roles = "User")]
        public IActionResult UserDashboard()
        {
            if (TempData["UserName"] != null)
            {
                ViewBag.UserName = TempData["UserName"].ToString();
                TempData.Keep("UserName");
            }
            else
            {
                var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var customer = _abc.Customers.FirstOrDefault(c => c.Email == email);
                ViewBag.UserName = customer?.CustomerName ?? "Unknown";
            }
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }










        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToWishlist(int imageId)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToAction("Login", "Account");

            var image = _abc.CategoryImages.FirstOrDefault(i => i.ImageId == imageId);
            if (image == null)
                return NotFound();

            var existing = _abc.Wishlist
                .FirstOrDefault(w => w.ImageId == imageId && w.UserEmail == userEmail);

            if (existing == null)
            {
                _abc.Wishlist.Add(new Wishlist
                {
                    ImageId = image.ImageId,
                    ImageUrl = image.ImagePath, 
                    UserEmail = userEmail
                });
                _abc.SaveChanges();
            }

            return RedirectToAction("Wishlist");
        }



        public IActionResult Wishlist()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var wishlistItems = _abc.Wishlist
                .Include(w => w.CategoryImage)
                .Where(w => w.UserEmail == userEmail)
                .ToList();

            return View(wishlistItems);
        }


        [HttpPost]
        public IActionResult RemoveFromWishlist(int id)
        {
            var wishlistItem = _abc.Wishlist.FirstOrDefault(w => w.Id == id);
            if (wishlistItem != null)
            {
                _abc.Wishlist.Remove(wishlistItem);
                _abc.SaveChanges();
            }

            return RedirectToAction("Wishlist"); 
        }




        // Add to Cart
        [HttpPost]
        public IActionResult AddToCart(int imageId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            // Use email instead of NameIdentifier
            var userId = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var image = _abc.CategoryImages.Find(imageId);
            if (image == null)
                return NotFound();

            var existingCartItem = _abc.CartItems
                .FirstOrDefault(c => c.ImageId == imageId && c.UserId == userId);

            if (existingCartItem != null)
            {
                existingCartItem.Quantity += 1;
            }
            else
            {
                var cartItem = new CartItem
                {
                    ImageId = imageId,
                    UserId = userId,
                    Quantity = 1,
                    Price = image.Price

                };
                _abc.CartItems.Add(cartItem);
            }

            _abc.SaveChanges();
            return RedirectToAction("Cart");
        }



        //public IActionResult Cart()
        //{
        //    if (!User.Identity.IsAuthenticated)
        //        return RedirectToAction("Login", "Account");

        //    var userId = User.FindFirstValue(ClaimTypes.Email); 

        //    var cartItems = _abc.CartItems
        //        .Include(c => c.Image)
        //        .ThenInclude(i => i.Category)
        //        .Where(c => c.UserId == userId)
        //        .ToList();

        //    return View(cartItems);
        //}


        //public IActionResult Cart()
        //{
        //    if (!User.Identity.IsAuthenticated)
        //        return RedirectToAction("Login", "Account");

        //    var userId = User.FindFirstValue(ClaimTypes.Email);

        //    var cartItems = _abc.CartItems
        //        .Include(c => c.Image)
        //        .ThenInclude(i => i.Category)
        //        .Where(c => c.UserId == userId)
        //        .ToList();

        //    // Determine if first order
        //    bool isFirstOrder = !_abc.Orders.Any(o => o.UserId == userId);
        //    ViewBag.IsFirstOrder = isFirstOrder;
        //    ViewBag.OrderDiscountPercent = isFirstOrder ? 5m : 2m;

        //    // date-based discount
        //    var day = DateTime.Now.Day;
        //    decimal dateDiscountPercent = day <= 10 ? 4m : day <= 20 ? 3m : 2m;
        //    ViewBag.DateDiscountPercent = dateDiscountPercent;

        //    return View(cartItems);
        //}

        public IActionResult Cart()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var userId = User.FindFirstValue(ClaimTypes.Email);
 
            var cartItems = _abc.CartItems
                .Include(c => c.Image)
                .ThenInclude(i => i.Category)
                .Where(c => c.UserId == userId)
                .ToList();

           
            using (var conn = new SqlConnection(_abc.Database.GetDbConnection().ConnectionString))
            using (var cmd = new SqlCommand("sp_GetCartSummary", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserId", userId ?? (object)DBNull.Value);
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    
                    if (reader.HasRows)
                    {
                        while (reader.Read()) {  }
                    }

                    if (reader.NextResult()) 
                    {
                        while (reader.Read()) {  }
                    }

                    if (reader.NextResult() && reader.Read()) 
                    {
                        ViewBag.SubTotal = reader["SubTotal"];
                        ViewBag.OrderDiscountPercent = reader["OrderDiscountPercent"];
                        ViewBag.OrderDiscountAmount = reader["OrderDiscountAmount"];
                        ViewBag.DateDiscountPercent = reader["DateDiscountPercent"];
                        ViewBag.DateDiscountAmount = reader["DateDiscountAmount"];
                        ViewBag.TotalDiscountAmount = reader["TotalDiscountAmount"];
                        ViewBag.GSTPercent = reader["GSTPercent"];
                        ViewBag.GSTAmount = reader["GSTAmount"];
                        ViewBag.FinalAmount = reader["FinalAmount"];
                        ViewBag.IsFirstOrder = reader["IsFirstOrder"];
                    }

                }
            }

            return View(cartItems);
        }


        [HttpPost]
        public IActionResult RemoveFromCart(int id)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var userId = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var cartItem = _abc.CartItems.FirstOrDefault(c => c.Id == id && c.UserId == userId);
            if (cartItem == null)
                return NotFound();

            _abc.CartItems.Remove(cartItem);
            _abc.SaveChanges();

            return RedirectToAction("Cart"); 
        }
































        //[Authorize(Roles = "Admin")]
        //public IActionResult CreateCustomer()
        //{
        //    ViewBag.Cities = _abc.Cities.ToList();
        //    return View();
        //}

        //[HttpPost]
        //[Authorize(Roles = "Admin")]
        //public IActionResult CreateCustomer(Customer customer)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        customer.Password = HashPassword(customer.Password ?? "123456"); 
        //        _abc.Customers.Add(customer);
        //        _abc.SaveChanges();
        //        return RedirectToAction("ManageCustomers");
        //    }
        //    ViewBag.Cities = _abc.Cities.ToList();
        //    return View(customer);
        //}

        //[Authorize(Roles = "Admin")]
        //public IActionResult EditCustomer(int id)
        //{
        //    var customer = _abc.Customers.Find(id);
        //    if (customer == null) return NotFound();

        //    ViewBag.Cities = _abc.Cities.ToList();
        //    return View(customer);
        //}

        //[HttpPost]
        //[Authorize(Roles = "Admin")]
        //public IActionResult EditCustomer(Customer customer)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _abc.Customers.Update(customer);
        //        _abc.SaveChanges();
        //        return RedirectToAction("ManageCustomers");
        //    }
        //    ViewBag.Cities = _abc.Cities.ToList();
        //    return View(customer);
        //}





        //ADmin
        [Authorize(Roles = "Admin")]
        public IActionResult ManageCustomers(string searchBy, string searchtext)
        {
            var customers = _abc.Customers.Include(c => c.City).AsQueryable();

            if (!string.IsNullOrEmpty(searchtext))
            {
                switch (searchBy)
                {
                    case "Name":
                        customers = customers.Where(c => c.CustomerName.Contains(searchtext));
                        break;
                    case "Email":
                        customers = customers.Where(c => c.Email.Contains(searchtext));
                        break;
                    case "Contact":
                        customers = customers.Where(c => c.ContactNumber.Contains(searchtext));
                        break;
                }
            }

            ViewBag.SearchBy = searchBy;
            ViewBag.Searchtext = searchtext;
            return View(customers.ToList());
        }

        [Authorize(Roles = "Admin")]
        public IActionResult DeleteCustomer(int id)
        {
            var customer = _abc.Customers.Find(id);
            if (customer != null)
            {
                _abc.Customers.Remove(customer);
                _abc.SaveChanges();
            }
            return RedirectToAction("ManageCustomers");
        }



        public IActionResult ManageCities()
        {
            var cities = _abc.Cities.ToList();
            return View(cities);
        }

      
        public IActionResult CreateCity()
        {
            return View();
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateCity(City city)
        {
            if (!ModelState.IsValid)
            {


                //if (_abc.Cities.Any(c => c.CityName == city.CityName))
                //{
                //    ModelState.AddModelError("CityName", "This city already exists.");
                //    return View(city);
                //}

                _abc.Cities.Add(city);
                _abc.SaveChanges();
                return RedirectToAction("ManageCities", "Account");
            }
            return View(city);
        }


        public IActionResult EditCity(int id)
        {
            var city = _abc.Cities.FirstOrDefault(c => c.CityId == id);
            if (city == null)
                return NotFound();

            return View(city);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCity(int CityId, string CityName)
        {
            if (string.IsNullOrWhiteSpace(CityName))
            {
                ModelState.AddModelError("CityName", "City name is required.");
                return View(new City { CityId = CityId, CityName = CityName });
            }

           
            if (_abc.Cities.Any(c => c.CityName == CityName && c.CityId != CityId))
            {
                ModelState.AddModelError("CityName", "This city name already exists.");
                return View(new City { CityId = CityId, CityName = CityName });
            }

            var existingCity = _abc.Cities.FirstOrDefault(c => c.CityId == CityId);
            if (existingCity == null)
                return NotFound();

            existingCity.CityName = CityName;
            _abc.SaveChanges();

            
            return RedirectToAction("ManageCities", "Account");
        }



        public IActionResult DeleteCity(int id)
        {
            var city = _abc.Cities.Find(id);
            if (city == null) return NotFound();

            _abc.Cities.Remove(city);
            _abc.SaveChanges();
            return RedirectToAction("ManageCities");
        }












        public IActionResult ManageCategories()
        {
            var categories = _abc.Categories.ToList();
            return View(categories);
        }

        public IActionResult AddCategory()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddCategory(Mypro.Models.Category category)
        {
            if (ModelState.IsValid)
            {
                _abc.Categories.Add(category);
                _abc.SaveChanges();
                return RedirectToAction("ManageCategories","Account");
            }
            return View(category);
        }

        public IActionResult EditCategory(int id)
        {
            var category = _abc.Categories.Find(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        public IActionResult EditCategory(Mypro.Models.Category category)
        {
            if (ModelState.IsValid)
            {
                _abc.Categories.Update(category);
                _abc.SaveChanges();
                return RedirectToAction("ManageCategories", "Account");
            }
            return View(category);
        }

        public IActionResult DeleteCategory(int id)
        {
            var category = _abc.Categories.Find(id);
            if (category != null)
            {
                _abc.Categories.Remove(category);
                _abc.SaveChanges();
            }
            return RedirectToAction("ManageCategories", "Account");
        }









        public IActionResult ManageCategoryImages(string selectedColor, int? selectedCategory, int? page, int? pageSize)
        {
           
            int currentPageSize = pageSize ?? 10;
            int pageNumber = page ?? 1;

            // Categories list
            ViewBag.Categories = _abc.Categories
                                     .Select(c => new { c.CategoryId, c.CategoryName })
                                     .ToList();

            // Colors list
            ViewBag.Colors = _abc.CategoryImages
                                 .Select(c => c.Color)
                                 .Where(c => !string.IsNullOrEmpty(c))
                                 .Distinct()
                                 .ToList();

            // Query
            var query = _abc.CategoryImages.Include(c => c.Category).AsQueryable();

            if (!string.IsNullOrEmpty(selectedColor))
            {
                query = query.Where(c => c.Color == selectedColor);
            }

            if (selectedCategory.HasValue && selectedCategory.Value > 0)
            {
                query = query.Where(c => c.CategoryId == selectedCategory.Value);
            }

            // Selected filters back to view
            ViewBag.SelectedColor = selectedColor;
            ViewBag.SelectedCategory = selectedCategory;
            ViewBag.PageSize = currentPageSize; 

            // Fetch + paginate
            var images = query.OrderBy(c => c.ImageId)
                              .ToList()
                              .ToPagedList(pageNumber, currentPageSize);

            return View(images);
        }



        //public IActionResult ManageCategoryImages(int? page)
        //{
        //    int pageSize = 10;  
        //    int pageNumber = page ?? 1;

        //    var images = _abc.CategoryImages
        //                     .Include(c => c.Category)
        //                     .OrderBy(c => c.ImageId)
        //                     .ToList() 
        //                     .ToPagedList(pageNumber, pageSize);

        //    return View(images);
        //}



        //public IActionResult AddCategoryImage()
        //{
        //    ViewBag.Categories = new SelectList(_abc.Categories.ToList(), "CategoryId", "CategoryName");
        //    return View();
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult AddCategoryImage(CategoryImage image)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        ViewBag.Categories = new SelectList(_abc.Categories.ToList(), "CategoryId", "CategoryName");
        //        return View(image);
        //    }

        //    _abc.CategoryImages.Add(image);
        //    _abc.SaveChanges();

        //    return RedirectToAction("ManageCategoryImages", "Account");
        //}

        [HttpGet]
        public IActionResult AddCategoryImage()
        {
            ViewBag.Categories = new SelectList(_abc.Categories.ToList(), "CategoryId", "CategoryName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddCategoryImage(CategoryImageVM model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_abc.Categories.ToList(), "CategoryId", "CategoryName", model.CategoryId);
                return View(model);
            }

            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/template/assets/img");
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
            var filePath = Path.Combine(uploadFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                model.ImageFile.CopyTo(stream);
            }

            var image = new CategoryImage
            {
                ImagePath = $"/template/assets/img/{uniqueFileName}", 
                CategoryId = model.CategoryId,
                Color = model.Color,
                HexColor = model.HexColor
            };

            _abc.CategoryImages.Add(image);
            _abc.SaveChanges();

            var category = _abc.Categories.FirstOrDefault(c => c.CategoryId == model.CategoryId);
            var notification = new Notification
            {
                Title = "New Arrival!",
                Message = $"New {category?.CategoryName} item added - {model.Color}",
                ImagePath = image.ImagePath,
                CategoryId = model.CategoryId,
                ImageId = image.ImageId,
                CreatedDate = DateTime.Now,
                IsRead = false,
                CustomerId = null 
            };

            _abc.Notifications.Add(notification);
            _abc.SaveChanges();

            return RedirectToAction("ManageCategoryImages", "Account");
        }


        [HttpGet]
        public IActionResult EditCategoryImage(int id)
        {
            var image = _abc.CategoryImages.FirstOrDefault(x => x.ImageId == id);
            if (image == null) return NotFound();

            ViewBag.Categories = _abc.Categories.ToList();
            return View(image);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCategoryImage(CategoryImage model, IFormFile? ImageFile)
        {
            
            ModelState.Remove(nameof(CategoryImage.ImagePath));
            ModelState.Remove(nameof(CategoryImage.Category));

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _abc.Categories.ToList();
                return View(model);
            }

            var existing = _abc.CategoryImages.FirstOrDefault(x => x.ImageId == model.ImageId);
            if (existing == null) return NotFound();

            
            existing.CategoryId = model.CategoryId;
            existing.Color = model.Color;
            existing.HexColor = model.HexColor;
            existing.Description = model.Description;


            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/template/assets/img");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(uploadDir, uniqueName);

                using (var fs = new FileStream(filePath, FileMode.Create))
                    ImageFile.CopyTo(fs);

               
                existing.ImagePath = $"/template/assets/img/{uniqueName}";
            }

            _abc.SaveChanges();

           
            return RedirectToAction("ManageCategoryImages", "Account");
        }




        public IActionResult DeleteCategoryImage(int id)
        {
            var image = _abc.CategoryImages.Find(id);
            if (image != null)
            {
                _abc.CategoryImages.Remove(image);
                _abc.SaveChanges();
            }
            return RedirectToAction("ManageCategoryImages", "Account");
        }


        [HttpGet]
        public IActionResult UploadProfilePhoto()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> UploadProfilePhoto(IFormFile Photo)
        {
            if (Photo != null && Photo.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Photo.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Photo.CopyToAsync(stream);
                }

                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                var customer = await _abc.Customers.FirstOrDefaultAsync(u => u.Email == email);
                if (customer != null)
                {
                    customer.PhotoPath = fileName;
                    _abc.Update(customer);
                    await _abc.SaveChangesAsync();

                   
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var photoClaim = claimsIdentity.FindFirst("PhotoPath");
                    if (photoClaim != null)
                        claimsIdentity.RemoveClaim(photoClaim);

                    claimsIdentity.AddClaim(new Claim("PhotoPath", fileName));
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));

                    TempData["SuccessMessage"] = "Profile photo uploaded successfully!";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Please select a valid photo!";
            }

            return RedirectToAction("AdminDashboard", "Account");
        }

        public IActionResult CityGraph()
        {
            var cityData = _abc.Customers
                .GroupBy(c => c.City.CityName)
                .Select(g => new
                {
                    CityName = g.Key,
                    CustomerCount = g.Count()
                })
                .ToList();

            ViewBag.CityNames = cityData.Select(x => x.CityName).ToList();
            ViewBag.CustomerCounts = cityData.Select(x => x.CustomerCount).ToList();

            return View();
        }



        // GET: /Account/Pricing
        public IActionResult Pricing(int? categoryId)
        {
            var categories = _abc.Categories.ToList();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", categoryId);


            var images = _abc.CategoryImages.Include(c => c.Category).AsQueryable();

            if (categoryId.HasValue)
            {
                images = images.Where(x => x.CategoryId == categoryId.Value);
            }

            return View(images.ToList());

        }

        [HttpPost]
        public async Task<IActionResult> UpdatePrice(int imageId, decimal price)
        {
            var image = await _abc.CategoryImages.FindAsync(imageId);
            if (image != null)
            {
                image.Price = price;
                await _abc.SaveChangesAsync();
            }
            return RedirectToAction("Pricing");
        }

        // List all orders
        public IActionResult Order()
        {
            var orders = _abc.Orders.Include(o => o.OrderItems).ToList();

            return View(orders);
        }

        // Show order details
        public IActionResult orderdetails(int id)
        {
            var order = _abc.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null) return NotFound();

            return View(order);
        }

        [HttpPost]
        public IActionResult ToggleAvailability(int id, bool isAvailable)
        {
            var order = _abc.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null) return NotFound();

            order.IsAvailable = isAvailable;
            _abc.SaveChanges();

            string status = isAvailable ? "Available" : "Not Available";
            return Json(new { success = true, status });
        }


        //public IActionResult Query()
        //{
        //    var queries = _abc.ContactMessages
        //                      .OrderByDescending(q => q.CreatedAt)
        //                      .Select(q => new ContactMessage
        //                      {
        //                          Id = q.Id,
        //                          Name = q.Name,
        //                          Email = q.Email,
        //                          Subject = q.Subject,
        //                          Message = q.Message,
        //                          CreatedAt = q.CreatedAt,
        //                          Reply = q.Reply ?? ""   
        //                      })
        //                      .ToList();

        //    return View(queries);
        //}



   
        public IActionResult Query()
        {
            var queries = _abc.ContactMessages
                              .FromSqlRaw("EXEC sp_GetAllContactMessages")
                              .AsEnumerable()
                              .ToList();

            return View(queries);
        }

        [HttpPost]
        public async Task<IActionResult> Reply(int id, string reply)
        {
            var query = await _abc.ContactMessages.FindAsync(id);
            if (query == null)
                return Json(new { success = false, message = "Query not found." });

            query.Reply = reply;
            query.ReplyAt = DateTime.Now;

            await _abc.SaveChangesAsync();

            return Json(new { success = true, message = "Reply sent successfully.", reply = reply });
        }


    }



}

