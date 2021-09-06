using PROJEKT_PZ_NK_v3.DAL;
using PROJEKT_PZ_NK_v3.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using PROJEKT_PZ_NK_v3.ViewModels;
using Xamarin.Forms.Maps;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.Entity;

namespace PROJEKT_PZ_NK_v3.Controllers
{
    public class HomeController : Controller
    {
        OfferContext db = new OfferContext();
        public ActionResult Index()
        {
            Profile profile = db.Profiles.FirstOrDefault(p => p.Email == User.Identity.Name);
            ViewBag.Profil = profile;

            var offers = db.Offers
                .Where(a => a.Profile.Email != User.Identity.Name && a.Profile.Comments
                    .Where(b => ((b.Author.Email == User.Identity.Name || b.Profile.Email == User.Identity.Name)
                        && (b.Grade > 2 || b.Grade == 0)))
                .Count() >= 0);

            if (db.Applications.Any(a => a.Guardian.Email == User.Identity.Name))
            {
                int hours = db.Applications.Where(a => a.Guardian.Email == User.Identity.Name && DbFunctions.DiffDays(a.Offer.EndDate, a.Offer.StartingDate) == 0).Count();
                int days = db.Applications.Where(a => a.Guardian.Email == User.Identity.Name && DbFunctions.DiffDays(a.Offer.EndDate, a.Offer.StartingDate) > 0).Count();
                if (hours > days)
                {
                    ViewBag.Offers1 = offers
                        .OrderByDescending(a => DbFunctions.DiffDays(a.EndDate, a.StartingDate))
                        .ThenByDescending(a => a.Profile.Rate)
                    .Take(4).ToList();
                    ViewBag.Offers2 = offers
                        .OrderByDescending(a => DbFunctions.DiffDays(a.EndDate, a.StartingDate))
                        .ThenByDescending(a => a.Profile.Rate)
                    .Skip(4).Take(4).ToList();
                }
                else
                {
                    ViewBag.Offers1 = offers
                        .OrderBy(a => DbFunctions.DiffDays(a.EndDate, a.StartingDate))
                        .ThenByDescending(a => a.Profile.Rate)
                    .Take(4).ToList();
                    ViewBag.Offers2 = offers
                        .OrderBy(a => DbFunctions.DiffDays(a.EndDate, a.StartingDate))
                        .ThenByDescending(a => a.Profile.Rate)
                    .Skip(4).Take(4).ToList();
                }
            }
            else
            {
                ViewBag.Offers1 = offers
                    .OrderByDescending(a => a.Profile.Rate)
                    .Take(4).ToList();
                ViewBag.Offers2 = offers
                    .OrderByDescending(a => a.Profile.Rate)
                    .Skip(4).Take(4).ToList();
            }
            return View();
        }

        private async Task<int> Disctance(Profile profile)
        {
            Geocoder geoCoder = new Geocoder();

            Profile myProfile = db.Profiles.Single(p => p.Email == User.Identity.Name);
            String myAddress = myProfile.City + ", " + myProfile.Street + " " + myProfile.HouseNumber;
            IEnumerable<Position> myApproximateLocations = await geoCoder.GetPositionsForAddressAsync(myAddress);
            Position myPosition = myApproximateLocations.FirstOrDefault();

            String secondAddress = profile.City + ", " + profile.Street + " " + profile.HouseNumber;
            IEnumerable<Position> secondApproximateLocations = await geoCoder.GetPositionsForAddressAsync(secondAddress);
            Position secondPosition = secondApproximateLocations.FirstOrDefault();

            Distance distance = Distance.BetweenPositions(myPosition, secondPosition);
            return ((int)distance.Meters);
        }

        public ActionResult About()
        {
            IQueryable<EnrollmentDateGroup> data = from animal in db.Animals
                                                   group animal by animal.Species into Group
                                                   select new EnrollmentDateGroup()
                                                   {
                                                       Species = Group.Key,
                                                       SpeciesCount = Group.Count()
                                                   };
            return View(data.ToList());
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}