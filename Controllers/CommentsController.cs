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
        public ActionResult Create(Comments comments)
        {
            if (ModelState.IsValid)
            {
                Profile myProfile = db.Profiles.Single(p => p.Email == User.Identity.Name);
                comments.AuthorID = myProfile.ID;
                if (db.Comments.Where(a => a.Author.Email == User.Identity.Name && a.Grade != 0).Count() > 0)
                {
                    var comms = db.Comments.First(a => a.Author.Email == User.Identity.Name && a.Grade != 0);
                    comms.Grade = comments.Grade;
                    comments.Grade = 0;
                }
                db.Comments.Add(comments);
                db.SaveChanges();
                if (comments.Grade != 0)
                {
                    int id = db.Profiles.Single(p => p.ID == comments.ProfileID).ID;
                    var komy = db.Comments.Where(a => a.ProfileID == id && a.Grade != 0).Select(a => a.Grade);
                    int b = (int)komy.Average() * 20;
                    db.Profiles.Single(p => p.ID == comments.ProfileID).Rate = b;
                }
                db.SaveChanges();
                return RedirectToAction("DetailsAnotherProfile", "Profiles", new { id = comments.ProfileID});
            }

            return RedirectToAction("DetailsAnotherProfile", "Profiles", new { id = comments.ProfileID });
        }

        // GET: Comments/Edit/5
        public ActionResult Edit(int? id)
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

        // POST: Comments/Edit/5
        // Aby zapewnić ochronę przed atakami polegającymi na przesyłaniu dodatkowych danych, włącz określone właściwości, z którymi chcesz utworzyć powiązania.
        // Aby uzyskać więcej szczegółów, zobacz https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Comments comments)
        {
            if (ModelState.IsValid)
            {
                db.SaveChanges();
                
                return RedirectToAction("Index");
            }
            return View(comments);
        }

        public ActionResult Delete(int? idCommm, int profileID)
        {
            if (idCommm == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comments comments = db.Comments.Find(idCommm);
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
