using PROJEKT_PZ_NK_v3.DAL;
using PROJEKT_PZ_NK_v3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PROJEKT_PZ_NK_v3.Controllers
{
    public class MyLayoutController : Controller, IController
    {

        OfferContext db = new OfferContext();

        // GET: Layout
        public ActionResult Index()
        {
            var profile = db.Profiles.Single(p => p.Email == User.Identity.Name);
            ViewBag.notifi = profile.Notifications.Where(n => n.Removed == false).ToList();
            ViewBag.notifiCount = profile.Notifications.Where(n => n.Removed == false).Count();



            var applicationsEnded = db.Applications
                .Where(a => (a.StatusOwner == "Zaakceptowane!" || a.StatusGuardian == "Zaakceptowane!")
                    && a.Offer.EndDate < DateTime.Now
                    && !db.Notifications.Any(n => n.OfferID == a.OfferID && n.Message.Contains("Oceń"))).FirstOrDefault();

            if (applicationsEnded != null && db.Notifications.Find(applicationsEnded.OfferID) != null)
            {
                db.Notifications.Add(new Notification
                {
                    Offer = applicationsEnded.Offer,
                    Profile = applicationsEnded.Guardian,
                    WhoIRateID = applicationsEnded.OwnerID,
                    Message = "Oferta zakończona. Oceń właściciela!"
                });
                db.Notifications.Add(new Notification
                {
                    Offer = applicationsEnded.Offer,
                    Profile = applicationsEnded.Owner,
                    WhoIRateID = applicationsEnded.GuardianID,
                    Message = "Oferta zakończona. Oceń opiekuna!"
                });
                db.SaveChanges();
            }
            

            return PartialView("Index");
        }

        public ActionResult DeleteAndViewProfile(int idNotifications, int idProfile)
        {
            var notifi = db.Notifications.Find(idNotifications);
            notifi.Removed = true;
            db.SaveChanges();

            return RedirectToAction("DetailsAnotherProfile", "Profiles", new { id = idProfile });
        }

        public ActionResult DeleteAndViewOffer(int idNotifications, int idOffer)
        {
            var notifi = db.Notifications.Find(idNotifications);
            notifi.Removed = true;
            db.SaveChanges();

            return RedirectToAction("Details", "Offers", new { id = idOffer });

        }

        public ActionResult DeleteAllNotifications()
        {
            var notifi = db.Notifications.Where(a => a.Profile.Email == User.Identity.Name);
            foreach (var notification in notifi)
            {
                notification.Removed = true;
            }
            db.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

    }
}