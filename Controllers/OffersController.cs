using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PROJEKT_PZ_NK_v3.DAL;
using PROJEKT_PZ_NK_v3.Models;
using PagedList;
using System.Data.Entity.SqlServer;

namespace PROJEKT_PZ_NK_v3.Controllers
{
    public class OffersController : Controller
    {
        private OfferContext db = new OfferContext();

        // GET: Offers
        public ActionResult Index(string sortOrder, string searchString, string searchSpecies, string searchRace, 
            string searchOwner, string searchAnimal, int? searchTime, int? page)
        {

            ViewBag.CurrentSort = sortOrder;
            ViewBag.TitleSortParmAsc = String.IsNullOrEmpty(sortOrder) ? "Title_asc" : "";
            ViewBag.TitleSortParmDesc = String.IsNullOrEmpty(sortOrder) ? "Title_desc" : "";
            ViewBag.DateSortParmAsc = String.IsNullOrEmpty(sortOrder) ? "StartingDateAsc" : "";
            ViewBag.DateSortParmDesc = String.IsNullOrEmpty(sortOrder) ? "StartingDateDesc" : "";

            if (searchString != null)
            {
                page = 1;
            }
            ViewBag.CurrentFilterT = searchString;
            ViewBag.CurrentFilterS = searchSpecies;
            ViewBag.CurrentFilterR = searchRace;
            ViewBag.CurrentFilterO = searchOwner;
            ViewBag.CurrentFilterA = searchAnimal;
            ViewBag.CurrentFilterT = searchTime;

            var offers = from s in db.Offers
                         where s.StartingDate > DateTime.Now
                         select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                offers = offers.Where(s => s.Title.ToLower().Contains(searchString.ToLower()));
            }
            if (!String.IsNullOrEmpty(searchSpecies))
            {
                offers = offers.Where(s => s.Animal.Species.ToLower().Contains(searchSpecies.ToLower()));
            }
            if (!String.IsNullOrEmpty(searchRace))
            {
                offers = offers.Where(s => s.Animal.Race.ToLower().Contains(searchRace.ToLower()));
            }
            if (!String.IsNullOrEmpty(searchOwner))
            {
                offers = offers.Where(s => s.Profile.FirstName.ToLower().Contains(searchOwner.ToLower()) 
                    || s.Profile.Login.ToLower().Contains(searchOwner.ToLower()));
            }
            if (!String.IsNullOrEmpty(searchAnimal))
            {
                offers = offers.Where(s => s.Animal.Name.ToLower().Contains(searchAnimal.ToLower()));
            }
            if (searchTime != null)
            {
                offers = offers.Where(s => SqlFunctions.DateDiff("DD", s.StartingDate, s.EndDate) == searchTime);
            }
            switch (sortOrder)
            {
                case "StartingDateAsc":
                    offers = offers.OrderBy(s => s.StartingDate.Year).ThenBy(s => s.StartingDate.Month).ThenBy(s => s.StartingDate.Day);
                    break;
                case "StartingDateDesc":
                    offers = offers.OrderByDescending(s => s.StartingDate.Year).ThenByDescending(s => s.StartingDate.Month).ThenByDescending(s => s.StartingDate.Day);
                    break;
                case "Title_desc":
                    offers = offers.OrderByDescending(s => s.Title);
                    break;
                case "Title_asc":
                    offers = offers.OrderBy(s => s.Title);
                    break;
                default:
                    offers = offers.OrderBy(s => s.ID);
                    break;
            }
            int pageSize = 12;
            int pageNumber = (page ?? 1);
            ViewBag.SortList = new List<string>() {"tytuł malejąco", "tytuł rosnąco", "data malejąco", "data rosnąco"};
            return View(offers.ToPagedList(pageNumber, pageSize));
        }

        [Authorize]
        public ActionResult MyOffers()
        {
            var offers = from s in db.Offers
                         where s.StartingDate > DateTime.Now && s.Profile.Email == User.Identity.Name
                         select s;
            return View(offers.ToList());
        }

        [Authorize]
        // GET: Offers/Details/5
        public ActionResult Details(int? id)
        {
            Session["Applications"] = db.Applications.ToList();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Offer offer = db.Offers.Find(id);
            if (offer == null)
            {
                return HttpNotFound();
            }
            ViewBag.IsApplied = db.Applications.Any(app => app.Guardian.Email == User.Identity.Name && app.OfferID == id);
            var applications = db.Applications
                .Include(a => a.Guardian)
                .Include(a => a.Offer)
                .Include(a => a.Owner)
                .Where(a => (a.Owner.Email == User.Identity.Name || a.Guardian.Email == User.Identity.Name)
                && a.Offer.StartingDate > DateTime.Now
                && a.StatusOwner != "Odrzucone"
                && a.StatusGuardian != "Odrzucone"
                && a.StatusGuardian != "Usuniete"
                && a.StatusOwner != "Usuniete");
            ViewBag.Applications = applications;
            ViewBag.ApplicationsCount = applications.Count();

            var offers = db.Offers
                .Where(a => a.ID != id && a.Profile.Email != User.Identity.Name)
                .OrderBy(a => a.Profile.ID == id)
                .ThenBy(a => a.Animal.Species == offer.Animal.Species)
                .ThenBy(a => a.Animal.Race == offer.Animal.Race)
                .Take(5)
                .ToList();
            ViewBag.AnotherOffers = offers;
            ViewBag.AnotherOffersCount = offers.Count();

            return View(offer);
        }

        [Authorize]
        // GET: Offers/Create
        public ActionResult Create()
        {
            Profile myProfile = db.Profiles.Single(p => p.Email == User.Identity.Name);
            ViewBag.Animals = myProfile.Animals.ToList();
            ViewBag.AnimalsCount = myProfile.Animals.Count();
            return View();
        }

        // POST: Offers/Create
        // Aby zapewnić ochronę przed atakami polegającymi na przesyłaniu dodatkowych danych, włącz określone właściwości, z którymi chcesz utworzyć powiązania.
        // Aby uzyskać więcej szczegółów, zobacz https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Offer offer)
        {
            if (ModelState.IsValid)
            {
                Profile myProfile = db.Profiles.Single(p => p.Email == User.Identity.Name);
                offer.Profile = myProfile;
                db.Offers.Add(offer);
                db.SaveChanges();
                db.Profiles.Single(p => p.Email == User.Identity.Name).Offers.Add(offer);

                foreach (var item in db.SavedProfiles.Where(a => a.SavedProfile.ID == offer.Profile.ID && a.SavedAs == Saved.favourite))
                {
                    Notification notifi = new Notification
                    {
                        Offer = offer,
                        Message = offer.Profile.Login + " dodał nową ofertę",
                        Profile = item.MyProfile
                    };
                    db.Notifications.Add(notifi);
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(offer);
        }


        [Authorize]
        // GET: Offers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Offer offer = db.Offers.Find(id);
            if (offer == null)
            {
                return HttpNotFound();
            }
            return View(offer);
        }

        // POST: Offers/Edit/5
        // Aby zapewnić ochronę przed atakami polegającymi na przesyłaniu dodatkowych danych, włącz określone właściwości, z którymi chcesz utworzyć powiązania.
        // Aby uzyskać więcej szczegółów, zobacz https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Offer offer)
        {
            if (ModelState.IsValid)
            {
                db.Entry(offer).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(offer);
        }

        [Authorize]
        // GET: Offers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Offer offer = db.Offers.Find(id);
            if (offer == null)
            {
                return HttpNotFound();
            }
            return View(offer);
        }

        // POST: Offers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Offer offer = db.Offers.Find(id);
            var applications = db.Applications.Where(app => app.OfferID == id).ToList();
            foreach (var item in applications)
            {
                db.Applications.Remove(item);
            }
            db.Offers.Remove(offer);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
