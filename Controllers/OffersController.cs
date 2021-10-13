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
using PagedList;
using Neo4j.Driver;
using System.Threading.Tasks;

namespace PROJEKT_PZ_NK_v3.Controllers
{
    [Authorize]
    public class OffersController : Controller
    {
        private OfferContext db = new OfferContext();

        // GET: Offers
        [Authorize]
        public async Task<ActionResult> Index(string sortOrder, string searchString, string searchSpecies, string searchRace, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.TitleSortParmAsc = String.IsNullOrEmpty(sortOrder) ? "Title_asc" : "";
            ViewBag.TitleSortParmDesc = String.IsNullOrEmpty(sortOrder) ? "Title_desc" : "";
            ViewBag.DateSortParmAsc = String.IsNullOrEmpty(sortOrder) ? "StartingDateAsc" : "";
            ViewBag.DateSortParmDesc = String.IsNullOrEmpty(sortOrder) ? "StartingDateDesc" : "";

            if (searchString != null)
            {
                page = 1;
            }
            ViewBag.CurrentFilterT = searchString;
            ViewBag.CurrentFilterS = searchSpecies;
            ViewBag.CurrentFilterR = searchRace;

            List<Offer> offers = new List<Offer>();
            IAsyncSession session = db._driver.AsyncSession();
            try
            {
                var cursorOffer = await session.RunAsync(
                    "match (p:Profile)-[rel:AUTHOR]->(o:Offer)<-[rel2:ANIMAL_OFFER]-(a:Animal) " +
                    "return o,p,a");
                List<IRecord> Records2 = await cursorOffer.ToListAsync();
                foreach (var item in Records2)
                {
                    INode nodeOffer = (INode)item.Values["o"];
                    INode nodeAnimal = (INode)item.Values["a"];
                    INode nodeProfile = (INode)item.Values["p"];

                    Profile profile = NodeToProfile(nodeProfile);
                    Animal animal = new Animal
                    {
                        DateOfBirth = nodeAnimal.Properties.Where(a => a.Key == "DateOfBirth").Select(a => a.Value).First().As<DateTime>(),
                        Description = nodeAnimal.Properties.Where(a => a.Key == "Description").Select(a => a.Value).First().As<string>(),
                        Race = nodeAnimal.Properties.Where(a => a.Key == "Race").Select(a => a.Value).First().As<string>(),
                        Gender = nodeAnimal.Properties.Where(a => a.Key == "Gender").Select(a => a.Value).First().As<string>(),
                        Image = nodeAnimal.Properties.Where(a => a.Key == "Image").Select(a => a.Value).First().As<string>(),
                        Species = nodeAnimal.Properties.Where(a => a.Key == "Species").Select(a => a.Value).First().As<string>(),
                        Weight = nodeAnimal.Properties.Where(a => a.Key == "Weight").Select(a => a.Value).First().As<string>(),
                        Name = nodeAnimal.Properties.Where(a => a.Key == "Name").Select(a => a.Value).First().As<string>()
                    };

                    Offer offer = new Offer
                    {
                        StartingDate = nodeOffer.Properties.Where(a => a.Key == "StartingDate").Select(a => a.Value).First().As<string>(),
                        Title = nodeOffer.Properties.Where(a => a.Key == "Title").Select(a => a.Value).First().As<string>(),
                        ID = ((int)nodeOffer.Id),
                        Description = nodeOffer.Properties.Where(a => a.Key == "Description").Select(a => a.Value).First().As<string>(),
                        EndDate = nodeOffer.Properties.Where(a => a.Key == "EndDate").Select(a => a.Value).First().As<string>(),
                        Profile = profile,
                        Animal = animal
                    };

                    offers.Add(offer);
                }
                await cursorOffer.ConsumeAsync();
            }
            finally
            {
                await session.CloseAsync();
            }


            offers = offers.Where(s => DateTime.Parse(s.StartingDate) > DateTime.Now).ToList();

            if (!String.IsNullOrEmpty(searchString))
            {
                offers = offers.Where(s => s.Title.ToLower().Contains(searchString.ToLower())).ToList();
            }
            if (!String.IsNullOrEmpty(searchSpecies))
            {
                //offers = offers.Where(s => s.Animal.Species.ToLower().Contains(searchSpecies.ToLower()));
            }
            if (!String.IsNullOrEmpty(searchRace))
            {
                //offers = offers.Where(s => s.Animal.Race.ToLower().Contains(searchRace.ToLower()));
            }
            switch (sortOrder)
            {
                case "StartingDateAsc":
                    offers = offers.OrderBy(s => DateTime.Parse(s.StartingDate).Year).ThenBy(s => DateTime.Parse(s.StartingDate).Month).ThenBy(s => DateTime.Parse(s.StartingDate).Day).ToList();
                    break;
                case "StartingDateDesc":
                    offers = offers.OrderByDescending(s => DateTime.Parse(s.StartingDate).Year).ThenByDescending(s => DateTime.Parse(s.StartingDate).Month).ThenByDescending(s => DateTime.Parse(s.StartingDate).Day).ToList();
                    break;
                case "Title_desc":
                    offers = offers.OrderByDescending(s => s.Title).ToList();
                    break;
                case "Title_asc":
                    offers = offers.OrderBy(s => s.Title).ToList();
                    break;
                default:
                    offers = offers.OrderBy(s => s.ID).ToList();
                    break;
            }
            int pageSize = 12;
            int pageNumber = (page ?? 1);
            ViewBag.SortList = new List<string>() { "tytuł malejąco", "tytuł rosnąco", "data malejąco", "data rosnąco" };
            return View(offers.ToPagedList(pageNumber, pageSize));
        }

        public async Task<ActionResult> MyOffers()
        {
            List<Offer> offers = new List<Offer>();
            IAsyncSession session = db._driver.AsyncSession();
            try
            {
                var cursorOffer = await session.RunAsync(
                    "match (p:Profile {Email: '"+  User.Identity.Name + "'})-[rel:AUTHOR]->(o:Offer)<-[rel2:ANIMAL_OFFER]-(a:Animal) " +
                    "return o,p,a");
                List<IRecord> Records2 = await cursorOffer.ToListAsync();
                foreach (var item in Records2)
                {
                    INode nodeOffer = (INode)item.Values["o"];
                    INode nodeAnimal = (INode)item.Values["a"];
                    INode nodeProfile = (INode)item.Values["p"];

                    Profile profile = NodeToProfile(nodeProfile);
                    Animal animal = new Animal
                    {
                        DateOfBirth = nodeAnimal.Properties.Where(a => a.Key == "DateOfBirth").Select(a => a.Value).First().As<DateTime>(),
                        Description = nodeAnimal.Properties.Where(a => a.Key == "Description").Select(a => a.Value).First().As<string>(),
                        Race = nodeAnimal.Properties.Where(a => a.Key == "Race").Select(a => a.Value).First().As<string>(),
                        Gender = nodeAnimal.Properties.Where(a => a.Key == "Gender").Select(a => a.Value).First().As<string>(),
                        Image = nodeAnimal.Properties.Where(a => a.Key == "Image").Select(a => a.Value).First().As<string>(),
                        Species = nodeAnimal.Properties.Where(a => a.Key == "Species").Select(a => a.Value).First().As<string>(),
                        Weight = nodeAnimal.Properties.Where(a => a.Key == "Weight").Select(a => a.Value).First().As<string>(),
                        Name = nodeAnimal.Properties.Where(a => a.Key == "Name").Select(a => a.Value).First().As<string>()
                    };

                    Offer offer = new Offer
                    {
                        StartingDate = nodeOffer.Properties.Where(a => a.Key == "StartingDate").Select(a => a.Value).First().As<string>(),
                        Title = nodeOffer.Properties.Where(a => a.Key == "Title").Select(a => a.Value).First().As<string>(),
                        ID = ((int)nodeOffer.Id),
                        Description = nodeOffer.Properties.Where(a => a.Key == "Description").Select(a => a.Value).First().As<string>(),
                        EndDate = nodeOffer.Properties.Where(a => a.Key == "EndDate").Select(a => a.Value).First().As<string>(),
                        Profile = profile,
                        Animal = animal
                    };

                    offers.Add(offer);
                }
                await cursorOffer.ConsumeAsync();
            }
            finally
            {
                await session.CloseAsync();
            }

            offers = offers.Where(s => DateTime.Parse(s.StartingDate) > DateTime.Now).ToList();
            return View(offers);
        }

        Profile NodeToProfile(INode nodeProfile)
        {
            Profile profile = new Profile
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
            return profile;
        }

        // GET: Offers/Details/5
        public async Task<ActionResult> Details(int offerID)
        {
            Offer offer;
            List<Applications> applications = new List<Applications>();
            IAsyncSession session = db._driver.AsyncSession();
            try
            {
                var cursorOffer = await session.RunAsync(
                    "match (p:Profile)-[rel:AUTHOR]->(o:Offer)<-[rel2:ANIMAL_OFFER]-(a:Animal) " +
                    "where id(o)=" +offerID+
                    " return o,p,a");
                IRecord Record = await cursorOffer.SingleAsync();
                INode nodeOffer = (INode)Record.Values["o"];
                INode nodeAnimal = (INode)Record.Values["a"];
                INode nodeProfile = (INode)Record.Values["p"];

                Profile profile = NodeToProfile(nodeProfile);
                Animal animal = new Animal
                {
                    DateOfBirth = nodeAnimal.Properties.Where(a => a.Key == "DateOfBirth").Select(a => a.Value).First().As<DateTime>(),
                    Description = nodeAnimal.Properties.Where(a => a.Key == "Description").Select(a => a.Value).First().As<string>(),
                    Race = nodeAnimal.Properties.Where(a => a.Key == "Race").Select(a => a.Value).First().As<string>(),
                    Gender = nodeAnimal.Properties.Where(a => a.Key == "Gender").Select(a => a.Value).First().As<string>(),
                    Image = nodeAnimal.Properties.Where(a => a.Key == "Image").Select(a => a.Value).First().As<string>(),
                    Species = nodeAnimal.Properties.Where(a => a.Key == "Species").Select(a => a.Value).First().As<string>(),
                    Weight = nodeAnimal.Properties.Where(a => a.Key == "Weight").Select(a => a.Value).First().As<string>(),
                    Name = nodeAnimal.Properties.Where(a => a.Key == "Name").Select(a => a.Value).First().As<string>()
                };


                offer = new Offer
                {
                    StartingDate = nodeOffer.Properties.Where(a => a.Key == "StartingDate").Select(a => a.Value).First().As<string>(),
                    Title = nodeOffer.Properties.Where(a => a.Key == "Title").Select(a => a.Value).First().As<string>(),
                    ID = ((int)nodeOffer.Id),
                    Description = nodeOffer.Properties.Where(a => a.Key == "Description").Select(a => a.Value).First().As<string>(),
                    EndDate = nodeOffer.Properties.Where(a => a.Key == "EndDate").Select(a => a.Value).First().As<string>(),
                    Profile = profile,
                    Animal = animal
                };
                await cursorOffer.ConsumeAsync();
            
                if (offer == null)
                {
                    return HttpNotFound();
                }

            
                await session.ReadTransactionAsync(async tx =>
                {
                    var cursor =
                        await tx.RunAsync(
                            "match (p:Profile)-[rel1:GUARDIAN]->(app:Application)<-[rel2:OWNER]-(p2:Profile)," +
                            "(app)-[rel3:NOTIFICATION_TO_THE_OFFER]->(o:Offer)," +
                            "(o:Offer)<-[rel4:ANIMAL_OFFER]-(a:Animal)  " +
                            "where id(o)="+ offerID +
                            " return a,p,app,p2,o");

                    List<IRecord> Records = await cursor.ToListAsync();
                    foreach (var item in Records)
                    {
                        INode nodeGuardian = (INode)item.Values["p"];
                        INode nodeOwner = (INode)item.Values["p2"];
                        INode nodeOffer2 = (INode)item.Values["o"];
                        INode nodeAnimal2 = (INode)item.Values["a"];
                        INode nodeApplication = (INode)item.Values["app"];

                        Profile guardian = NodeToProfile(nodeGuardian);
                        Profile owner = NodeToProfile(nodeOwner);

                        Animal animal2 = new Animal
                        {
                            DateOfBirth = nodeAnimal2.Properties.Where(a => a.Key == "DateOfBirth").Select(a => a.Value).First().As<DateTime>(),
                            Description = nodeAnimal2.Properties.Where(a => a.Key == "Description").Select(a => a.Value).First().As<string>(),
                            Race = nodeAnimal2.Properties.Where(a => a.Key == "Race").Select(a => a.Value).First().As<string>(),
                            Gender = nodeAnimal2.Properties.Where(a => a.Key == "Gender").Select(a => a.Value).First().As<string>(),
                            Image = nodeAnimal2.Properties.Where(a => a.Key == "Image").Select(a => a.Value).First().As<string>(),
                            Species = nodeAnimal2.Properties.Where(a => a.Key == "Species").Select(a => a.Value).First().As<string>(),
                            Weight = nodeAnimal2.Properties.Where(a => a.Key == "Weight").Select(a => a.Value).First().As<string>(),
                            Name = nodeAnimal2.Properties.Where(a => a.Key == "Name").Select(a => a.Value).First().As<string>()
                        };

                        Offer offer2 = new Offer
                        {
                            StartingDate = nodeOffer2.Properties.Where(a => a.Key == "StartingDate").Select(a => a.Value).First().As<string>(),
                            Title = nodeOffer2.Properties.Where(a => a.Key == "Title").Select(a => a.Value).First().As<string>(),
                            ID = ((int)nodeOffer2.Id),
                            Description = nodeOffer2.Properties.Where(a => a.Key == "Description").Select(a => a.Value).First().As<string>(),
                            EndDate = nodeOffer2.Properties.Where(a => a.Key == "EndDate").Select(a => a.Value).First().As<string>(),
                            Animal = animal2,
                            Profile = owner
                        };

                        Applications application = new Applications
                        {
                            Message = nodeApplication.Properties.Where(a => a.Key == "Message").Select(a => a.Value).First().As<string>(),
                            Status = nodeApplication.Properties.Where(a => a.Key == "Status").Select(a => a.Value).First().As<string>(),
                            Guardian = guardian,
                            Owner = owner,
                            Offer = offer2
                        };

                        applications.Add(application);
                    }

                });

            }
            finally
            {
                await session.CloseAsync();
            }

            ViewBag.IsApplied = applications.Any(app => app.Guardian.Email == User.Identity.Name);
            ViewBag.Applications = applications
                .Where(a => a.Owner.Email == User.Identity.Name
                && DateTime.Parse(a.Offer.StartingDate) > DateTime.Now
                && a.Status != "Właściciel odrzucił zgłoszenie"
                && a.Status != "Odrzucone"
                && !a.Status.Contains("Usunieta"));
            return View(offer);
        }

        // GET: Offers/Create
        public async Task<ActionResult> Create()
        {
            List<Animal> animals = new List<Animal>();
            IAsyncSession session = db._driver.AsyncSession();
            try
            {
                await session.ReadTransactionAsync(async tx =>
                {
                    var cursor =
                        await tx.RunAsync(
                            "match(p:Profile {Email: '"+ User.Identity.Name +"'})-->(a:Animal) " +
                            "return a");

                    List<IRecord> Records = await cursor.ToListAsync();
                    foreach (var item in Records)
                    {
                        INode nodeAnimal = (INode)item.Values["a"];
                        Animal animal = new Animal
                        {
                            DateOfBirth = nodeAnimal.Properties.Where(a => a.Key == "DateOfBirth").Select(a => a.Value).First().As<DateTime>(),
                            Description = nodeAnimal.Properties.Where(a => a.Key == "Description").Select(a => a.Value).First().As<string>(),
                            Race = nodeAnimal.Properties.Where(a => a.Key == "Race").Select(a => a.Value).First().As<string>(),
                            Gender = nodeAnimal.Properties.Where(a => a.Key == "Gender").Select(a => a.Value).First().As<string>(),
                            Image = nodeAnimal.Properties.Where(a => a.Key == "Image").Select(a => a.Value).First().As<string>(),
                            Species = nodeAnimal.Properties.Where(a => a.Key == "Species").Select(a => a.Value).First().As<string>(),
                            Weight = nodeAnimal.Properties.Where(a => a.Key == "Weight").Select(a => a.Value).First().As<string>(),
                            Name = nodeAnimal.Properties.Where(a => a.Key == "Name").Select(a => a.Value).First().As<string>()
                        };

                        animals.Add(animal);
                    }

                });

            }
            finally
            {
                await session.CloseAsync();
            }
            ViewBag.Animals = animals.ToList();
            ViewBag.AnimalsCount = animals.Count();
            return View();
        }

        // POST: Offers/Create
        // Aby zapewnić ochronę przed atakami polegającymi na przesyłaniu dodatkowych danych, włącz określone właściwości, z którymi chcesz utworzyć powiązania.
        // Aby uzyskać więcej szczegółów, zobacz https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Offer offer, string name)
        {
            if (ModelState.IsValid)
            {
                IAsyncSession session = db._driver.AsyncSession();
                try
                {
                    IResultCursor cursor = await session.RunAsync(
                        "MATCH (n:Profile { Email:'" + User.Identity.Name + "'})-[rel:OWNER]->(a:Animal {Name: '" + offer.AnimalName + "'}) " +
                        "CREATE(n) -[r:AUTHOR]-> (p:Offer " +
                        "{ Title: '" + offer.Title +
                        "', Description: '" + offer.Description +
                        "', StartingDate: '" + DateTime.Parse(offer.StartingDate).ToString("yyyy-MM-dd HH:mm") +
                        "', EndDate: '" + DateTime.Parse(offer.EndDate).ToString("yyyy-MM-dd HH:mm") + "'})," +
                        "(a) -[t:ANIMAL_OFFER]-> (p)"
                    );
                    await cursor.ConsumeAsync();
                }
                finally
                {
                    await session.CloseAsync();
                }
                return RedirectToAction("MyOffers");
            }

            return View(offer);
        }

        // GET: Offers/Edit/5
        public async Task<ActionResult> Edit(int? offerID)
        {
            Offer offer = new Offer();
            if (offerID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            IAsyncSession session = db._driver.AsyncSession();
            try
            {
                var cursorOffer = await session.RunAsync(
                    "match (p:Profile)-[rel:AUTHOR]->(o:Offer)<-[rel2:ANIMAL_OFFER]-(a:Animal) " +
                    "where id(o)=" + offerID +
                    " return o,p,a");
                List<IRecord> Records = await cursorOffer.ToListAsync();
                foreach (var Record in Records)
                {
                    INode nodeOffer = (INode)Record.Values["o"];
                    INode nodeAnimal = (INode)Record.Values["a"];
                    INode nodeProfile = (INode)Record.Values["p"];

                    Profile profile = NodeToProfile(nodeProfile);
                    Animal animal = new Animal
                    {
                        DateOfBirth = nodeAnimal.Properties.Where(a => a.Key == "DateOfBirth").Select(a => a.Value).First().As<DateTime>(),
                        Description = nodeAnimal.Properties.Where(a => a.Key == "Description").Select(a => a.Value).First().As<string>(),
                        Race = nodeAnimal.Properties.Where(a => a.Key == "Race").Select(a => a.Value).First().As<string>(),
                        Gender = nodeAnimal.Properties.Where(a => a.Key == "Gender").Select(a => a.Value).First().As<string>(),
                        Image = nodeAnimal.Properties.Where(a => a.Key == "Image").Select(a => a.Value).First().As<string>(),
                        Species = nodeAnimal.Properties.Where(a => a.Key == "Species").Select(a => a.Value).First().As<string>(),
                        Weight = nodeAnimal.Properties.Where(a => a.Key == "Weight").Select(a => a.Value).First().As<string>(),
                        Name = nodeAnimal.Properties.Where(a => a.Key == "Name").Select(a => a.Value).First().As<string>()
                    };

                    offer.StartingDate = nodeOffer.Properties.Where(a => a.Key == "StartingDate").Select(a => a.Value).First().As<string>();
                    offer.Title = nodeOffer.Properties.Where(a => a.Key == "Title").Select(a => a.Value).First().As<string>();
                    offer.ID = ((int)nodeOffer.Id);
                    offer.Description = nodeOffer.Properties.Where(a => a.Key == "Description").Select(a => a.Value).First().As<string>();
                    offer.EndDate = nodeOffer.Properties.Where(a => a.Key == "EndDate").Select(a => a.Value).First().As<string>();
                    offer.Profile = profile;
                    offer.Animal = animal;
                }
                
                await cursorOffer.ConsumeAsync();
            }
            finally
            {
                await session.CloseAsync();
            }
            if (offer == null)
            {
                return HttpNotFound();
            }
            return View(offer);
        }

        // POST: Offers/Edit/5
        // Aby zapewnić ochronę przed atakami polegającymi na przesyłaniu dodatkowych danych, włącz określone właściwości, z którymi chcesz utworzyć powiązania.
        // Aby uzyskać więcej szczegółów, zobacz https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Offer offer)
        {
            if (ModelState.IsValid)
            {
                IAsyncSession session = db._driver.AsyncSession();
                try
                {
                    IResultCursor cursor = await session.RunAsync(
                        "MATCH (o:Offer { OfferID:" + offer.ID + "}) " +
                        "SET o.Title = '" + offer.Title +
                        "', o.Description = '" + offer.Description +
                        "', o.StartingDate = '" + DateTime.Parse(offer.StartingDate).ToString("yyyy-MM-dd HH:mm") +
                        "', o.EndDate = '" + DateTime.Parse(offer.EndDate).ToString("yyyy-MM-dd HH:mm") + "'"
                    );
                    await cursor.ConsumeAsync();
                }
                finally
                {
                    await session.CloseAsync();
                }
                return RedirectToAction("MyOffers");
            }
            return View(offer);
        }

        // GET: Offers/Delete/5
        public async Task<ActionResult> Delete(int offerID)
        {
            Offer offer = new Offer();
            IAsyncSession session = db._driver.AsyncSession();
            try
            {
                var cursorOffer = await session.RunAsync(
                    "match (p:Profile)-[rel:AUTHOR]->(o:Offer)<-[rel2:ANIMAL_OFFER]-(a:Animal) " +
                    "where id(o)= " + offerID +
                    " return o,p,a");
                List<IRecord> Records = await cursorOffer.ToListAsync();
                foreach (var item in Records)
                {
                    INode nodeOffer = (INode)item.Values["o"];
                    INode nodeAnimal = (INode)item.Values["a"];
                    INode nodeProfile = (INode)item.Values["p"];

                    Profile profile = NodeToProfile(nodeProfile);
                    Animal animal = new Animal
                    {
                        DateOfBirth = nodeAnimal.Properties.Where(a => a.Key == "DateOfBirth").Select(a => a.Value).First().As<DateTime>(),
                        Description = nodeAnimal.Properties.Where(a => a.Key == "Description").Select(a => a.Value).First().As<string>(),
                        Race = nodeAnimal.Properties.Where(a => a.Key == "Race").Select(a => a.Value).First().As<string>(),
                        Gender = nodeAnimal.Properties.Where(a => a.Key == "Gender").Select(a => a.Value).First().As<string>(),
                        Image = nodeAnimal.Properties.Where(a => a.Key == "Image").Select(a => a.Value).First().As<string>(),
                        Species = nodeAnimal.Properties.Where(a => a.Key == "Species").Select(a => a.Value).First().As<string>(),
                        Weight = nodeAnimal.Properties.Where(a => a.Key == "Weight").Select(a => a.Value).First().As<string>(),
                        Name = nodeAnimal.Properties.Where(a => a.Key == "Name").Select(a => a.Value).First().As<string>()
                    };

                    offer = new Offer
                    {
                        StartingDate = nodeOffer.Properties.Where(a => a.Key == "StartingDate").Select(a => a.Value).First().As<string>(),
                        Title = nodeOffer.Properties.Where(a => a.Key == "Title").Select(a => a.Value).First().As<string>(),
                        ID = ((int)nodeOffer.Id),
                        Description = nodeOffer.Properties.Where(a => a.Key == "Description").Select(a => a.Value).First().As<string>(),
                        EndDate = nodeOffer.Properties.Where(a => a.Key == "EndDate").Select(a => a.Value).First().As<string>(),
                        Profile = profile,
                        Animal = animal
                    };
                    break;
                }
                await cursorOffer.ConsumeAsync();
            }
            finally
            {
                await session.CloseAsync();
            }
            if (offer == null)
            {
                return HttpNotFound();
            }
            return View(offer);
        }

        // POST: Offers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int offerID)
        {
            IAsyncSession session = db._driver.AsyncSession();
            try
            {
                IResultCursor cursor = await session.RunAsync(
                    "match (app:Application)-[rel:NOTIFICATION_TO_THE_OFFER]->(o:Offer) " +
                    "where id(o)= " + offerID +
                    " detach delete app,o");
                await cursor.ConsumeAsync();

                IResultCursor cursor2 = await session.RunAsync(
                    "match (o:Offer) " +
                    "where id(o)= " + offerID +
                    " detach delete o");
                await cursor.ConsumeAsync();
            }
            finally
            {
                await session.CloseAsync();
            }
            return RedirectToAction("MyOffers");
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
