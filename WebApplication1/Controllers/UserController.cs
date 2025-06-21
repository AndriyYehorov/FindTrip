using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Drawing.Printing;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
	[Authorize]
	public class UserController : Controller
    {
        private readonly ApplicationContext _context;     
        
        public UserController(ApplicationContext context)
        {
            _context = context;
        }

		[AllowAnonymous]
		public IActionResult Index(string returnUrl)
        {
            ViewData["RetUrl"] = returnUrl;
            return View();
        }

		[AllowAnonymous]
		public async Task<IActionResult> LoginAsync([Bind(Prefix = "l")] LoginViewModel model,string retUrl) {
            if (!ModelState.IsValid)
            {
                return View("Index", new UserViewModel
                {
                    LoginViewModel = model
                });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == model.Login && u.Password == model.Password);

            if (user == null) {

                ViewBag.Error = "Неправильний логін і(або) пароль";
                return View("Index", new UserViewModel
                {
                    LoginViewModel = model
                });
            }            

            await AuthenticateAsync(user);            

            if (retUrl!="empty")
            {
                return Redirect(retUrl);
            }

            return RedirectToAction("Index","Trip");
        }

		[AllowAnonymous]
		private async Task AuthenticateAsync(User user)
        {
            string role;

            if (!string.IsNullOrEmpty(user.Role))
                role = user.Role;
            else
                role = "user";     

            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, user.Id.ToString()),
                new (ClaimsIdentity.DefaultNameClaimType, user.Login),
                new (ClaimsIdentity.DefaultRoleClaimType, role)
            };

            var id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

		[AllowAnonymous]
		public async Task<IActionResult> RegisterAsync([Bind(Prefix = "r")] RegisterViewModel model, string retUrl)
        {
            if (!ModelState.IsValid)
                return View("Index", new UserViewModel
                {
                    RegisterViewModel = model
                });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == model.Login);

            if (user != null)
            {
                ViewBag.RegisterError = "Користувач з таким логіном уже існує!";
                return View("Index", new UserViewModel
                {
                    RegisterViewModel = model
                });
            }

			var userEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

			if (userEmail != null)
			{
				ViewBag.RegisterError = "Користувач з такою електронною поштою уже існує!";
				return View("Index", new UserViewModel
				{
					RegisterViewModel = model
				});
			}

            user = new User(model.Login, model.Password)
            {
                Email = model.Email,
                Role = "user",
                AmountOfReports = 0,
                AmountOfBadReports = 0
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            await AuthenticateAsync(user);

            if (retUrl != "empty")
            {
                return Redirect(retUrl);
            }
            
            return RedirectToAction("Index", "Trip");
        }

        public async Task<IActionResult> LogoutAsync()
        {            
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); 
            return RedirectToAction("Index", "Trip");
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AdminPanel(int showTab = 0, string errorMsg = "")
        {
			var requests = await _context.Requests.Where(r => r.Status == "On review").ToListAsync();           

			ViewData["Requests"] = requests;

            ViewData["ShowTab"] = showTab;

            if (errorMsg != "")
            {
                ViewBag.Error = errorMsg;
            }

			return View();
		}        

        public async Task<IActionResult> Profile()
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));

            var model = new ProfileViewModel(user.Login,user.Email);

            var statuses = await _context.Requests.Where(r => r.UserId == user.Id).Select(r => r.Status).ToListAsync();

            string userStatus = "";

            if (user.Role == "user")
            {  
                if (statuses.Contains("on review"))
                    userStatus = "on review";
                else if (statuses.Contains("declined"))
                    userStatus = "declined";
            }
            else
                userStatus = "accepted";

            ViewData["DriverModel"] = new DriverViewModel(user.Name,user.Surname,user.PhoneNumber);

            ViewData["UserStatus"] = userStatus;
            
            return View("Profile", model);
        }       

        public async Task<IActionResult> ChangePassword(ProfileViewModel model)
        {
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));

			if (!ModelState.IsValid)
            {
				var statuses = await _context.Requests.Where(r => r.UserId == user.Id).Select(r => r.Status).ToListAsync();

				string userStatus = "";

				if (user.Role == "user")
				{
					if (statuses.Contains("on review"))
						userStatus = "on review";
					else if (statuses.Contains("declined"))
						userStatus = "declined";
				}
				else
					userStatus = "accepted";

				ViewData["DriverModel"] = new DriverViewModel(user.Name, user.Surname, user.PhoneNumber);

				ViewData["UserStatus"] = userStatus;

				return View("Profile",model);
            }            
             
            user.Password = model.Password;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Profile");
        }

        public async Task<IActionResult> CreateDriverRequest(DriverViewModel model) 
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                var profModel = new ProfileViewModel(user.Login,user.Email);				

				return View("Profile",profModel);
            }

            var userPhone = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);

			if (userPhone != null)
			{
				var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
				var profModel = new ProfileViewModel(user.Login, user.Email);
				ViewBag.Errord = "Такий номер телефону вже зареєстровано";

				return View("Profile", profModel);
			}

			var fileVal = FileValidation(model.DriverLicense);

            if (fileVal != "") 
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                var profModel = new ProfileViewModel(user.Login, user.Email);
                ViewBag.Errord = fileVal;

                return View("Profile", profModel);
            }

            var request = new DriverRequest
            {
                Name = model.Name,
                Surname = model.Surname,
                PhoneNumber = model.PhoneNumber
            };

            using (MemoryStream ms = new())
            {
                await model.DriverLicense.CopyToAsync(ms);
                request.DriverLicense = ms.ToArray();                
            }

            request.UserId = userId;
            request.Status = "on review";

            await _context.Requests.AddAsync(request);
            await _context.SaveChangesAsync();

            return RedirectToAction("Profile");
        }

        private string FileValidation(IFormFile file)
        {
            int maxSize = 5000000; //в байтах (5Мб)
            string[] permittedExtensions = [".png", ".jpg", ".gif", ".jpeg"];            

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
            {
                StringBuilder extensionStr = new();

                foreach (var str in permittedExtensions)
                {
                    extensionStr.Append($"{str} ");
                }

                return "Файл повинен мати одне з наступних розширень: " + extensionStr.ToString();
            }

            else if(file.Length> maxSize)
            {
                return "Розмір файлу не повинен перевищувати 5Мб:";
            }

            return "";
        }

        [Authorize(Roles = "admin")]
		public async Task<IActionResult> DeclineRequest(int id) 
        {
            var req = await _context.Requests.FirstOrDefaultAsync(r => r.Id == id);

            if (req != null)
            {
                req.Status = "declined";

                _context.Requests.Update(req);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction("AdminPanel", new { showTab = 1 });
        }

		[Authorize(Roles = "admin")]
		public async Task<IActionResult> AcceptRequest(int id)
		{
			var req = await _context.Requests.FirstOrDefaultAsync(r => r.Id == id);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == req.UserId);

            if (req != null)
            {
                req.Status = "accepted";

                user.Role = "driver";
                user.Name = req.Name;
                user.Surname = req.Surname;
                user.DriverLicense = req.DriverLicense;
                user.PhoneNumber = req.PhoneNumber;

                _context.Requests.Update(req);
				_context.Users.Update(user);
				await _context.SaveChangesAsync();
            }

            
            return RedirectToAction("AdminPanel", new { showTab = 1 });
        }

		[Authorize(Roles = "admin")]
        public async Task<IActionResult> AddCity(string newCityName) 
        {
            var cities = await _context.Cities.Select(c => c.Name).ToListAsync();			

            var regExp = new Regex("^[а-яґєіїА-ЯҐЄІЇ]+$");            

            if (newCityName == null || newCityName.Length < 3 || newCityName.Length > 25 || !regExp.IsMatch(newCityName))
			{		
                return RedirectToAction("AdminPanel", new { errorMsg = "Назва міста повинна бути довжиною від 3 до 25 символів і містити тільки кирилицю" });
            }
            
			else if (cities.Contains(newCityName))
            {                				
                return RedirectToAction("AdminPanel", new { errorMsg = "Назва цього міста вже є у базі" });
            }

            var city = new City
            {
                Name = newCityName
            };

            await _context.Cities.AddAsync(city);
			await _context.SaveChangesAsync();			

			return RedirectToAction("AdminPanel");
		}

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UnbanDriver(string login)
        {            
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == login);

            if (user != null && user.isBanned == true)
            {
                user.AmountOfBadReports = 0;
                user.isBanned = false;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }

            else 
            {     
                return RedirectToAction("AdminPanel", new { showTab = 2 , errorMsg = "Такого користувача не існує або він не заблокований" });
            }

            return RedirectToAction("AdminPanel", new { showTab = 2 });
        }

		[Authorize(Roles = "admin")]
		public async Task<IActionResult> GetDriverReports(string login)
		{
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == login);					

			if (user != null){

				var reports = await _context.Reports.Where(r => r.ReportedId == user.Id).ToListAsync();
				ViewData["Reports"] = reports;
                ViewData["ShowTab"] = 2;
				return View("AdminPanel");
			}			

			else
			{               				
                return RedirectToAction("AdminPanel", new { showTab = 2, errorMsg = "Такого користувача не існує" });
            }
		}	
	}
}