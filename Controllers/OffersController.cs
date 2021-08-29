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

namespace PROJEKT_PZ_NK_v3.Controllers
{
    [Authorize]
    public class OffersController : Controller
    {
        private OfferContext db = new OfferContext();

        // GET: Offers
        [Authorize]
        public ActionResult Index(string sortOrder, string searchString, string currentFilter, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.TitleSortParm = String.IsNullOrEmpty(sortOrder) ? "Title_desc" : "";
            ViewBag.DateSortParm = String.IsNullOrEmpty(sortOrder) ? "StartingDate" : "";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var offers = from s in db.Offers
                         where s.StartingDate >= DateTime.Now
                         select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                offers = offers.Where(s => s.Title.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "StartingDate":
                    offers = offers.OrderBy(s => s.StartingDate);
                    break;
                case "Title_desc":
                    offers = offers.OrderByDescending(s => s.Title);
                    break;
                default:
                    offers = offers.OrderBy(s => s.ID);
                    break;
            }
            int pageSize = 9;
            int pageNumber = (page ?? 1);
            return View(offers.ToPagedList(pageNumber, pageSize));
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
            List<Applications> AppList = db.Applications
                .Include(a => a.Offer)
                .Include(a => a.Guardian)
                .Where(app => app.Owner.Email == User.Identity.Name && app.OfferID == id)
                .ToList();
            ViewBag.Applications = AppList;
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
        public ActionResult Create(Offer offer)
        {
            if (ModelState.IsValid)
            {
                Profile myProfile = db.Profiles.Single(p => p.Email == User.Identity.Name);
                offer.Profile = myProfile;

                db.Offers.Add(offer);
                db.SaveChanges();
                return RedirectToAction("Index");
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
