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
using System.Net;
using System.IO;
using System.Data;
using Google.Protobuf.WellKnownTypes;
using System.Text;
using Nest;
using MathNet.Numerics;

namespace PROJEKT_PZ_NK_v3.Controllers
{
    public class HomeController : Controller
    {
        OfferContext db = new OfferContext();



        public ActionResult Index()
        {

            Models.Profile profile = db.Profiles.FirstOrDefault(p => p.Email == User.Identity.Name);
            ViewBag.Profil = profile;

            var offers = db.Offers
                .Where(a => a.Profile.Email != User.Identity.Name
                    && a.EndDate > DateTime.Now
                    && a.Profile.Comments
                    .Where(b => (b.Author.Email == User.Identity.Name || b.Profile.Email == User.Identity.Name)
                        && (b.Grade > 2 || b.Grade == 0))
                .Count() >= 0);

            //offers.OrderBy(a => CalculateDistance(a.Profile.City + " " + a.Profile.Street, profile.City + " " + profile.Street)).ToList();

            if (db.Applications.Any(a => a.Guardian.Email == User.Identity.Name))
            {
                int hours = db.Applications.Where(a => a.Guardian.Email == User.Identity.Name && DbFunctions.DiffDays(a.Offer.EndDate, a.Offer.StartingDate) == 0).Count();
                int days = db.Applications.Where(a => a.Guardian.Email == User.Identity.Name && DbFunctions.DiffDays(a.Offer.EndDate, a.Offer.StartingDate) > 0).Count();
                if (hours > days)
                {
                    ViewBag.Offers1 = offers.ToList()
                        .OrderByDescending(a => CalculateDistance(a.Profile.City + " " + a.Profile.Street, profile.City + " " + profile.Street))
                        /*.ThenByDescending(a => a.EndDate.Subtract(a.StartingDate))
                        .ThenByDescending(a => a.Profile.Rate)*/
                    .Take(4).ToList();


                    ViewBag.Offers2 = offers
                        .OrderByDescending(a => DbFunctions.DiffDays(a.EndDate, a.StartingDate))
                        .ThenByDescending(a => a.Profile.Rate)
                    .Skip(4).Take(4).ToList();
                }
                else
                {
                    ViewBag.Offers1 = offers
                        .OrderByDescending(a => CalculateDistance(a.Profile.City + " " + a.Profile.Street, profile.City + " " + profile.Street))
                        /*.ThenBy(a => a.EndDate.Subtract(a.StartingDate))
                        .ThenByDescending(a => a.Profile.Rate)*/
                    .Take(4).ToList();
                    ViewBag.Offers2 = offers
                        .OrderBy(a => DbFunctions.DiffDays(a.EndDate, a.StartingDate))
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

            ViewBag.StatisticsAnimals = db.Animals.Count();
            ViewBag.StatisticsOffers = db.Offers.Count();
            ViewBag.StatisticsUsers = db.Profiles.Count();

            return View();
        }

        /*private async Task<int> MyDisctance(Profile profile)
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

        public ActionResult About()
        {
            List<Models.Profile> data = db.Profiles.ToList();


            return View();
        }

        float CalculateDistance(string source, string destination)
        {
            string distance;
            string url = "https://maps.googleapis.com/maps/api/distancematrix/xml?origins=" + source + "&destinations=" + destination + "&key=AIzaSyA1BOzf3325XV08x9aMj_kELckTcgL3xrQ";
            WebRequest request = WebRequest.Create(url);
            using (WebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    DataSet dsResult = new DataSet();
                    dsResult.ReadXml(reader);
                    distance = dsResult.Tables["distance"].Rows[0]["value"].ToString();
                }
            }
            return float.Parse(distance);
        }


        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}