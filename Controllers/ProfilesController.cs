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
        public async Task<ActionResult> Ranking()
        {
            List<Offer> offers = new List<Offer>();
            List<Profile> profiles = new List<Profile>();
            IAsyncSession session = db._driver.AsyncSession();
            try
            {
                await session.ReadTransactionAsync(async tx =>
                {
                    var cursor =
                        await tx.RunAsync(
                            "match (p:Profile)-[rel:AUTHOR]->(o:Offer)<-[rel2:ANIMAL_OFFER]-(a:Animal) return p,o");

                    List<IRecord> Records = await cursor.ToListAsync();
                    foreach (var item in Records)
                    {
                        INode nodeProfile = (INode)item.Values["p"];
                        INode nodeOffer = (INode)item.Values["o"];
                        INode nodeAnimal = (INode)item.Values["a"];

                        Profile profile = new Profile
                        {
                            HouseNumber = nodeProfile.Properties.Values.First().As<string>(),
                            Email = nodeProfile.Properties.Values.Skip(1).First().As<string>(),
                            Rate = nodeProfile.Properties.Values.Skip(2).First().As<int>(),
                            FirstName = nodeProfile.Properties.Values.Skip(3).First().As<string>(),
                            Street = nodeProfile.Properties.Values.Skip(4).First().As<string>(),
                            PhoneNumber = nodeProfile.Properties.Values.Skip(5).First().As<string>(),
                            City = nodeProfile.Properties.Values.Skip(6).First().As<string>(),
                            Login = nodeProfile.Properties.Values.Skip(7).First().As<string>(),
                            LastName = nodeProfile.Properties.Values.Skip(8).First().As<string>()
                        };

                        Animal animal = new Animal
                        {
                            DateOfBirth = nodeAnimal.Properties.Values.First().As<DateTime>(),
                            Description = nodeAnimal.Properties.Values.Skip(1).First().As<string>(),
                            Race = nodeAnimal.Properties.Values.Skip(2).First().As<string>(),
                            Gender = nodeAnimal.Properties.Values.Skip(3).First().As<string>(),
                            Image = nodeAnimal.Properties.Values.Skip(4).First().As<string>(),
                            Species = nodeAnimal.Properties.Values.Skip(5).First().As<string>(),
                            Weight = nodeAnimal.Properties.Values.Skip(6).First().As<string>(),
                            Name = nodeAnimal.Properties.Values.Skip(7).First().As<string>()
                        };

                        Offer offer = new Offer
                        {
                            Profile = profile,
                            Animal = animal,
                            AnimalName = animal.Name,
                            Description = nodeOffer.Properties.Values.First().As<string>(),
                            EndDate = nodeOffer.Properties.Values.Skip(1).First().As<DateTime>(),
                            ID = nodeOffer.Properties.Values.Skip(2).First().As<int>(),
                            StartingDate = nodeOffer.Properties.Values.Skip(3).First().As<DateTime>(),
                            Title = nodeOffer.Properties.Values.Skip(4).First().As<string>()
                        };
                        offers.Add(offer);
                        profiles.Add(profile);

                    }
                    ViewBag.Offers = offers;
                });

            }
            finally
            {
                await session.CloseAsync();
            }
            return View(profiles.OrderByDescending(p => p.Rate).Take(20).ToList());
        }
        public async Task<ActionResult> Details()
        {
            Profile myProfile = new Profile();
            IAsyncSession session = db._driver.AsyncSession();
            try
            {
                await session.ReadTransactionAsync(async tx =>
                {

                    var cursorProfile =
                        await tx.RunAsync(
                            "match (p:Profile {Email: '" + User.Identity.Name + "'}) " +
                            "return p");
                        List<IRecord> Record2 = await cursorProfile.ToListAsync();
                        foreach (var item in Record2)
                        {
                            INode nodeProfile = (INode)item.Values["p"];

                            myProfile.HouseNumber = nodeProfile.Properties.Values.First().As<string>();
                            myProfile.Email = nodeProfile.Properties.Values.Skip(1).First().As<string>();
                            myProfile.Rate = nodeProfile.Properties.Values.Skip(2).First().As<int>();
                            myProfile.FirstName = nodeProfile.Properties.Values.Skip(3).First().As<string>();
                            myProfile.Street = nodeProfile.Properties.Values.Skip(4).First().As<string>();
                            myProfile.PhoneNumber = nodeProfile.Properties.Values.Skip(5).First().As<string>();
                            myProfile.City = nodeProfile.Properties.Values.Skip(6).First().As<string>();
                            myProfile.Login = nodeProfile.Properties.Values.Skip(7).First().As<string>();
                            myProfile.LastName = nodeProfile.Properties.Values.Skip(8).First().As<string>();

                            break;
                        }

                        var cursor =
                            await tx.RunAsync(
                                "match (p:Profile {Email: '" + User.Identity.Name + "'})<-[rel:COMMENTED_PROFILE]-(c:Comment) " +
                                "return c");
                        if (cursor.FetchAsync().IsCompleted)
                        {
                            List<IRecord> records = await cursor.ToListAsync();
                            List<Comments> comments = new List<Comments>();
                            foreach (var item in records)
                            {
                                INode nodeComment = (INode)item.Values["c"];
                                Comments comm = new Comments
                                {
                                    Contents = nodeComment.Properties.Values.First().As<string>(),
                                    Grade = nodeComment.Properties.Values.Skip(1).First().As<int>(),
                                    ProfilEmail = myProfile.Email,
                                    ProfilLogin = myProfile.Login
                                };

                                comments.Add(comm);
                            }

                            ViewBag.ProgressBarCount = comments.Where(m => m.Grade != 0).Count();
                            ViewBag.FoundComment = true;
                    } 
                    else
                    {
                        ViewBag.ProgressBarCount = 0;
                        ViewBag.FoundComment = true;
                    }


                });

                await session.ReadTransactionAsync(async tx =>
                {
                    var cursor =
                        await tx.RunAsync(
                            "match (p:Profile)-[rell:AUTHOR]->(c:Comment)-[rel:COMMENTED_PROFILE]->(p2:Profile {Email: '" + User.Identity.Name + "'}) " +
                            "return c, p");

                    /*ViewBag.Profile = cursor.SingleAsync().Result.Values.First().Value.As<string>();*/
                        List<Comments> lista = new List<Comments>();

                    if (cursor.FetchAsync().IsCompleted)
                    {
                        List<IRecord> Records = await cursor.ToListAsync();
                        foreach (var item in Records)
                        {
                            INode node = (INode)item.Values["c"];
                            INode nodeProfile = (INode)item.Values["p"];
                            Comments comm = new Comments();
                            comm.Contents = node.Properties.Values.First().As<string>();
                            comm.Grade = node.Properties.Values.Skip(1).First().As<int>();
                            comm.ProfilEmail = nodeProfile.Properties.Values.Skip(1).First().As<string>();
                            comm.ProfilLogin = nodeProfile.Properties.Values.Skip(7).First().As<string>();

                            lista.Add(comm);
                        }
                    } 
                        ViewBag.Comments = lista;
                    

                });

                List<Animal> animals = new List<Animal>();
                await session.ReadTransactionAsync(async tx =>
                {
                    var cursor =
                        await tx.RunAsync(
                            "match (p:Profile {Email: '" + User.Identity.Name + "'})-[rel:OWNER]->(a:Animal) return a");

                    if(cursor.FetchAsync().IsCompleted)
                    {
                        List<IRecord> Records = await cursor.ToListAsync();
                        foreach (var item in Records)
                        {
                            INode nodeAnimal = (INode)item.Values["a"];

                            Animal animal = new Animal
                            {
                                DateOfBirth = nodeAnimal.Properties.Values.First().As<DateTime>(),
                                Description = nodeAnimal.Properties.Values.Skip(1).First().As<string>(),
                                Race = nodeAnimal.Properties.Values.Skip(2).First().As<string>(),
                                Gender = nodeAnimal.Properties.Values.Skip(3).First().As<string>(),
                                Image = nodeAnimal.Properties.Values.Skip(4).First().As<string>(),
                                Species = nodeAnimal.Properties.Values.Skip(5).First().As<string>(),
                                Weight = nodeAnimal.Properties.Values.Skip(6).First().As<string>(),
                                Name = nodeAnimal.Properties.Values.Skip(7).First().As<string>()
                            };
                            animals.Add(animal);
                        }
                    }
                    
                    ViewBag.Animals = animals;
                });
            }
            finally
            {
                await session.CloseAsync();
            }

            return View(myProfile);
        }

        public async Task<ActionResult> DetailsAnotherProfile(string email)
        {
            ViewBag.Animals = null;
            Profile anotherProfile = new Profile();
            IAsyncSession session = db._driver.AsyncSession();
            try
            {
                await session.ReadTransactionAsync(async tx =>
                {
                    var cursor =
                        await tx.RunAsync(
                            "match (p:Profile {Email: '" + email + "'})<-[rel:COMMENTED_PROFILE]-(c:Comment)<-[a:AUTHOR]-(p2:Profile) " +
                            "return p,c,p2");

                    IRecord Record = await cursor.SingleAsync();
                    INode nodeProfile = (INode)Record.Values["p"];

                    anotherProfile.HouseNumber = nodeProfile.Properties.Values.First().As<string>();
                    anotherProfile.Email = nodeProfile.Properties.Values.Skip(1).First().As<string>();
                    anotherProfile.Rate = nodeProfile.Properties.Values.Skip(2).First().As<int>();
                    anotherProfile.FirstName = nodeProfile.Properties.Values.Skip(3).First().As<string>();
                    anotherProfile.Street = nodeProfile.Properties.Values.Skip(4).First().As<string>();
                    anotherProfile.PhoneNumber = nodeProfile.Properties.Values.Skip(5).First().As<string>();
                    anotherProfile.City = nodeProfile.Properties.Values.Skip(6).First().As<string>();
                    anotherProfile.Login = nodeProfile.Properties.Values.Skip(7).First().As<string>();
                    anotherProfile.LastName = nodeProfile.Properties.Values.Skip(8).First().As<string>();

                    List<IRecord> records = await cursor.ToListAsync();
                    List<Comments> comments = new List<Comments>();
                    foreach (var item in records)
                    {
                        INode nodeComment = (INode)item.Values["c"];
                        INode nodeAuthor = (INode)item.Values["p2"];
                        Comments comm = new Comments
                        {
                            Contents = nodeComment.Properties.Values.First().As<string>(),
                            Grade = nodeComment.Properties.Values.Skip(1).First().As<int>(),
                            ProfilEmail = nodeAuthor.Properties.Values.Skip(1).First().As<string>(),
                            ProfilLogin = nodeAuthor.Properties.Values.Skip(7).First().As<string>()
                        };

                        comments.Add(comm);
                    }
                    ViewBag.Comments = comments;

                    ViewBag.ProgressBarCount = comments.Where(m => m.Grade != 0).Count();
                    ViewBag.FoundComment = comments.Any();
                    if (!ViewBag.FoundComment)
                    {
                        ViewBag.MyComment = null;
                    }
                    else
                    {
                        ViewBag.MyComment = comments.First(m => m.ProfilEmail == User.Identity.Name);
                    }
                });
            }
            finally
            {
                await session.CloseAsync();
            }

            if (anotherProfile == null)
            {
                return HttpNotFound();
            }
            return View("Details", anotherProfile);
        }

        // GET: Profiles/Edit/5
        public async Task<ActionResult> Edit(string email)
        {
            Profile myProfile = new Profile();
            IAsyncSession session = db._driver.AsyncSession();
            try
            {
                await session.ReadTransactionAsync(async tx =>
                {
                    var cursor =
                        await tx.RunAsync(
                            "match (p:Profile {Email: '" + User.Identity.Name + "'}) return p");

                    IRecord Record = await cursor.SingleAsync();
                    INode nodeProfile = (INode)Record.Values["p"];

                    myProfile.HouseNumber = nodeProfile.Properties.Values.First().As<string>();
                    myProfile.Email = nodeProfile.Properties.Values.Skip(1).First().As<string>();
                    myProfile.Rate = nodeProfile.Properties.Values.Skip(2).First().As<int>();
                    myProfile.FirstName = nodeProfile.Properties.Values.Skip(3).First().As<string>();
                    myProfile.Street = nodeProfile.Properties.Values.Skip(4).First().As<string>();
                    myProfile.PhoneNumber = nodeProfile.Properties.Values.Skip(5).First().As<string>();
                    myProfile.City = nodeProfile.Properties.Values.Skip(6).First().As<string>();
                    myProfile.Login = nodeProfile.Properties.Values.Skip(7).First().As<string>();
                    myProfile.LastName = nodeProfile.Properties.Values.Skip(8).First().As<string>();
                });

            }
            finally
            {
                await session.CloseAsync();
            }

            if (myProfile == null)
            {
                return HttpNotFound();
            }
            return View(myProfile);
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
