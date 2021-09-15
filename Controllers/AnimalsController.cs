﻿using System;
using System.Collections.Generic;
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
    public class AnimalsController : Controller
    {
        private OfferContext db = new OfferContext();

        // GET: Animals/Details/5
        public async Task<ActionResult> Details(string email, string name)
        {
            Animal animal = new Animal();

            IAsyncSession session = db._driver.AsyncSession();
            try
            {
                await session.ReadTransactionAsync(async tx =>
                {
                    var cursor =
                        await tx.RunAsync(
                            "match (p:Profile {Email:'"+ email +"'})-[rel:OWNER]->(a:Animal {Name:'"+ name +"'}) return a,p");

                    List<IRecord> Records = await cursor.ToListAsync();
                    foreach (var item in Records)
                    {
                        INode nodeAnimal = (INode)item.Values["a"];
                        INode nodeProfile = (INode)item.Values["p"];

                        Profile profile = new Profile();
                        profile.HouseNumber = nodeProfile.Properties.Values.First().As<string>();
                        profile.Email = nodeProfile.Properties.Values.Skip(1).First().As<string>();
                        profile.Rate = nodeProfile.Properties.Values.Skip(2).First().As<int>();
                        profile.FirstName = nodeProfile.Properties.Values.Skip(3).First().As<string>();
                        profile.Street = nodeProfile.Properties.Values.Skip(4).First().As<string>();
                        profile.PhoneNumber = nodeProfile.Properties.Values.Skip(5).First().As<string>();
                        profile.City = nodeProfile.Properties.Values.Skip(6).First().As<string>();
                        profile.Login = nodeProfile.Properties.Values.Skip(7).First().As<string>();
                        profile.LastName = nodeProfile.Properties.Values.Skip(8).First().As<string>();
                        ViewBag.profile = profile;

                        animal.DateOfBirth = nodeAnimal.Properties.Values.First().As<DateTime>();
                        animal.Description = nodeAnimal.Properties.Values.Skip(1).First().As<string>();
                        animal.Race = nodeAnimal.Properties.Values.Skip(2).First().As<string>();
                        animal.Gender = nodeAnimal.Properties.Values.Skip(3).First().As<string>();
                        animal.Image = nodeAnimal.Properties.Values.Skip(4).First().As<string>();
                        animal.Species = nodeAnimal.Properties.Values.Skip(5).First().As<string>();
                        animal.Weight = nodeAnimal.Properties.Values.Skip(6).First().As<string>();
                        animal.Name = nodeAnimal.Properties.Values.Skip(7).First().As<string>();

                        break;
                    }

                });

            }
            finally
            {
                await session.CloseAsync();
            }
            if (animal == null)
            {
                return HttpNotFound();
            }
            return View(animal);
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
                HttpPostedFileBase file = Request.Files["plikZObrazkiem"];
                if (file != null && file.ContentLength > 0)
                {
                    animal.Image = file.FileName;
                    file.SaveAs(HttpContext.Server.MapPath("~/ImagesAnimals/") + animal.Image);
                }

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

                return RedirectToAction("Details", "Animals", new { email = User.Identity.Name, name = animal.Name });
            }

            return View("Details", "Profiles");
        }

        // GET: Animals/Edit/5
        public async Task<ActionResult> Edit(string name)
        {
            if (name == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Animal animal = new Animal();
            IAsyncSession session = db._driver.AsyncSession();
            try
            {
                await session.ReadTransactionAsync(async tx =>
                {
                    var cursor =
                        await tx.RunAsync(
                            "match (p:Profile {Email:'" + User.Identity.Name + "'})-[rel:OWNER]->(a:Animal {Name:'" + name + "'}) return a,p");

                    IRecord Record = await cursor.SingleAsync();
                    INode nodeAnimal = (INode)Record.Values["a"];

                    animal.ID = nodeAnimal.Id.As<int>();
                    animal.DateOfBirth = nodeAnimal.Properties.Values.First().As<DateTime>();
                    animal.Description = nodeAnimal.Properties.Values.Skip(1).First().As<string>();
                    animal.Race = nodeAnimal.Properties.Values.Skip(2).First().As<string>();
                    animal.Gender = nodeAnimal.Properties.Values.Skip(3).First().As<string>();
                    animal.Image = nodeAnimal.Properties.Values.Skip(4).First().As<string>();
                    animal.Species = nodeAnimal.Properties.Values.Skip(5).First().As<string>();
                    animal.Weight = nodeAnimal.Properties.Values.Skip(6).First().As<string>();
                    animal.Name = nodeAnimal.Properties.Values.Skip(7).First().As<string>();

                });

            }
            finally
            {
                await session.CloseAsync();
            }
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
                IAsyncSession session = db._driver.AsyncSession();
                try
                {
                    HttpPostedFileBase file = Request.Files["plikZObrazkiem"];
                    if (file != null && file.ContentLength > 0)
                    {
                        animal.Image = file.FileName;
                        file.SaveAs(HttpContext.Server.MapPath("~/ImagesAnimals/") + animal.Image);
                        IResultCursor cursor2 = await session.RunAsync(
                            "MATCH (a:Profile {Email:'" + User.Identity.Name + "'})-[rel:OWNER]->(b:Animal)" +
                            "WHERE id(b)=" + animal.ID +
                            " SET b.Image = '" + animal.Image + "'");
                        await cursor2.ConsumeAsync();
                    }

                    IResultCursor cursor = await session.RunAsync(
                        "MATCH (a:Profile {Email:'" + User.Identity.Name + "'})-[rel:OWNER]->(b:Animal)" +
                        "WHERE id(b)=" + animal.ID +
                        " SET b.Species = '" + animal.Species +
                        "', b.Name = '" + animal.Name +
                        "', b.Race = '" + animal.Race +
                        "', b.Gender = '" + animal.Gender +
                        "', b.Weight = '" + animal.Weight +
                        "', b.DateOfBirth = '" + animal.DateOfBirth.Date +
                        "', b.Description = '" + animal.Description +"'");
                    await cursor.ConsumeAsync();

                }
                finally
                {
                    await session.CloseAsync();
                }

                return RedirectToAction("Details", "Animals", new { email = User.Identity.Name, name = animal.Name });
            }
            return View(animal);
        }

        // GET: Animals/Delete/5
        public async Task<ActionResult> Delete(string name)
        {
            if (name == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Animal animal = new Animal();
            IAsyncSession session = db._driver.AsyncSession();
            try
            {
                await session.ReadTransactionAsync(async tx =>
                {
                    var cursor =
                        await tx.RunAsync(
                            "match (p:Profile {Email:'" + User.Identity.Name + "'})-[rel:OWNER]->(a:Animal {Name:'" + name + "'}) return a,p");

                    IRecord Record = await cursor.SingleAsync();
                    INode nodeAnimal = (INode)Record.Values["a"];

                    animal.DateOfBirth = nodeAnimal.Properties.Values.First().As<DateTime>();
                    animal.Description = nodeAnimal.Properties.Values.Skip(1).First().As<string>();
                    animal.Race = nodeAnimal.Properties.Values.Skip(2).First().As<string>();
                    animal.Gender = nodeAnimal.Properties.Values.Skip(3).First().As<string>();
                    animal.Image = nodeAnimal.Properties.Values.Skip(4).First().As<string>();
                    animal.Species = nodeAnimal.Properties.Values.Skip(5).First().As<string>();
                    animal.Weight = nodeAnimal.Properties.Values.Skip(6).First().As<string>();
                    animal.Name = nodeAnimal.Properties.Values.Skip(7).First().As<string>();

                });

            }
            finally
            {
                await session.CloseAsync();
            }
            if (animal == null)
            {
                return HttpNotFound();
            }
            return View(animal);
        }

        // POST: Animals/Delete/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string name)
        {
            IAsyncSession session = db._driver.AsyncSession();
            try
            {
                IResultCursor cursor = await session.RunAsync(
                    "match (p:Profile {Email:'"+ User.Identity.Name +"'})" +
                        "-[rel:OWNER]->(a:Animal {Name:'"+ name +"'})" +
                        "-[rell:ANIMAL_OFFER]->(o:Offer)" +
                        "<-[relll:NOTIFICATION_TO_THE_OFFER]-(app:Application)" +
                        "DETACH delete a,o,app");
                await cursor.ConsumeAsync();

                IResultCursor cursor2 = await session.RunAsync(
                    "match (p:Profile {Email:'" + User.Identity.Name + "'})" +
                        "-[rel:OWNER]->(a:Animal {Name:'" + name + "'})" +
                        "-[rell:ANIMAL_OFFER]->(o:Offer)" +
                        "DETACH delete a,o");
                await cursor2.ConsumeAsync();

                IResultCursor cursor3 = await session.RunAsync(
                    "match (p:Profile {Email:'" + User.Identity.Name + "'})" +
                        "-[rel:OWNER]->(a:Animal {Name:'" + name + "'})" +
                        "DETACH delete a");
                await cursor3.ConsumeAsync();
            }
            finally
            {
                await session.CloseAsync();
            }
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
