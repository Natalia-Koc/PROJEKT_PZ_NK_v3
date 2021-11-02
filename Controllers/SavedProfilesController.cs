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
        public ActionResult Index()
        {
            return View(db.SavedProfiles.ToList());
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

        // GET: SavedProfiles/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: SavedProfiles/Create
        // Aby zapewnić ochronę przed atakami polegającymi na przesyłaniu dodatkowych danych, włącz określone właściwości, z którymi chcesz utworzyć powiązania.
        // Aby uzyskać więcej szczegółów, zobacz https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,SavedAs")] SavedProfiles savedProfiles)
        {
            if (ModelState.IsValid)
            {
                db.SavedProfiles.Add(savedProfiles);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(savedProfiles);
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
        public ActionResult Delete(int? id)
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

        // POST: SavedProfiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SavedProfiles savedProfiles = db.SavedProfiles.Find(id);
            db.SavedProfiles.Remove(savedProfiles);
            db.SaveChanges();
            return RedirectToAction("Index");
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
