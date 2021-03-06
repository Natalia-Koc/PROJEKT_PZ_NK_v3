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
    public class SavedProfilesController : Controller
    {
        private OfferContext db = new OfferContext();

        // GET: SavedProfiles
        public ActionResult Blocked()
        {
            var savedProfiles = db.SavedProfiles
                .Include(a => a.SavedProfile)
                .Where(a => a.MyProfile.Email == User.Identity.Name
                    && a.SavedAs == Saved.blocked)
                .ToList();
            return View(savedProfiles);
        }

        // GET: SavedProfiles
        public ActionResult Favourite()
        {
            var savedProfiles = db.SavedProfiles
                .Include(a => a.SavedProfile)
                .Where(a => a.MyProfile.Email == User.Identity.Name
                    && a.SavedAs == Saved.favourite)
                .ToList();
            return View(savedProfiles);
        }

        // GET: SavedProfiles/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SavedProfiles savedProfiles = db.SavedProfiles.Find(id);
            if (savedProfiles == null)
            {
                return HttpNotFound();
            }
            return View(savedProfiles);
        }

        public ActionResult NewFavourite(int? id)
        {
            if(id != null)
            {
                SavedProfiles savedProfiles = new SavedProfiles
                {
                    MyProfile = db.Profiles.Single(a => a.Email == User.Identity.Name),
                    SavedProfile = db.Profiles.Single(a => a.ID == id),
                    SavedAs = Saved.favourite
                };
                db.SavedProfiles.Add(savedProfiles);
                db.SaveChanges();
            }
            return RedirectToAction("DetailsAnotherProfile", "Profiles", new { id = id });
        }

        public ActionResult NewBlocked(int? id)
        {
            if (id != null)
            {
                SavedProfiles savedProfiles = new SavedProfiles
                {
                    MyProfile = db.Profiles.Single(a => a.Email == User.Identity.Name),
                    SavedProfile = db.Profiles.Single(a => a.ID == id),
                    SavedAs = Saved.blocked
                };
                db.SavedProfiles.Add(savedProfiles);
                db.SaveChanges();
            }
            return RedirectToAction("DetailsAnotherProfile", "Profiles", new { id = id });
        }


        // GET: SavedProfiles/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SavedProfiles savedProfiles = db.SavedProfiles.Find(id);
            if (savedProfiles == null)
            {
                return HttpNotFound();
            }
            return View(savedProfiles);
        }

        // POST: SavedProfiles/Edit/5
        // Aby zapewnić ochronę przed atakami polegającymi na przesyłaniu dodatkowych danych, włącz określone właściwości, z którymi chcesz utworzyć powiązania.
        // Aby uzyskać więcej szczegółów, zobacz https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,SavedAs")] SavedProfiles savedProfiles)
        {
            if (ModelState.IsValid)
            {
                db.Entry(savedProfiles).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(savedProfiles);
        }

        // GET: SavedProfiles/Delete/5
        public ActionResult DeleteFavourite(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SavedProfiles savedProfiles = db.SavedProfiles.Single(a => a.SavedProfile.ID == id && a.SavedAs == Saved.favourite);
            if (savedProfiles == null)
            {
                return HttpNotFound();
            }
            else
            {
                db.SavedProfiles.Remove(savedProfiles);
                db.SaveChanges();
            }
            return RedirectToAction("DetailsAnotherProfile", "Profiles", new { id });
        }

        public ActionResult DeleteBlocked(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SavedProfiles savedProfiles = db.SavedProfiles.Single(a => a.SavedProfile.ID == id && a.SavedAs == Saved.blocked);
            if (savedProfiles == null)
            {
                return HttpNotFound();
            }
            else
            {
                db.SavedProfiles.Remove(savedProfiles);
                db.SaveChanges();
            }
            return RedirectToAction("DetailsAnotherProfile", "Profiles", new { id });
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
