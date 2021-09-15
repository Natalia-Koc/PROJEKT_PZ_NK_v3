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
                var cursorProfil =
                        await session.RunAsync(
                            "match (p:Profile {Email: '"+ User.Identity.Name +"'}) return p");
                if(cursorProfil.FetchAsync().IsCompleted)
                {
                    IRecord Record = await cursorProfil.SingleAsync();
                    INode nodeProfil = (INode)Record.Values["p"];
                    ViewBag.Profil = NodeToProfile(nodeProfil);
                }
                else
                {
                    ViewBag.Profil = null;
                }
                await cursorProfil.ConsumeAsync();

                List<Offer> offers = new List<Offer>();
                var cursorOffers =
                        await session.RunAsync(
                            "match (p:Profile)-[rel:AUTHOR]->(o:Offer)," +
                            "(c:Comment)-->(p) where c.rate>2 OR c.rate=0 " +
                            "return o");

                List<IRecord> Records = await cursorOffers.ToListAsync();
                foreach (var item in Records)
                {
                    INode nodeOffer = (INode)item.Values["o"];

                    Offer offer = new Offer
                    {
                        StartingDate = nodeOffer.Properties.Values.First().As<string>(),
                        Title = nodeOffer.Properties.Values.Skip(1).First().As<string>(),
                        ID = ((int)nodeOffer.Id),
                        Description = nodeOffer.Properties.Values.Skip(2).First().As<string>(),
                        EndDate = nodeOffer.Properties.Values.Skip(3).First().As<string>(),
                    };
                    offers.Add(offer);
                }
                await cursorOffers.ConsumeAsync();

                var cursorApp =
                        await session.RunAsync(
                            "match (p:Profile {Email: 'Nowy-71@o2.pl'})-[rel:GUARDIAN]->(app:Application) " +
                            "return app");

                List<Applications> applications = new List<Applications>();
                List<IRecord> Records2 = await cursorApp.ToListAsync();
                foreach (var item in Records2)
                {
                    INode nodeApplication = (INode)item.Values["app"];

                    Applications application = new Applications
                    {
                        Message = nodeApplication.Properties.Values.First().As<string>(),
                        Status = nodeApplication.Properties.Values.Skip(1).First().As<string>()
                    };

                    applications.Add(application);
                }
                await cursorApp.ConsumeAsync();

                if (applications.Count > 0)
                {

                    int hours = applications.Where(a => a.Guardian.Email == User.Identity.Name && DbFunctions.DiffDays(DateTime.Parse(a.Offer.EndDate), DateTime.Parse(a.Offer.StartingDate)) == 0).Count();
                    int days = applications.Where(a => a.Guardian.Email == User.Identity.Name && DbFunctions.DiffDays(DateTime.Parse(a.Offer.EndDate), DateTime.Parse(a.Offer.StartingDate)) > 0).Count();
                    if (hours > days)
                    {
                        ViewBag.Offers1 = offers
                            .OrderByDescending(a => DbFunctions.DiffDays(DateTime.Parse(a.EndDate), DateTime.Parse(a.StartingDate)))
                        .Take(4).ToList();

                        //.ThenByDescending(a => a.Profile.Rate)


                        ViewBag.Offers2 = offers
                            .OrderByDescending(a => DbFunctions.DiffDays(DateTime.Parse(a.EndDate), DateTime.Parse(a.StartingDate)))
                        .Skip(4).Take(4).ToList();

                        //.ThenByDescending(a => a.Profile.Rate)
                    }
                    else
                    {
                        ViewBag.Offers1 = offers
                            .OrderBy(a => DbFunctions.DiffDays(DateTime.Parse(a.EndDate), DateTime.Parse(a.StartingDate)))
                        .Take(4).ToList();

                        //.ThenByDescending(a => a.Profile.Rate)


                        ViewBag.Offers2 = offers
                            .OrderBy(a => DbFunctions.DiffDays(DateTime.Parse(a.EndDate), DateTime.Parse(a.StartingDate)))
                        .Skip(4).Take(4).ToList();

                        //.ThenByDescending(a => a.Profile.Rate)
                    }
                }
                else
                {
                    ViewBag.Offers1 = offers
                        .Take(4).ToList();

                    //.OrderByDescending(a => a.Profile.Rate)

                    ViewBag.Offers2 = offers
                        .Skip(4).Take(4).ToList();
                    //.OrderByDescending(a => a.Profile.Rate)
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