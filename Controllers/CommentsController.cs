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
    public class CommentsController : Controller
    {
        private OfferContext db = new OfferContext();

        // GET: Comments
        public ActionResult Index()
        {
            return View(db.Comments.ToList());
        }

        // GET: Comments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comments comments = db.Comments.Find(id);
            
            if (comments == null)
            {
                return HttpNotFound();
            }
            return View(comments);
        }

        // GET: Comments/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Comments/Create
        // Aby zapewnić ochronę przed atakami polegającymi na przesyłaniu dodatkowych danych, włącz określone właściwości, z którymi chcesz utworzyć powiązania.
        // Aby uzyskać więcej szczegółów, zobacz https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<ActionResult> Create(Comments comments)
        {
            if (ModelState.IsValid)
            {
                Profile myProfile = db.Profiles.Single(p => p.Email == User.Identity.Name);
                comments.AuthorID = myProfile.ID;
                comments.Profile = db.Profiles.Single(a => a.ID == comments.ProfileID);
                db.Comments.Add(comments);
                db.SaveChanges();
                if (comments.Grade != 0)
                {
                    int id = db.Profiles.Single(p => p.ID == comments.ProfileID).ID;
                    var average = db.Comments.Where(a => a.ProfileID == id && a.Grade != 0).Select(a => a.Grade).Average() * 20;
                    db.Profiles.Single(p => p.ID == comments.ProfileID).Rate = (int) average;
                }
                db.SaveChanges();

                IAsyncSession session = db._driver.AsyncSession();

                try
                {
                    IResultCursor cursor = await session.RunAsync(
                        "MATCH (n:Profile {Email: '" + User.Identity.Name + "'}), (p2:Profile {Email: '" + comments.Profile.Email + "'}) " +
                        "CREATE(n) -[r:AUTHOR]-> (p:Comment {contents: '" + comments.Contents + "', rate: " + comments.Grade + "})," +
                        "(p) -[t:COMMENTED_PROFILE]-> (p2)"
                    );
                    await cursor.ConsumeAsync();
                }
                finally
                {
                    await session.CloseAsync();
                }
            }

            return RedirectToAction("DetailsAnotherProfile", "Profiles", new { id = comments.ProfileID });
        }

        public async Task<ActionResult> Delete(int profileID)
        {
            Comments comments = db.Comments.Single(a => a.Author.Email == User.Identity.Name && a.ProfileID == profileID);
            if (comments == null)
            {
                return HttpNotFound();
            }
            db.Comments.Remove(comments);
            db.SaveChanges();

            if (db.Profiles.Single(p => p.ID == profileID).Comments.Where(a => a.Grade > 0).Count() > 0)
            {
                db.Profiles.
                    Single(p => p.ID == profileID)
                    .Rate = (int)db.Comments
                    .Where(a => a.ProfileID == profileID && a.Grade != 0)
                    .Select(a => a.Grade)
                    .Average() * 20;
            }
            else
            {
                db.Profiles
                    .Single(p => p.ID == profileID)
                    .Rate = 0;
            }
            db.SaveChanges();
            return RedirectToAction("DetailsAnotherProfile", "Profiles", new { id = profileID});
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
