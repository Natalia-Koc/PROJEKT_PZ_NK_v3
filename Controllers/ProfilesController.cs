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
using Neo4jClient;
using PROJEKT_PZ_NK_v3.DAL;
using PROJEKT_PZ_NK_v3.Models;
using PROJEKT_PZ_NK_v3.ViewModels;

namespace PROJEKT_PZ_NK_v3.Controllers
{
    public class ProfilesController : Controller
    {
        private OfferContext db = new OfferContext();

        public ActionResult Ranking()
        {
            var profiles = db.Profiles.OrderByDescending(p => p.Rate).Take(20).ToList();
            return View(profiles);
        }
        public async Task<ActionResult> Details()
        {
            Profile profile = db.Profiles.FirstOrDefault(p => p.Email == User.Identity.Name);
            Comments comments = new Comments();
            ViewBag.comments = comments;
            ViewBag.ProgressBarCount = db.Comments.Where(m => m.Profile.Email == User.Identity.Name && m.Grade != 0).Count();
            ViewBag.FoundComment = db.Comments.Any(m => m.Author.Email == User.Identity.Name);

            IAsyncSession session = db._driver.AsyncSession();
            try
            {

                await session.ReadTransactionAsync(async tx =>
                {
                    var cursor =
                        await tx.RunAsync(
                            "MATCH (a:Profile) WHERE a.Email = $email RETURN a",
                            new { email = User.Identity.Name });

                    /*ViewBag.Profile = cursor.SingleAsync().Result.Values.First().Value.As<string>();*/

                    List<IRecord> Records = await cursor.ToListAsync();
                    foreach (var item in Records)
                    {
                        INode node = (INode)item.Values["a"];
                        List<string> lista = new List<string>();
                        foreach (var a in node.Properties)
                        {
                            int b = a.Key.IndexOf("id");
                            lista.Add(a.Value.As<string>());
                        }
                        ViewBag.Profile = lista;
                        break;
                    }

                });
                
            }
            finally
            {
                await session.CloseAsync();
            }
            await db._driver.CloseAsync();

            return View(profile);
        }

        public ActionResult DetailsAnotherProfile(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Profile 
            profile = db.Profiles.Find(id); 
            ViewBag.ProgressBarCount = db.Comments.Where(m => m.ProfileID == id && m.Grade != 0).Count();
            ViewBag.FoundComment = db.Comments.Any(m => m.Author.Email == User.Identity.Name && m.ProfileID == id);
            if (!ViewBag.FoundComment)
            {
                ViewBag.MyComment = null;
            }
            else
            {
                ViewBag.MyComment = db.Comments.First(m => m.Author.Email == User.Identity.Name);
            }

            if (profile == null)
            {
                return HttpNotFound();
            }
            return View("Details", profile);
        }

        // GET: Profiles/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var profile = db.Profiles.Find(id);

            if (profile == null)
            {
                return HttpNotFound();
            }
            return View(profile);
        }

        // POST: Profiles/Edit/5
        // Aby zapewnić ochronę przed atakami polegającymi na przesyłaniu dodatkowych danych, włącz określone właściwości, z którymi chcesz utworzyć powiązania.
        // Aby uzyskać więcej szczegółów, zobacz https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Profile profile)
        {
            if (ModelState.IsValid)
            {
                profile.Rate = 0;
                db.Entry(profile).State = EntityState.Modified;
                db.SaveChanges();

                IAsyncSession session = db._driver.AsyncSession();

                try
                {
                    IResultCursor cursor = await session.RunAsync("match (a:Profile {Email: '" + User.Identity.Name + "'})" +
                        "SET a.Login = '" + profile.Login + "'," +
                        "a.FirstName = '" + profile.FirstName + "'," +
                        "a.LastName = '" + profile.LastName + "'," +
                        "a.PhoneNumber = '" + profile.PhoneNumber + "'," +
                        "a.City = '" + profile.City + "'," +
                        "a.Street = '" + profile.Street + "'," +
                        "a.HouseNumber = '" + profile.HouseNumber + "'");
                    await cursor.ConsumeAsync();
                }
                finally
                {
                    await session.CloseAsync();
                }
                await db._driver.CloseAsync();
                return RedirectToAction("Details");
            }
            return View(profile);
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
