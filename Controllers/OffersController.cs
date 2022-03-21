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
using System.IO;
using System.Text;

namespace PROJEKT_PZ_NK_v3.Controllers
{
    public class OffersController : Controller
    {
        private OfferContext db = new OfferContext();
        float CalculateDistance(string source, string destination)
        {
            string distance;
            string url = "https://maps.googleapis.com/maps/api/distancematrix/xml?origins=" + source + "&destinations=" + destination + "&key=AIzaSyA1BOzf3325XV08x9aMj_kELckTcgL3xrQ";
            WebRequest request = WebRequest.Create(url);
            using (WebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    DataSet dsResult = new DataSet();
                    dsResult.ReadXml(reader);
                    distance = dsResult.Tables["distance"].Rows[0]["value"].ToString();
                }
            }
            return float.Parse(distance);
        }

        // GET: Offers
        public ActionResult Index(string sortOrder, string searchString, string searchSpecies, string searchRace, 
            string searchOwner, string searchAnimal, int? searchTime, int? page)
        {

            ViewBag.CurrentSort = sortOrder;
            ViewBag.TitleSortParmAsc = String.IsNullOrEmpty(sortOrder) ? "Title_asc" : "";
            ViewBag.TitleSortParmDesc = String.IsNullOrEmpty(sortOrder) ? "Title_desc" : "";
            ViewBag.DateSortParmAsc = String.IsNullOrEmpty(sortOrder) ? "StartingDateAsc" : "";
            ViewBag.DateSortParmDesc = String.IsNullOrEmpty(sortOrder) ? "StartingDateDesc" : "";
            ViewBag.DistanceSortParmAsc = String.IsNullOrEmpty(sortOrder) ? "DistanceAsc" : "";
            ViewBag.DistanceSortParmDesc = String.IsNullOrEmpty(sortOrder) ? "DistanceDesc" : "";
            ViewBag.TimeSortParmAsc = String.IsNullOrEmpty(sortOrder) ? "TimeAsc" : "";
            ViewBag.TimeSortParmDesc = String.IsNullOrEmpty(sortOrder) ? "TimeDesc" : "";

            if (searchString != null || searchSpecies != null || searchRace != null || searchOwner != null || searchAnimal != null || searchTime != null)
            {
                page = 1;
            }
            ViewBag.CurrentFilterSt = searchString;
            ViewBag.CurrentFilterS = searchSpecies;
            ViewBag.CurrentFilterR = searchRace;
            ViewBag.CurrentFilterO = searchOwner;
            ViewBag.CurrentFilterA = searchAnimal;
            ViewBag.CurrentFilterT = searchTime;

            var offers = db.Offers
                         .Include(a => a.Profile)
                         .Where(w => w.StartingDate > DateTime.Now)
                         .ToList();

            if (!String.IsNullOrEmpty(searchString))
            {
                offers = offers.Where(s => s.Title.ToLower().Contains(searchString.ToLower())).ToList();
            }
            if (!String.IsNullOrEmpty(searchSpecies))
            {
                offers = offers.Where(s => s.Animal.Species.ToLower().Contains(searchSpecies.ToLower())).ToList();
            }
            if (!String.IsNullOrEmpty(searchRace))
            {
                offers = offers.Where(s => s.Animal.Race.ToLower().Contains(searchRace.ToLower())).ToList();
            }
            if (!String.IsNullOrEmpty(searchOwner))
            {
                offers = offers.Where(s => s.Profile.FirstName.ToLower().Contains(searchOwner.ToLower()) 
                    || s.Profile.Login.ToLower().Contains(searchOwner.ToLower())).ToList();
            }
            if (!String.IsNullOrEmpty(searchAnimal))
            {
                offers = offers.Where(s => s.Animal.Name.ToLower().Contains(searchAnimal.ToLower())).ToList();
            }
            if (searchTime != null)
            {
                var DDoffers = offers.Where(s => (s.EndDate.DayOfYear - s.StartingDate.DayOfYear) == searchTime);
                offers = DDoffers.ToList();
            }

            Profile profile = db.Profiles.FirstOrDefault(p => p.Email == User.Identity.Name);
            ViewBag.Profil = profile;

            switch (sortOrder)
            {
                case "StartingDateAsc":
                    offers = offers.OrderBy(s => s.StartingDate.Year).ThenBy(s => s.StartingDate.Month).ThenBy(s => s.StartingDate.Day).ToList();
                    break;
                case "StartingDateDesc":
                    offers = offers.OrderByDescending(s => s.StartingDate.Year).ThenByDescending(s => s.StartingDate.Month).ThenByDescending(s => s.StartingDate.Day).ToList();
                    break;
                case "Title_desc":
                    offers = offers.OrderByDescending(s => s.Title).ToList();
                    break;
                case "Title_asc":
                    offers = offers.OrderBy(s => s.Title).ToList();
                    break;
                case "DistanceDesc":
                    offers = offers.OrderByDescending(a => CalculateDistance(a.Profile.City + " " + a.Profile.Street, profile.City + " " + profile.Street)).ToList();
                    break;
                case "DistanceAsc":
                    offers = offers.OrderBy(a => CalculateDistance(a.Profile.City + " " + a.Profile.Street, profile.City + " " + profile.Street)).ToList();
                    break;
                case "TimeDesc":
                    offers = offers.OrderByDescending(s => (s.EndDate.DayOfYear - s.StartingDate.DayOfYear)).ToList();
                    break;
                case "TimeAsc":
                    offers = offers.OrderBy(s => (s.EndDate.DayOfYear - s.StartingDate.DayOfYear)).ToList();
                    break;
                default:
                    offers = offers.OrderBy(s => s.ID).ToList();
                    break;
            }
            int pageSize = 12;
            int pageNumber = (page ?? 1);
            ViewBag.SortList = new List<string>() {
                "tytuł malejąco", 
                "tytuł rosnąco", 
                "data malejąco", 
                "data rosnąco", 
                "odległość malejąco", 
                "odległość rosnąco", 
                "czas trwania malejąco", 
                "czas trwania rosnąco" 
            };
            return View(offers.ToList().ToPagedList(pageNumber, pageSize));
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
                && a.OfferID == id
                && a.Offer.StartingDate > DateTime.Now
                && a.StatusOwner != "Odrzucone"
                && a.StatusGuardian != "Odrzucone"
                && a.StatusGuardian != "Usuniete"
                && a.StatusOwner != "Usuniete");
            ViewBag.Applications = applications;
            ViewBag.ApplicationsCount = applications.Count();

            var offers = db.Offers
                .Where(a => a.ID != id && a.Profile.Email != User.Identity.Name && a.StartingDate >= DateTime.Now)
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

                foreach (var item in db.SavedProfiles.Include("MyProfile").Where(a => a.SavedProfile.Email == myProfile.Email && a.SavedAs == Saved.favourite))
                {
                    Notification notifi = new Notification
                    {
                        Offer = offer,
                        Message = myProfile.Login + " dodał(a) nową ofertę",
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
        // GET: Offers/Edit/5
        public ActionResult Copy(int? id)
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
        public ActionResult Copy(Offer offer)
        {
            if (ModelState.IsValid)
            {
                offer.Profile = db.Profiles.Single(a => a.Email == User.Identity.Name);
                db.Offers.Add(offer);
                db.SaveChanges();
                return RedirectToAction("Details", new { id = offer.ID });
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
