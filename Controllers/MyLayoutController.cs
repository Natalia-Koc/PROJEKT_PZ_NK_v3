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
            var notifi = db.Notifications
                .Include("Offer")
                .Where(a => a.Profile.Email == User.Identity.Name);
            ViewBag.notifi = notifi;
            ViewBag.notifiCount = notifi.Count();

            if (db.Applications != null )
            {
                var applications = db.Applications
                    .Include("Offer");
                var applicationsEnded = applications
                    .Where(a => (a.StatusOwner == "Zaakceptowane!" || a.StatusGuardian == "Zaakceptowane!")
                        && a.Offer.EndDate >= DateTime.Now).FirstOrDefault();

                if (applicationsEnded != null && db.Notifications.Find(applicationsEnded.OfferID) != null)
                {
                    db.Notifications.Add(new Notification
                    {
                        Offer = applicationsEnded.Offer,
                        Profile = applicationsEnded.Guardian,
                        Message = "Oferta zakończona. Oceń właściciela!"
                    });
                    db.Notifications.Add(new Notification
                    {
                        Offer = applicationsEnded.Offer,
                        Profile = applicationsEnded.Owner,
                        Message = "Oferta zakończona. Oceń opiekuna!"
                    });
                    db.SaveChanges();
                }
            }

            return PartialView("Index");
        }

        public ActionResult DeleteAndViewProfile(int idNotifications, int idProfile)
        {
            var notifi = db.Notifications.Find(idNotifications);
            db.Notifications.Remove(notifi);
            db.SaveChanges();

            return RedirectToAction("DetailsAnotherProfile", "Profiles", new { id = idProfile });
        }

        public ActionResult DeleteAndViewOffer(int idNotifications, int idOffer)
        {
            var notifi = db.Notifications.Find(idNotifications);
            db.Notifications.Remove(notifi);
            db.SaveChanges();

            return RedirectToAction("Details", "Offers", new { id = idOffer });

        }

        public ActionResult DeleteAllNotifications()
        {
            var notifi = db.Notifications.Where(a => a.Profile.Email == User.Identity.Name);
            foreach (var notification in notifi)
            {
                db.Notifications.Remove(notification);
            }
            db.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

    }
}