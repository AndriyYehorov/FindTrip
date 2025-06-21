using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Filters;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{    
    public class TripController : Controller
    {
        private readonly ApplicationContext _context;

        public TripController(ApplicationContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        [EdgeFilter]
        public IActionResult Index()
        {           
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Index(TripViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index",model);
            }

            var cityValidation = AreCitiesValidInModel(model);

            if (!cityValidation.isValid)
            {
                ViewBag.Error = cityValidation.errorMessage;
                return View("Index", model);
            }

            var DepartureCity = await _context.Cities.FirstOrDefaultAsync(c => c.Name == model.DeparturePoint);
            var ArrivalCity = await _context.Cities.FirstOrDefaultAsync(c => c.Name == model.ArrivalPoint);

            var trips = await _context.Trips.Include(t => t.Creator).Where(
                                        tr => tr.DeparturePoint == DepartureCity &&
                                        tr.ArrivalPoint == ArrivalCity &&
                                        tr.AmountOfSeats >= model.AmountOfSeats &&
                                        tr.DepartureTime.Date >= model.DepartureTime.Date &&
                                        tr.DepartureTime >= DateTime.Now).OrderBy(t => t.DepartureTime).ToListAsync();

            var driversId = trips.Select(t => t.Creator.Id);
            var driversInfo = await _context.Users.Where(u => driversId.Contains(u.Id)).ToListAsync();

            var driversRatings = driversInfo.Select(d => d.Rating ?? 0);      

            ViewData["driverRatingsForAllTrips"] = driversRatings;

            ViewData["Trips"] = trips;
            ViewData["tripsIdWhereUserRegistrated"] = new List<int>();
            ViewData["tripsIdWhereUserIsCreator"] = new List<int>();

            if (User.Identity.IsAuthenticated)
            {
                var userId = GetUserId();
                var tripsIdWhereUserRegistrated = await _context.TripRegistrations.Where(tr => tr.UserId == userId).Select(tr => tr.TripId).ToListAsync();
                var tripsIdWhereUserIsCreator = trips.Where(tr => tr.Creator.Id == userId).Select(t => t.Id);
                
                ViewData["tripsIdWhereUserRegistrated"] = tripsIdWhereUserRegistrated;
                ViewData["tripsIdWhereUserIsCreator"] = tripsIdWhereUserIsCreator;
            }

            return View();           
        }        

        [HttpGet]
        [Authorize(Roles = "admin, driver")]
        public IActionResult Create()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<ActionResult> LoadCityAutoComplete(string Prefix)
        {
            var cities = await _context.Cities.Where(u => u.Name.StartsWith(Prefix)).Select(u => u.Name).ToListAsync(); 

            return Json(cities);
        }
              
        [HttpPost]
        [Authorize(Roles = "admin, driver")]
        public async Task<IActionResult> CreateTrip(TripViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Create", model);
            }

            var userId = GetUserId();

            var creator = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (creator?.isBanned == true)
            {
                ViewBag.Error = "Ви не можете створювати поїдки через надмірку кількість скарг, для розблокування зв'яжіться з адміністратором за номером телефону : +380353759714";
                return View("Create", model);
            }

            var cities = await _context.Cities.Select(c => c.Name).ToListAsync();

            var isValidCity = cities.Contains(model.DeparturePoint) && cities.Contains(model.ArrivalPoint);
            var isValidTime = model.DepartureTime > DateTime.Now.AddHours(1);
            var arePointsDifferent = model.ArrivalPoint != model.DeparturePoint;

            if (!(isValidCity && isValidTime && arePointsDifferent))
            {
                ViewBag.Error = GetErrorMessage(isValidCity, arePointsDifferent);
                return View("Create", model);
            }

            var DepartureCity = await _context.Cities.Where(c => c.Name == model.DeparturePoint).FirstOrDefaultAsync();
            var ArrivalCity = await _context.Cities.Where(c => c.Name == model.ArrivalPoint).FirstOrDefaultAsync();

            var trip = new Trip(model.DepartureTime,    
                                DepartureCity, 
                                ArrivalCity,
                                model.AmountOfSeats,
                                model.AmountOfSeats,
                                model.Price,
                                creator);     

            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyTrips");
        }

        [Authorize]
        public async Task<IActionResult> TripRegistration(int tripId)
        {
            var userId = GetUserId();
            var trip = await _context.Trips.Include(t => t.Creator).FirstOrDefaultAsync(t => t.Id == tripId);

            var userIsNotCreator = userId != trip?.Creator.Id;

            var registationsOnThisTripThisUser = await _context.TripRegistrations.Where(tr => tr.TripId == tripId && tr.UserId == userId).ToListAsync();               

            if (trip?.AmountOfFreeSeats > 0 && registationsOnThisTripThisUser.Count == 0 && userIsNotCreator)  
            {
                trip.AmountOfFreeSeats -= 1;

                var registration = new TripRegistration( userId , tripId);   
                
                _context.Trips.Update(trip);
                _context.TripRegistrations.Add(registration);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction("MyTrips");
        }

        [Authorize]
        public async Task<IActionResult> MyTrips(string sortOrder)
        {
            bool isVisibleEndedTab;

            if (sortOrder !=null && sortOrder.Last() == 'Q')
            {
                isVisibleEndedTab = true;
                sortOrder = sortOrder.Remove(sortOrder.Length - 1);
            }
            else
            {
                isVisibleEndedTab = false;
            }

            ViewData["isVisibleEndedTab"] = isVisibleEndedTab;           

            ViewData["DateSortParm"] = string.IsNullOrEmpty(sortOrder) ? "date_asc" : "";
            ViewData["PriceSortParm"] = sortOrder == "Price" ? "price_desc" : "Price";

            var userId = GetUserId();

            var thisUserRegistations = await _context.TripRegistrations.Where(tr => tr.UserId == userId).Select(tr => tr.TripId).ToListAsync();
            var thisUserTrips = await _context.Trips.Include(t => t.Creator).Include(t => t.ArrivalPoint).Include(t => t.DeparturePoint).Where(t => t.Creator.Id == userId || thisUserRegistations.Contains(t.Id)).OrderBy(t=> t.DepartureTime).ToListAsync();

            var endedTrips = new List<Trip>();
            var futureTrips = new List<Trip>();

            foreach (var trip in thisUserTrips)
            {
                if (trip.DepartureTime <= DateTime.Now)
                    endedTrips.Add(trip);
                else
                    futureTrips.Add(trip);
            }

            switch (sortOrder)
            {
                case "date_asc":
                    endedTrips = endedTrips.OrderBy(s => s.DepartureTime).ToList();
                    futureTrips = futureTrips.OrderByDescending(s => s.DepartureTime).ToList();
                    break;
                case "Price":
                    endedTrips = endedTrips.OrderBy(s => s.Price).ToList();
                    futureTrips = futureTrips.OrderBy(s => s.Price).ToList();
                    break;
                case "price_desc":
                    endedTrips = endedTrips.OrderByDescending(s => s.Price).ToList();
                    futureTrips = futureTrips.OrderByDescending(s => s.Price).ToList();
                    break;
                default:
                    endedTrips = endedTrips.OrderByDescending(s => s.DepartureTime).ToList();
                    futureTrips = futureTrips.OrderBy(s => s.DepartureTime).ToList();
                    break;
            }

            ViewData["EndedTrips"] = endedTrips;
            ViewData["FutureTrips"] = futureTrips;
            ViewData["UserId"] = userId;  

            return View();
        }

        [Authorize]
        public async Task<IActionResult> CancelTripRegistration(int tripId) 
        {
            var userId = GetUserId();
            var trip = await _context.Trips.FirstOrDefaultAsync(t => t.Id == tripId);

            var registationOnThisTripThisUser = await _context.TripRegistrations.FirstOrDefaultAsync(tr => tr.TripId == tripId && tr.UserId == userId);

            if (registationOnThisTripThisUser != null)
            {               
                trip.AmountOfFreeSeats += 1;

                _context.Trips.Update(trip);
                _context.TripRegistrations.Remove(registationOnThisTripThisUser);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("MyTrips");
        }

        [Authorize]
        public async Task<IActionResult> CancelTrip(int tripId)
        {
            var userId = GetUserId();
            var trip = await _context.Trips.Include(t => t.Creator).FirstOrDefaultAsync(t => t.Id == tripId);

            var userIsCreator = userId == trip?.Creator.Id;

            var registationsOnThisTrip = await _context.TripRegistrations.Where(tr => tr.TripId == tripId).ToListAsync();           

            if (userIsCreator)
            {
                foreach (var registration in registationsOnThisTrip)
                {
                    _context.TripRegistrations.Remove(registration);
                }

                _context.Trips.Remove(trip);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("MyTrips");
        }
        [Authorize]
        [HttpGet]
        public IActionResult Report(int tripId)
        {
            return View(new ReportViewModel() { TripId = tripId });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Report(ReportViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Report", model);
            }
            
            var userId = GetUserId();            

            var tripRegistrations = await _context.TripRegistrations.Where(t => t.TripId == model.TripId).Select(t => t.UserId).ToListAsync();

            var trip = await _context.Trips.Include(t => t.Creator).FirstOrDefaultAsync(t => t.Id == model.TripId);

            if (!tripRegistrations.Contains(userId) || trip?.Creator.Id == userId)
            {
                ViewBag.Error = "Ви не можете залишати відгук на цю поїздку";
                return View("Report", model);
            }            

            var driver = await _context.Users.FirstOrDefaultAsync(u => u.Id == trip.Creator.Id);

            var reportsOfThisDriver = await _context.Reports.Where(r => r.ReportedId == driver.Id).ToListAsync();

            var reportsOfThisDriverOnThisTripByThisUser = from r in reportsOfThisDriver where r.TripId == model.TripId && r.CreatorId == userId select r;
                                         
            if (reportsOfThisDriverOnThisTripByThisUser.Any()) 
            {
                ViewBag.Error = "Ви вже залишали відгук на цього водія за цю поїздку";
                return View("Report", model);
            }

            var report = new Report
            {
                Text = model.Text,
                TripId = model.TripId,
                CreatorId = userId,
                Rating = int.Parse(model.Rating),
                ReportedId = driver.Id
            };

            if (driver.Rating == null) 
                driver.Rating = report.Rating;  
            else
                driver.Rating = (driver.Rating * reportsOfThisDriver.Count + report.Rating) / (reportsOfThisDriver.Count + 1);

            driver.AmountOfReports += 1;

            if (report.Rating == 1)
            {                
                driver.AmountOfBadReports += 1;

                if (driver.AmountOfBadReports > 2)
                    driver.isBanned = true;                
            }

            _context.Users.Update(driver);
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();            

            return RedirectToAction("MyTrips");
        }       

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        private static string GetErrorMessage(bool isValidCity, bool arePointsDifferent)
        {
            if (!isValidCity)
            {
                return "Обох або одного з місць немає у базі";
            }
            else if (!arePointsDifferent)
            {
                return "Місця відправлення та прибуття повинні відрізнятися";
            }
            else
            {
                return "Час відправлення повинен бути не раніше чим через годину він цього моменту.";
            }
        }        

        private (bool isValid, string errorMessage) AreCitiesValidInModel(TripViewModel model) 
        {
            var cities = _context.Cities.Select(c => c.Name).ToList();

            var isValidCity = cities.Contains(model.DeparturePoint) && cities.Contains(model.ArrivalPoint);
            var isValidTime = model.DepartureTime.Date >= DateTime.Now.Date;
            var arePointsDifferent = model.ArrivalPoint != model.DeparturePoint;            

            return new (isValidCity && isValidTime && arePointsDifferent, GetErrorMessage(isValidCity, arePointsDifferent) );            
        }
    }    
}