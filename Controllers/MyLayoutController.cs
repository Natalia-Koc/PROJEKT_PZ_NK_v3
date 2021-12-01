using PROJEKT_PZ_NK_v3.DAL;
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
            var notifi = new List<string>() {
                "Category 1", "Category 2", "Category 3", "Category 4"
            };
            ViewBag.notifi = notifi;
            return PartialView("Index");
        }
    }
}