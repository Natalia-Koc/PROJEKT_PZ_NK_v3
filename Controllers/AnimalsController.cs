using System;
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

                        ViewBag.profile = new Profile
                        {
                            ID = ((int)nodeProfile.Id),
                            HouseNumber = nodeProfile.Properties.Where(a => a.Key == "HouseNumber").Select(a => a.Value).First().As<string>(),
                            Email = nodeProfile.Properties.Where(a => a.Key == "Email").Select(a => a.Value).First().As<string>(),
                            Rate = nodeProfile.Properties.Where(a => a.Key == "Rate").Select(a => a.Value).First().As<int>(),
                            FirstName = nodeProfile.Properties.Where(a => a.Key == "FirstName").Select(a => a.Value).First().As<string>(),
                            Street = nodeProfile.Properties.Where(a => a.Key == "Street").Select(a => a.Value).First().As<string>(),
                            PhoneNumber = nodeProfile.Properties.Where(a => a.Key == "PhoneNumber").Select(a => a.Value).First().As<string>(),
                            City = nodeProfile.Properties.Where(a => a.Key == "City").Select(a => a.Value).First().As<string>(),
                            Login = nodeProfile.Properties.Where(a => a.Key == "Login").Select(a => a.Value).First().As<string>(),
                            LastName = nodeProfile.Properties.Where(a => a.Key == "LastName").Select(a => a.Value).First().As<string>()
                        };

                        animal.DateOfBirth = nodeAnimal.Properties.Where(a => a.Key == "DateOfBirth").Select(a => a.Value).First().As<DateTime>();
                        animal.Description = nodeAnimal.Properties.Where(a => a.Key == "Description").Select(a => a.Value).First().As<string>();
                        animal.Race = nodeAnimal.Properties.Where(a => a.Key == "Race").Select(a => a.Value).First().As<string>();
                        animal.Gender = nodeAnimal.Properties.Where(a => a.Key == "Gender").Select(a => a.Value).First().As<string>();
                        animal.Image = nodeAnimal.Properties.Where(a => a.Key == "Image").Select(a => a.Value).First().As<string>();
                        animal.Species = nodeAnimal.Properties.Where(a => a.Key == "Species").Select(a => a.Value).First().As<string>();
                        animal.Weight = nodeAnimal.Properties.Where(a => a.Key == "Weight").Select(a => a.Value).First().As<string>();
                        animal.Name = nodeAnimal.Properties.Where(a => a.Key == "Name").Select(a => a.Value).First().As<string>();

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
                    animal.DateOfBirth = nodeAnimal.Properties.Where(a => a.Key == "DateOfBirth").Select(a => a.Value).First().As<DateTime>();
                    animal.Description = nodeAnimal.Properties.Where(a => a.Key == "Description").Select(a => a.Value).First().As<string>();
                    animal.Race = nodeAnimal.Properties.Where(a => a.Key == "Race").Select(a => a.Value).First().As<string>();
                    animal.Gender = nodeAnimal.Properties.Where(a => a.Key == "Gender").Select(a => a.Value).First().As<string>();
                    animal.Image = nodeAnimal.Properties.Where(a => a.Key == "Image").Select(a => a.Value).First().As<string>();
                    animal.Species = nodeAnimal.Properties.Where(a => a.Key == "Species").Select(a => a.Value).First().As<string>();
                    animal.Weight = nodeAnimal.Properties.Where(a => a.Key == "Weight").Select(a => a.Value).First().As<string>();
                    animal.Name = nodeAnimal.Properties.Where(a => a.Key == "Name").Select(a => a.Value).First().As<string>();

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

                    animal.DateOfBirth = nodeAnimal.Properties.Where(a => a.Key == "DateOfBirth").Select(a => a.Value).First().As<DateTime>();
                    animal.Description = nodeAnimal.Properties.Where(a => a.Key == "Description").Select(a => a.Value).First().As<string>();
                    animal.Race = nodeAnimal.Properties.Where(a => a.Key == "Race").Select(a => a.Value).First().As<string>();
                    animal.Gender = nodeAnimal.Properties.Where(a => a.Key == "Gender").Select(a => a.Value).First().As<string>();
                    animal.Image = nodeAnimal.Properties.Where(a => a.Key == "Image").Select(a => a.Value).First().As<string>();
                    animal.Species = nodeAnimal.Properties.Where(a => a.Key == "Species").Select(a => a.Value).First().As<string>();
                    animal.Weight = nodeAnimal.Properties.Where(a => a.Key == "Weight").Select(a => a.Value).First().As<string>();
                    animal.Name = nodeAnimal.Properties.Where(a => a.Key == "Name").Select(a => a.Value).First().As<string>();

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
