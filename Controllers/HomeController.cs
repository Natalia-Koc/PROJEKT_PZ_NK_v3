using PROJEKT_PZ_NK_v3.DAL;
using PROJEKT_PZ_NK_v3.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using PROJEKT_PZ_NK_v3.ViewModels;
using Xamarin.Forms.Maps;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.Entity;
using Neo4j.Driver;

namespace PROJEKT_PZ_NK_v3.Controllers
{
    public class HomeController : Controller
    {
        OfferContext db = new OfferContext();
        public async Task<ActionResult> Index()
        {
            IAsyncSession session = db._driver.AsyncSession();
            try
            {

                List<Offer> offers = new List<Offer>();
                var cursorOffers =
                        await session.RunAsync(
                            "match (p:Profile)-[rel:AUTHOR]->(o:Offer)<-[rell:ANIMAL_OFFER]-(a:Animal) " +
                            "return p,o,a");

                List<IRecord> Records = await cursorOffers.ToListAsync();
                foreach (var item in Records)
                {
                    INode nodeOffer = (INode)item.Values["o"];
                    INode nodeProfile = (INode)item.Values["p"];
                    INode nodeAnimal = (INode)item.Values["a"];

                    Animal animal = new Animal
                    {
                        ID = nodeAnimal.Id.As<int>(),
                        DateOfBirth = nodeAnimal.Properties.Where(a => a.Key == "DateOfBirth").Select(a => a.Value).First().As<DateTime>(),
                        Description = nodeAnimal.Properties.Where(a => a.Key == "Description").Select(a => a.Value).First().As<string>(),
                        Race = nodeAnimal.Properties.Where(a => a.Key == "Race").Select(a => a.Value).First().As<string>(),
                        Gender = nodeAnimal.Properties.Where(a => a.Key == "Gender").Select(a => a.Value).First().As<string>(),
                        Image = nodeAnimal.Properties.Where(a => a.Key == "Image").Select(a => a.Value).First().As<string>(),
                        Species = nodeAnimal.Properties.Where(a => a.Key == "Species").Select(a => a.Value).First().As<string>(),
                        Weight = nodeAnimal.Properties.Where(a => a.Key == "Weight").Select(a => a.Value).First().As<string>(),
                        Name = nodeAnimal.Properties.Where(a => a.Key == "Name").Select(a => a.Value).First().As<string>()
                    };

                    Profile owner = new Profile
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
                    Offer offer = new Offer
                    {
                        Profile = owner,
                        Animal = animal,
                        StartingDate = nodeOffer.Properties.Where(a => a.Key == "StartingDate").Select(a => a.Value).First().As<string>(),
                        Title = nodeOffer.Properties.Where(a => a.Key == "Title").Select(a => a.Value).First().As<string>(),
                        ID = ((int)nodeOffer.Id),
                        Description = nodeOffer.Properties.Where(a => a.Key == "Description").Select(a => a.Value).First().As<string>(),
                        EndDate = nodeOffer.Properties.Where(a => a.Key == "EndDate").Select(a => a.Value).First().As<string>()
                    };
                    offers.Add(offer);
                }
                await cursorOffers.ConsumeAsync();


                var cursorApp =
                        await session.RunAsync(
                            "match (p:Profile {Email: '"+ User.Identity.Name + "'})-[rel:GUARDIAN]->(app:Application)<-[rel2:OWNER]-(p2:Profile)," +
                            "(app)-[rel3:NOTIFICATION_TO_THE_OFFER]->(o:Offer) " +
                            "return app,p,p2,o");

                List<Applications> applications = new List<Applications>();
                List<IRecord> Records2 = await cursorApp.ToListAsync();
                foreach (var item in Records2)
                {
                    INode nodeApplication = (INode)item.Values["app"];
                    INode nodeProfile = (INode)item.Values["p"];
                    INode nodeProfile2 = (INode)item.Values["p2"];
                    INode nodeOffer = (INode)item.Values["o"];

                    Profile guardian = new Profile
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

                    Profile owner = new Profile
                    {
                        ID = ((int)nodeProfile2.Id),
                        HouseNumber = nodeProfile2.Properties.Where(a => a.Key == "HouseNumber").Select(a => a.Value).First().As<string>(),
                        Email = nodeProfile2.Properties.Where(a => a.Key == "Email").Select(a => a.Value).First().As<string>(),
                        Rate = nodeProfile2.Properties.Where(a => a.Key == "Rate").Select(a => a.Value).First().As<int>(),
                        FirstName = nodeProfile2.Properties.Where(a => a.Key == "FirstName").Select(a => a.Value).First().As<string>(),
                        Street = nodeProfile2.Properties.Where(a => a.Key == "Street").Select(a => a.Value).First().As<string>(),
                        PhoneNumber = nodeProfile2.Properties.Where(a => a.Key == "PhoneNumber").Select(a => a.Value).First().As<string>(),
                        City = nodeProfile2.Properties.Where(a => a.Key == "City").Select(a => a.Value).First().As<string>(),
                        Login = nodeProfile2.Properties.Where(a => a.Key == "Login").Select(a => a.Value).First().As<string>(),
                        LastName = nodeProfile2.Properties.Where(a => a.Key == "LastName").Select(a => a.Value).First().As<string>()
                    };
                    Offer offer = new Offer
                    {
                        Profile = owner,
                        StartingDate = nodeOffer.Properties.Where(a => a.Key == "StartingDate").Select(a => a.Value).First().As<string>(),
                        Title = nodeOffer.Properties.Where(a => a.Key == "Title").Select(a => a.Value).First().As<string>(),
                        ID = ((int)nodeOffer.Id),
                        Description = nodeOffer.Properties.Where(a => a.Key == "Description").Select(a => a.Value).First().As<string>(),
                        EndDate = nodeOffer.Properties.Where(a => a.Key == "EndDate").Select(a => a.Value).First().As<string>()
                    };

                    Applications application = new Applications
                    {
                        Message = nodeApplication.Properties.Where(a => a.Key == "Message").Select(a => a.Value).First().As<string>(),
                        Status = nodeApplication.Properties.Where(a => a.Key == "Status").Select(a => a.Value).First().As<string>(),
                        Guardian = guardian,
                        Owner = owner,
                        Offer = offer,
                        ID = ((int)nodeApplication.Id)
                    };

                    applications.Add(application);
                }
                await cursorApp.ConsumeAsync();

                if (applications.Count > 0)
                {

                    int hours = applications.Where(a => (DateTime.Parse(a.Offer.EndDate) - DateTime.Parse(a.Offer.StartingDate)).TotalDays == 0).Count();
                    int days = applications.Where(a => (DateTime.Parse(a.Offer.EndDate) - DateTime.Parse(a.Offer.StartingDate)).TotalDays > 0).Count();
                    if (hours > days)
                    {
                        ViewBag.Offers1 = offers
                            .OrderByDescending(a => (DateTime.Parse(a.EndDate) - DateTime.Parse(a.StartingDate)).TotalDays)
                            .ThenByDescending(a => a.Profile.Rate)
                        .Take(4).ToList();

                        ViewBag.Offers2 = offers
                            .OrderByDescending(a => (DateTime.Parse(a.EndDate) - DateTime.Parse(a.StartingDate)).TotalDays)
                            .ThenByDescending(a => a.Profile.Rate)
                        .Skip(4).Take(4).ToList();

                    }
                    else
                    {
                        ViewBag.Offers1 = offers
                            .OrderBy(a => (DateTime.Parse(a.EndDate) - DateTime.Parse(a.StartingDate)).TotalDays)
                            .ThenByDescending(a => a.Profile.Rate)
                            .Take(4).ToList();
                        ViewBag.Offers2 = offers
                                .OrderBy(a => (DateTime.Parse(a.EndDate) - DateTime.Parse(a.StartingDate)).TotalDays)
                                .ThenByDescending(a => a.Profile.Rate)
                                .Skip(4).Take(4).ToList();
                    }
                }
                else
                {
                    ViewBag.Offers1 = offers
                            .OrderByDescending(a => a.Profile.Rate)
                        .Take(4).ToList();
                    ViewBag.Offers2 = offers
                        .OrderByDescending(a => a.Profile.Rate)
                        .Skip(4).Take(4).ToList();
                }
            }
            finally
            {
                await session.CloseAsync();
            }
            return View();
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

        /*
        private async Task<int> MyDisctance(Profile profile)
        {
            Geocoder geoCoder = new Geocoder();

            Profile myProfile = db.Profiles.Single(p => p.Email == User.Identity.Name);
            String myAddress = myProfile.City + ", " + myProfile.Street + " " + myProfile.HouseNumber;
            Position myPosition = geoCoder.GetPositionsForAddressAsync(myAddress).Result.FirstOrDefault();

            String secondAddress = profile.City + ", " + profile.Street + " " + profile.HouseNumber;
            IEnumerable<Position> secondApproximateLocations = await geoCoder.GetPositionsForAddressAsync(secondAddress);
            Position secondPosition = secondApproximateLocations.FirstOrDefault();

            Distance distance = Distance.BetweenPositions(myPosition, secondPosition);
            return ((int)distance.Meters);
        }*/

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}