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

            var applicationsEnded = db.Applications
                .Where(a => (a.StatusOwner == "Zaakceptowane!" || a.StatusGuardian == "Zaakceptowane!")
                    && a.Offer.EndDate >= DateTime.Now).First();
            if(applicationsEnded != null && db.Notifications.Find(applicationsEnded.OfferID) != null)
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

            return PartialView("Index");
        }

    }
}