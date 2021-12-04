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
            return PartialView("Index");
        }

    }
}