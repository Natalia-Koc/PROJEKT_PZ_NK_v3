using PROJEKT_PZ_NK_v3.DAL;
using PROJEKT_PZ_NK_v3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PROJEKT_PZ_NK_v3.DAL;
using PROJEKT_PZ_NK_v3.ViewModels;

namespace PROJEKT_PZ_NK_v3.Controllers
{
    public class HomeController : Controller
    {
        OfferContext db = new OfferContext();
        public ActionResult Index()
        {
            Profile profile = db.Profiles.FirstOrDefault(p => p.Email == User.Identity.Name);

            ViewBag.Profil = profile;

            return View();
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