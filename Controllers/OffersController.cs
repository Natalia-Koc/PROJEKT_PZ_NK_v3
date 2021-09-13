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
using Neo4j.Driver;
using System.Threading.Tasks;

namespace PROJEKT_PZ_NK_v3.Controllers
{
    [Authorize]
    public class OffersController : Controller
    {
        private OfferContext db = new OfferContext();

        // GET: Offers
        [Authorize]
        public ActionResult Index(string sortOrder, string searchString, string searchSpecies, string searchRace, int? page)
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
            ViewBag.SortList = new List<string>() { "tytuł malejąco", "tytuł rosnąco", "data malejąco", "data rosnąco" };
            return View(offers.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult MyOffers()
        {
            var offers = from s in db.Offers
                         where s.StartingDate > DateTime.Now && s.Profile.Email == User.Identity.Name
                         select s;
            return View(offers.ToList());
        }

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
                .Where(a => a.Owner.Email == User.Identity.Name
                && a.Offer.StartingDate > DateTime.Now
                && a.Status != "Właściciel odrzucił zgłoszenie"
                && a.Status != "Odrzucone"
                && !a.Status.Contains("Usunieta"));
            ViewBag.Applications = applications;
            return View(offer);
        }

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
        public async Task<ActionResult> Create(Offer offer)
        {
            if (ModelState.IsValid)
            {
                Profile myProfile = db.Profiles.Single(p => p.Email == User.Identity.Name);
                offer.AnimalID = db.Animals.Where(a => a.Name == offer.AnimalName).First().ID;
                offer.Profile = myProfile;
                db.Offers.Add(offer);
                db.SaveChanges();
                db.Profiles.Single(p => p.Email == User.Identity.Name).Offers.Add(offer);
                db.SaveChanges();
                IAsyncSession session = db._driver.AsyncSession();
                try
                {
                    IResultCursor cursor = await session.RunAsync(
                        "MATCH (n:Profile { Email:'" + User.Identity.Name + "'})-[rel:OWNER]->(a:Animal {Name: '" + offer.AnimalName + "'}) " +
                        "CREATE(n) -[r:AUTHOR]-> (p:Offer " +
                        "{ OfferID: " + offer.ID +
                        ", Title: '" + offer.Title +
                        "', Description: '" + offer.Description +
                        "', StartingDate: '" + offer.StartingDate.Date +
                        "', EndDate: '" + offer.EndDate.Date + "'})," +
                        "(a) -[t:ANIMAL_OFFER]-> (p)"
                    );
                    await cursor.ConsumeAsync();
                    /*IResultCursor cursor = await session.RunAsync();
                    await cursor.ConsumeAsync();*/
                }
                finally
                {
                    await session.CloseAsync();
                }
                return RedirectToAction("MyOffers");
            }

            return View(offer);
        }

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
        public async Task<ActionResult> Edit(Offer offer)
        {
            if (ModelState.IsValid)
            {
                db.Entry(offer).State = EntityState.Modified;
                db.SaveChanges();

                IAsyncSession session = db._driver.AsyncSession();
                try
                {
                    IResultCursor cursor = await session.RunAsync(
                        "MATCH (o:Offer { OfferID:" + offer.ID + "}) " +
                        "SET o.Title = '" + offer.Title +
                        "', o.Description = '" + offer.Description +
                        "', o.StartingDate = '" + offer.StartingDate.Date +
                        "', o.EndDate = '" + offer.EndDate.Date + "'"
                    );
                    await cursor.ConsumeAsync();
                }
                finally
                {
                    await session.CloseAsync();
                }
                return RedirectToAction("MyOffers");
            }
            return View(offer);
        }

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
