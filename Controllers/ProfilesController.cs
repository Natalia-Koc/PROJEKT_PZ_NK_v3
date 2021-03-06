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
    public class ProfilesController : Controller
    {
        private OfferContext db = new OfferContext();

        [Authorize]
        public ActionResult Ranking()
        {
            var profiles = db.Profiles.OrderByDescending(p => p.Rate).Take(20).ToList();
            return View(profiles);
        }
        public ActionResult Details()
        {
            Profile profile = db.Profiles.FirstOrDefault(p => p.Email == User.Identity.Name);
            Comments comments = new Comments();
            ViewBag.comments = comments;
            ViewBag.ProgressBarCount = db.Comments.Where(m => m.Profile.Email == User.Identity.Name && m.Grade != 0).Count();
            ViewBag.FoundComment = db.Comments.Any(m => m.Author.Email == User.Identity.Name);
            
            

            return View(profile);
        }

        public ActionResult DetailsAnotherProfile(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Profile profile = db.Profiles.Find(id); 
            ViewBag.ProgressBarCount = db.Comments.Where(m => m.ProfileID == id && m.Grade != 0).Count();
            ViewBag.FoundComment = db.Comments.Any(m => m.Author.Email == User.Identity.Name && m.ProfileID == id);
            ViewBag.Favourite = db.SavedProfiles
                .Where(m => m.SavedProfile.ID == id && m.MyProfile.Email == User.Identity.Name && m.SavedAs == Saved.favourite)
                .Count();
            ViewBag.Blocked = db.SavedProfiles
                .Where(m => m.SavedProfile.ID == id && m.MyProfile.Email == User.Identity.Name && m.SavedAs == Saved.blocked)
                .Count();
            if (!ViewBag.FoundComment)
            {
                ViewBag.MyComment = null;
            }
            else
            {
                ViewBag.MyComment = db.Comments.First(c => c.Author.Email == User.Identity.Name && c.ProfileID == id);
            }


            var zmienna = db.Applications
                .Include("Offer")
                .Include("Owner")
                .Include("Guardian")
                .Any(a => a.StatusOwner == "Zaakceptowane!"
                    && (a.Guardian.Email == User.Identity.Name
                    || a.Owner.Email == User.Identity.Name)
                    && a.Offer.StartingDate < DateTime.Now);


            var costam = profile.GuardianApplications.Any(a => a.StatusOwner == "Zaakceptowane!"
                    && a.Owner.Email == User.Identity.Name
                    && a.Offer.StartingDate < DateTime.Now) 
                || profile.OwnerApplications.Any(a => a.StatusOwner == "Zaakceptowane!"
                    && a.Guardian.Email == User.Identity.Name
                    && a.Offer.StartingDate < DateTime.Now);




            ViewBag.Notified = costam;






            ViewBag.NotifiEnded = db.Notifications.Where(a => a.Message.Contains("Oceń") && a.Profile.Email == User.Identity.Name).Count();
            

            if (profile == null)
            {
                return HttpNotFound();
            }
            return View("Details", profile);
        }

        // GET: Profiles/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Profile profile = db.Profiles.Find(id);
            if (profile == null)
            {
                return HttpNotFound();
            }
            return View(profile);
        }

        // POST: Profiles/Edit/5
        // Aby zapewnić ochronę przed atakami polegającymi na przesyłaniu dodatkowych danych, włącz określone właściwości, z którymi chcesz utworzyć powiązania.
        // Aby uzyskać więcej szczegółów, zobacz https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Profile profile)
        {
            if (ModelState.IsValid)
            {
                profile.Rate = 0;
                db.Entry(profile).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details");
            }
            return View(profile);
        }

        // GET: Profiles/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Profile profile = db.Profiles.Find(id);
            if (profile == null)
            {
                return HttpNotFound();
            }
            return View(profile);
        }

        // POST: Profiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Profile profile = db.Profiles.Find(id);
            db.Profiles.Remove(profile);
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
