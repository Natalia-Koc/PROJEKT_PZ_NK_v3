﻿using System;
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
using PROJEKT_PZ_NK_v3.ViewModels;

namespace PROJEKT_PZ_NK_v3.Controllers
{
    public class AnimalsController : Controller
    {
        private OfferContext db = new OfferContext();
        // GET: Animals
        public ActionResult Index(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            List<Animal> animals = db.Animals.
                Where(animal => animal.Profiles.ID == id).
                ToList();
            if (animals == null)
            {
                return HttpNotFound();
            }
            return View(animals);
        }

        // GET: Animals/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Animal animal = db.Animals.Find(id);
            if (animal == null)
            {
                return HttpNotFound();
            }
            return View(animal);
        }

        public ActionResult DetailsAnotherAnimal(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Animal animal = db.Animals.Find(id);
            if (animal == null)
            {
                return HttpNotFound();
            }
            return View("Details", animal);
        }

        // GET: Animals/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Animals/Create
        // Aby zapewnić ochronę przed atakami polegającymi na przesyłaniu dodatkowych danych, włącz określone właściwości, z którymi chcesz utworzyć powiązania.
        // Aby uzyskać więcej szczegółów, zobacz https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Animal animal)
        {
            if (ModelState.IsValid)
            {
                Profile pa = db.Profiles.Single(p => p.Email == User.Identity.Name);
                animal.Profiles = pa;

                HttpPostedFileBase file = Request.Files["plikZObrazkiem"];

                if (file != null && file.ContentLength > 0)
                {
                    animal.Image = file.FileName;
                    file.SaveAs(HttpContext.Server.MapPath("~/ImagesAnimals/") + animal.Image);
                }

                db.Animals.Add(animal);
                db.SaveChanges();

                IAsyncSession session = db._driver.AsyncSession();

                try
                {
                    IResultCursor cursor = await session.RunAsync(
                        "MATCH (a:Profile { Email:'" + User.Identity.Name + "'})" +
                        "CREATE(a) -[r: OWNER]->(b: Animal " +
                        "{ Name: '" + animal.Name +
                        "', Species: '" + animal.Species +
                        "', Race: '" + animal.Race +
                        "', Gender: '" + animal.Gender +
                        "', Weight: '" + animal.Weight +
                        "', DateOfBirth: '" + animal.DateOfBirth.Date +
                        "', Description: '" + animal.Description +
                        "', Image: '" + animal.Image +
                        "'})"
                    );
                    await cursor.ConsumeAsync();
                }
                finally
                {
                    await session.CloseAsync();
                }

                await db._driver.CloseAsync();
                return RedirectToAction("Details", "Animals", new { id = animal.ID });
            }

            return View("Details", "Profiles");
        }

        // GET: Animals/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Animal animal = db.Animals.Find(id);
            if (animal == null)
            {
                return HttpNotFound();
            }
            return View(animal);
        }

        // POST: Animals/Edit/5
        // Aby zapewnić ochronę przed atakami polegającymi na przesyłaniu dodatkowych danych, włącz określone właściwości, z którymi chcesz utworzyć powiązania.
        // Aby uzyskać więcej szczegółów, zobacz https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Animal animal)
        {
            if (ModelState.IsValid)
            {
                db.Entry(animal).State = EntityState.Modified;
                HttpPostedFileBase file = Request.Files["plikZObrazkiem"];
                if (file != null && file.ContentLength > 0)
                {
                    animal.Image = file.FileName;
                    file.SaveAs(HttpContext.Server.MapPath("~/ImagesAnimals/") + animal.Image);
                }
                else
                {
                    animal.Image = db.Animals.AsNoTracking().Single(a => a.ID == animal.ID).Image; ;
                }
                db.SaveChanges();
                
                IAsyncSession session = db._driver.AsyncSession();

                try
                {
                    IResultCursor cursor = await session.RunAsync(
                        "MATCH (a:Profile {Email:'"+ User.Identity.Name + "'})-[rel:OWNER]->(b:Animal {Name:'"+ animal.Name + "'})" +
                        "SET b.Species = '" + animal.Species +
                        "', b.Race = '" + animal.Race +
                        "', b.Gender = '" + animal.Gender +
                        "', b.Weight = '" + animal.Weight +
                        "', b.DateOfBirth = '" + animal.DateOfBirth.Date +
                        "', b.Description = '" + animal.Description +
                        "', b.Image = '" + animal.Image + "'");
                    await cursor.ConsumeAsync();
                }
                finally
                {
                    await session.CloseAsync();
                }

                return RedirectToAction("Details", "Animals", new { id = animal.ID });
            }
            return View(animal);
        }

        // GET: Animals/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Animal animal = db.Animals.Find(id);
            if (animal == null)
            {
                return HttpNotFound();
            }
            return View(animal);
        }

        // POST: Animals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Animal animal = db.Animals.Find(id);
            db.Animals.Remove(animal);
            db.SaveChanges();
            return RedirectToAction("Details", "Profiles");
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
