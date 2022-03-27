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
            if (User.Identity.IsAuthenticated)
            {
                Models.Profile profile = db.Profiles.FirstOrDefault(p => p.Email == User.Identity.Name);
                ViewBag.Profil = profile;

                if (profile != null)
                {
                    var offers = db.Offers
                    .Where(o => o.Profile.Email != User.Identity.Name
                        && o.EndDate > DateTime.Now
                        && !o.Profile.SavedProfiles
                            .Any(sp => sp.MyProfile.Email == User.Identity.Name
                                && sp.SavedProfile.Email == o.Profile.Email
                                && sp.SavedAs == Saved.blocked)
                        && !o.Profile.MySavedProfiles
                            .Any(sp => sp.SavedProfile.Email == User.Identity.Name
                                && sp.MyProfile.Email == o.Profile.Email
                                && sp.SavedAs == Saved.blocked)
                        && o.Profile.Comments.Where(b => (b.Author.Email == User.Identity.Name
                            || b.Profile.Email == User.Identity.Name)
                            && (b.Grade > 2 || b.Grade == 0)).Count() >= 0)
                    .ToList();

                    var offers2 = offers
                        .OrderBy(a => CalculateDistance(a.Profile.City + " " + a.Profile.Street, profile.City + " " + profile.Street));

                    ViewBag.Offers1 = offers2
                        .Take(4).ToList();

                    ViewBag.Offers2 = offers2
                        .Skip(4).Take(4).ToList();
                }

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