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
            ViewBag.notifi = profile.Notifications.Where(n => n.Removed == false).OrderByDescending(n => n.ID).ToList();
            ViewBag.notifiCount = profile.Notifications.Where(n => n.Removed == false).Count();

            var zmienna = db.Applications
                .Where(a => (a.StatusOwner == "Zaakceptowane!" || a.StatusGuardian == "Zaakceptowane!")).ToList();

            var applicationsEnded = zmienna.Where(a => a.Offer.EndDate < DateTime.Now
                    && (a.Guardian.Email == User.Identity.Name || a.Owner.Email == User.Identity.Name)
                    && !profile.Notifications.Any(n => n.OfferID == a.OfferID && n.Message.Contains("Oceń"))).ToList();

            foreach (Applications item in applicationsEnded)
            {
                if (item != null && db.Notifications.Any(n => n.OfferID == item.OfferID))
                {
                    db.Notifications.Add(new Notification
                    {
                        OfferID = item.OfferID,
                        ProfileID = item.GuardianID,
                        WhoIRateID = item.OwnerID,
                        Message = "Oferta zakończona. Oceń właściciela!",
                        Removed = false
                    });
                    db.Notifications.Add(new Notification
                    {
                        OfferID = item.OfferID,
                        ProfileID = item.OwnerID,
                        WhoIRateID = item.GuardianID,
                        Message = "Oferta zakończona. Oceń opiekuna!",
                        Removed = false
                    });
                    db.SaveChanges();
                }
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