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

namespace PROJEKT_PZ_NK_v3.Controllers
{
    [Authorize]
    public class ApplicationsController : Controller
    {
        private OfferContext db = new OfferContext();

        // GET: Applications
        public ActionResult Index(int? id)
        {
            var applications = db.Applications
                .Include(a => a.Guardian)
                .Include(a => a.Offer)
                .Include(a => a.Owner);
            return View(applications.ToList());
        }

        public ActionResult MyApplications()
        {
            var applications = db.Applications
                .Include(a => a.Guardian)
                .Include(a => a.Offer)
                .Include(a => a.Owner)
                .Where(a => a.Guardian.Email == User.Identity.Name 
                && a.Offer.StartingDate > DateTime.Now
                && a.StatusGuardian != "Odrzucone"
                && a.StatusGuardian != "Usuniete");
            return View(applications.ToList());
        }

        public ActionResult ApplicationsToMyOffers()
        {
            var applications = db.Applications
                .Include(a => a.Guardian)
                .Include(a => a.Offer)
                .Include(a => a.Owner)
                .Where(a => a.Owner.Email == User.Identity.Name 
                && a.Offer.StartingDate > DateTime.Now
                && a.StatusOwner != "Odrzucone"
                && a.StatusOwner != "Usuniete");
            return View(applications.ToList());
        }

        public ActionResult History()
        {
            var applications = db.Applications
                .Include(a => a.Guardian)
                .Include(a => a.Offer)
                .Include(a => a.Owner)
                .Where(a => (a.Offer.StartingDate < DateTime.Now || 
                    (a.StatusOwner == "Odrzucone" ||
                    a.StatusGuardian == "Odrzucone") &&
                    ((a.StatusOwner != "Usuniete" && a.Owner.Email == User.Identity.Name) ||
                    (a.StatusGuardian != "Usuniete" && a.Guardian.Email == User.Identity.Name))));
            return View(applications.ToList());
        }

        // GET: Applications/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Applications applications = db.Applications.Find(id);
            if (applications == null)
            {
                return HttpNotFound();
            }
            return View(applications);
        }

        // GET: Applications/Create
        public ActionResult Create(int offerID, int ownerID, Applications application)
        {
            application.Guardian = db.Profiles.Single(p => p.Email == User.Identity.Name);
            application.GuardianID = db.Profiles.Single(p => p.Email == User.Identity.Name).ID;
            application.Owner = db.Profiles.Find(ownerID);
            application.OwnerID = ownerID;
            application.Offer = db.Offers.Find(offerID);
            application.OfferID = offerID;
            application.StatusGuardian = "Oczekuje na akceptacje";
            application.StatusOwner = "Oczekuje na akceptacje";
            db.Applications.Add(application);
            db.SaveChanges();

            return RedirectToAction("Details", "Offers", new { id = offerID });
        }


        // GET: Applications/Edit/5
        public ActionResult EditResign(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            db.Applications.Find(id).StatusGuardian = "Odrzucone";
            db.SaveChanges();
            return RedirectToAction("MyApplications");
        }

        public ActionResult EditDiscard(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            db.Applications.Find(id).StatusOwner = "Odrzucone";
            db.SaveChanges();
            return RedirectToAction("ApplicationsToMyOffers");
        }

        public ActionResult EditAccept(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            db.Applications.Find(id).StatusOwner = "Zaakceptowane!";
            db.Applications.Find(id).StatusGuardian = "Zaakceptowane!";
            db.SaveChanges();
            return RedirectToAction("ApplicationsToMyOffers");
        }

        // POST: Applications/Edit/5
        // Aby zapewnić ochronę przed atakami polegającymi na przesyłaniu dodatkowych danych, włącz określone właściwości, z którymi chcesz utworzyć powiązania.
        // Aby uzyskać więcej szczegółów, zobacz https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Applications applications)
        {
            if (ModelState.IsValid)
            {
                db.Entry(applications).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.GuardianID = new SelectList(db.Profiles, "ID", "Login", applications.GuardianID);
            ViewBag.OfferID = new SelectList(db.Offers, "ID", "Title", applications.OfferID);
            ViewBag.OwnerID = new SelectList(db.Profiles, "ID", "Login", applications.OwnerID);
            return View(applications);
        }

        // GET: Applications/Delete/5
        public ActionResult Delete(int? id)
        {

            Applications applications = db.Applications.Find(id);
            if (db.Profiles.Find(applications.OwnerID).Email == User.Identity.Name)
            {
                db.Applications.Find(id).StatusOwner = "Usuniete";
            }
            else
            {
                db.Applications.Find(id).StatusGuardian = "Usuniete";
            }
            db.SaveChanges();
            return RedirectToAction("History");
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
