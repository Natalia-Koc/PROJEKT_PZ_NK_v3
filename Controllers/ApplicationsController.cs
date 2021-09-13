using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Neo4j.Driver;
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
                && a.Status != "Opiekun zrezygnował z oferty"
                && a.Status != "Odrzucone"
                && !a.Status.Contains("Usunieta"));
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
                && a.Status != "Właściciel odrzucił ofertę"
                && a.Status != "Odrzucone"
                && !a.Status.Contains("Usunieta"));
            return View(applications.ToList());
        }

        public ActionResult History()
        {
            var applications = db.Applications
                .Include(a => a.Guardian)
                .Include(a => a.Offer)
                .Include(a => a.Owner)
                .Where(a => (a.Offer.StartingDate < DateTime.Now || 
                    (a.Status == "Właściciel odrzucił ofertę" && a.Owner.Email == User.Identity.Name) ||
                    (a.Status == "Opiekun zrezygnował z oferty" && a.Guardian.Email == User.Identity.Name) ||
                    a.Status == "Odrzucone" ||
                    !a.Status.Contains("Usunieta")) && 
                    !a.Status.Contains(User.Identity.Name));
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
        public async Task<ActionResult> Create(int offerID, int ownerID, Applications application)
        {
            application.Guardian = db.Profiles.Single(p => p.Email == User.Identity.Name);
            application.GuardianID = db.Profiles.Single(p => p.Email == User.Identity.Name).ID;
            application.Owner = db.Profiles.Find(ownerID);
            application.OwnerID = ownerID;
            application.Offer = db.Offers.Find(offerID);
            application.OfferID = offerID;
            application.Status = "Oczekuje na akceptacje";
            db.Applications.Add(application);
            db.SaveChanges();

            IAsyncSession session = db._driver.AsyncSession();
            try
            {
                IResultCursor cursor = await session.RunAsync("" +
                    "MATCH (n:Profile {Email: '" + User.Identity.Name + "'}), " +
                        "(n2:Profile {Email: '" + application.Owner.Email + "'})," +
                        "(o:Offer {OfferID: " + application.OfferID + "})" +
                    "CREATE(n) -[r: GUARDIAN]-> (p: Application " +
                    "{ Status: '" + application.Status +
                    "', Message: '" + application.Message +"'})," +
                    "(n2) -[t: OWNER]-> (p)," +
                    "(p) -[y: NOTIFICATION_TO_THE_OFFER]-> (o)"
                );
                await cursor.ConsumeAsync();
            }
            finally
            {
                await session.CloseAsync();
            }

            return RedirectToAction("Details", "Offers", new { id = offerID });
        }


        // GET: Applications/Edit/5
        public async Task<ActionResult> EditResign(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            IAsyncSession session = db._driver.AsyncSession();
            try
            {
                if (db.Applications.Find(id).Status == "Właściciel odrzucił ofertę")
                {
                    db.Applications.Find(id).Status = "Odrzucone";
                    IResultCursor cursor = await session.RunAsync(
                        "match (k:Profile {Email: '"+ User.Identity.Name + "'})-[rk:GUARDIAN]->(app:Application)" +
                            "-[ro:NOTIFICATION_TO_THE_OFFER]->(o:Offer {OfferID:"+ db.Applications.Find(id).OfferID + "})" +
                        "SET app.Status = 'Odrzucone'"
                    );
                    await cursor.ConsumeAsync();
                }
                else
                {
                    db.Applications.Find(id).Status = "Opiekun zrezygnował z oferty";
                    IResultCursor cursor = await session.RunAsync(
                        "match (k:Profile {Email: '" + User.Identity.Name + "'})-[rk:GUARDIAN]->(app:Application)" +
                            "-[ro:NOTIFICATION_TO_THE_OFFER]->(o:Offer {OfferID:" + db.Applications.Find(id).OfferID + "})" +
                        "SET app.Status = 'Opiekun zrezygnował z oferty'"
                    );
                    await cursor.ConsumeAsync();
                }
            }
            finally
            {
                await session.CloseAsync();
            }
            db.SaveChanges();
            return RedirectToAction("MyApplications");
        }

        public async Task<ActionResult> EditDiscard(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            IAsyncSession session = db._driver.AsyncSession();
            try
            {
                if (db.Applications.Find(id).Status == "Opiekun zrezygnował z oferty")
                {
                    db.Applications.Find(id).Status = "Odrzucone";
                    IResultCursor cursor = await session.RunAsync(
                        "match (k:Profile {Email: '" + User.Identity.Name + "'})-[rk:OWNER]->(app:Application)" +
                            "-[ro:NOTIFICATION_TO_THE_OFFER]->(o:Offer {OfferID:" + db.Applications.Find(id).OfferID + "})" +
                        "SET app.Status = 'Odrzucone'"
                    );
                    await cursor.ConsumeAsync();
                }
                else
                {
                    db.Applications.Find(id).Status = "Właściciel odrzucił ofertę";
                    IResultCursor cursor = await session.RunAsync(
                        "match (k:Profile {Email: '" + User.Identity.Name + "'})-[rk:OWNER]->(app:Application)" +
                            "-[ro:NOTIFICATION_TO_THE_OFFER]->(o:Offer {OfferID:" + db.Applications.Find(id).OfferID + "})" +
                        "SET app.Status = 'Właściciel odrzucił ofertę'"
                    );
                    await cursor.ConsumeAsync();
                }
            }
            finally
            {
                await session.CloseAsync();
            }
            db.SaveChanges();
            return RedirectToAction("ApplicationsToMyOffers");
        }

        public async Task<ActionResult> EditAccept(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            db.Applications.Find(id).Status = "Zaakceptowane!";
            db.SaveChanges();
            IAsyncSession session = db._driver.AsyncSession();
            try
            {
                IResultCursor cursor = await session.RunAsync(
                    "match (k:Profile {Email: '" + User.Identity.Name + "'})-[rk:OWNER]->(app:Application)" +
                        "-[ro:NOTIFICATION_TO_THE_OFFER]->(o:Offer {OfferID:" + db.Applications.Find(id).OfferID + "})" +
                    "SET app.Status = 'Zaakceptowane'"
                );
                await cursor.ConsumeAsync();
            }
            finally
            {
                await session.CloseAsync();
            }
            return RedirectToAction("ApplicationsToMyOffers");
        }

        // GET: Applications/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            Applications applications = db.Applications.Find(id);
            IAsyncSession session = db._driver.AsyncSession();
            try
            {
                if (applications.Status.Contains("Usunieta"))
                {
                    db.Applications.Find(id).Status = "Usunieta";
                    IResultCursor cursor = await session.RunAsync(
                        "match (app:Application)-[ro:NOTIFICATION_TO_THE_OFFER]->(o:Offer {OfferID:" + db.Applications.Find(id).OfferID + "})" +
                        "SET app.Status = 'Usunieta'"
                    );
                    await cursor.ConsumeAsync();
                }
                else
                {
                    db.Applications.Find(id).Status = "Usunieta przez " + User.Identity.Name;
                    IResultCursor cursor = await session.RunAsync(
                        "match (app:Application)-[ro:NOTIFICATION_TO_THE_OFFER]->(o:Offer {OfferID:" + db.Applications.Find(id).OfferID + "})" +
                        "SET app.Status = 'Usunieta przez " + User.Identity.Name +"'"
                    );
                    await cursor.ConsumeAsync();
                }
            }
            finally
            {
                await session.CloseAsync();
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
