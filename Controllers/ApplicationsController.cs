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
using PROJEKT_PZ_NK_v3.DAL;
using PROJEKT_PZ_NK_v3.Models;

namespace PROJEKT_PZ_NK_v3.Controllers
{
    [Authorize]
    public class ApplicationsController : Controller
    {
        private OfferContext db = new OfferContext();

        public async Task<ActionResult> MyApplications()
        {
            List<Applications> applications = new List<Applications>();
            IAsyncSession session = db._driver.AsyncSession();
            try
            {
                await session.ReadTransactionAsync(async tx =>
                {
                    var cursor =
                        await tx.RunAsync(
                            "match (p:Profile {Email: '"+ User.Identity.Name + "'})-[rel1:GUARDIAN]->(app:Application)<-[rel2:OWNER]-(p2:Profile)," +
                            "(app)-[rel3:NOTIFICATION_TO_THE_OFFER]->(o:Offer) " +
                            "where app.Status = 'Zaakceptowane' OR app.Status = 'Oczekuje na akceptacje' OR app.Status = 'Właściciel odrzucił ofertę'" +
                            "return p,app,p2,o");

                    List<IRecord> Records = await cursor.ToListAsync();
                    foreach (var item in Records)
                    {
                        INode nodeGuardian = (INode)item.Values["p"];
                        INode nodeOwner = (INode)item.Values["p2"];
                        INode nodeOffer = (INode)item.Values["o"];
                        INode nodeApplication = (INode)item.Values["app"];

                        Profile guardian = NodeToProfile(nodeGuardian);
                        Profile owner = NodeToProfile(nodeOwner);

                        Offer offer = new Offer
                        {
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
                            Offer = offer
                        };

                        applications.Add(application);
                    }

                });

            }
            finally
            {
                await session.CloseAsync();
            }

            return View(applications);
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

        public async Task<ActionResult> ApplicationsToMyOffers()
        {
            List<Applications> applications = new List<Applications>();
            IAsyncSession session = db._driver.AsyncSession();
            try
            {
                await session.ReadTransactionAsync(async tx =>
                {
                    var cursor =
                        await tx.RunAsync(
                            "match (p:Profile)-[rel1:GUARDIAN]->(app:Application)<-[rel2:OWNER]-(p2:Profile {Email: '" + User.Identity.Name + "'})," +
                            "(app)-[rel3:NOTIFICATION_TO_THE_OFFER]->(o:Offer) " +
                            "where app.Status = 'Zaakceptowane' OR app.Status = 'Oczekuje na akceptacje' OR app.Status = 'Opiekun zrezygnował z oferty'" +
                            "return p,app,p2,o");

                    List<IRecord> Records = await cursor.ToListAsync();

                    foreach (var item in Records)
                    {
                        INode nodeGuardian = (INode)item.Values["p"];
                        INode nodeOwner = (INode)item.Values["p2"];
                        INode nodeOffer = (INode)item.Values["o"];
                        INode nodeApplication = (INode)item.Values["app"];

                        Profile guardian = NodeToProfile(nodeGuardian);
                        Profile owner = NodeToProfile(nodeOwner);

                        Offer offer = new Offer
                        {
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
                            Offer = offer
                        };

                        applications.Add(application);
                    }

                });

            }
            finally
            {
                await session.CloseAsync();
            }
            return View(applications);
        }

        public async Task<ActionResult> History()
        {
            List<Applications> applications = new List<Applications>();
            IAsyncSession session = db._driver.AsyncSession();
            try
            {
                await session.ReadTransactionAsync(async tx =>
                {
                    var cursor =
                        await tx.RunAsync(
                            "match (p:Profile)-[rel1:GUARDIAN]->(app:Application)<-[rel2:OWNER]-(p2:Profile)," +
                            "(app)-[rel3:NOTIFICATION_TO_THE_OFFER]->(o:Offer) " +
                            "where (p.Email='"+ User.Identity.Name + "' AND app.Status='Opiekun zrezygnował z oferty')  " +
                            "OR (p2.Email='" + User.Identity.Name + "' AND app.Status='Właściciel odrzucił ofertę')" +
                            "OR ((p.Email='" + User.Identity.Name + "' OR p2.Email='" + User.Identity.Name + "') AND " +
                            "app.Status <> 'Zaakceptowane' AND app.Status <> 'Oczekuje na akceptacje' " +
                            "AND app.Status <> 'Właściciel odrzucił ofertę' AND app.Status <> 'Opiekun zrezygnował z oferty')" +
                            "return p,app,p2,o");

                    List<IRecord> Records = await cursor.ToListAsync();
                    foreach (var item in Records)
                    {
                        INode nodeGuardian = (INode)item.Values["p"];
                        INode nodeOwner = (INode)item.Values["p2"];
                        INode nodeOffer = (INode)item.Values["o"];
                        INode nodeApplication = (INode)item.Values["app"];

                        Profile guardian = NodeToProfile(nodeGuardian);
                        Profile owner = NodeToProfile(nodeOwner);

                        Offer offer = new Offer
                        {
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
                            Offer = offer
                        };

                        applications.Add(application);
                    }

                });

            }
            finally
            {
                await session.CloseAsync();
            }
            return View(applications);
        }

        // GET: Applications/Create
        public async Task<ActionResult> Create(int offerID, string ownerEmail, Applications application)
        {

            IAsyncSession session = db._driver.AsyncSession();
            try
            {
                IResultCursor cursor = await session.RunAsync("" +
                    "MATCH (n:Profile {Email: '" + User.Identity.Name + "'}), " +
                        "(n2:Profile {Email: '" + ownerEmail + "'})," +
                        "(o:Offer)" +
                        " where id(o)= "+offerID +
                    " CREATE(n) -[r: GUARDIAN]-> (p: Application " +
                    "{ Status: 'Oczekuje na akceptacje" +
                    "', Message: '" + application.Message +"'})," +
                    "(n2) -[t: OWNER]-> (p)," +
                    "(p) -[y: NOTIFICATION_TO_THE_OFFER]-> (o)"
                );
                await cursor.ConsumeAsync();
            }
            finally
            {
                await session.CloseAsync();
            }

            return RedirectToAction("Details", "Offers", new { offerID });
        }


        // GET: Applications/Edit/5
        public async Task<ActionResult> EditResign(int offerID)
        {
            IAsyncSession session = db._driver.AsyncSession();
            try
            {
                string status = null;

                var cursor =
                        await session.RunAsync(
                            "match (p:Profile {Email: '" + User.Identity.Name + "'})-[rel1:GUARDIAN]->(app:Application)<-[rel2:OWNER]-(p2:Profile)," +
                            "(app)-[rel3:NOTIFICATION_TO_THE_OFFER]->(o:Offer) " +
                            "where id(o) = "+ offerID +
                            " return app");

                List<IRecord> Records = await cursor.ToListAsync();
                foreach (var item in Records)
                {
                    INode nodeApplication = (INode)item.Values["app"];
                    status = nodeApplication.Properties.Where(a => a.Key == "Status").Select(a => a.Value).First().As<string>();
                    break;
                }

                if (status == "Właściciel odrzucił ofertę")
                {
                    IResultCursor cursor1 = await session.RunAsync(
                        "match (k:Profile {Email: '"+ User.Identity.Name + "'})-[rk:GUARDIAN]->(app:Application)" +
                            "-[ro:NOTIFICATION_TO_THE_OFFER]->(o:Offer) " +
                            "where id(o) = " + offerID +
                        " SET app.Status = 'Odrzucone'"
                    );
                    await cursor1.ConsumeAsync();
                }
                else
                {
                    IResultCursor cursor2 = await session.RunAsync(
                        "match (k:Profile {Email: '" + User.Identity.Name + "'})-[rk:GUARDIAN]->(app:Application)" +
                            "-[ro:NOTIFICATION_TO_THE_OFFER]->(o:Offer) " +
                            "where id(o) = " + offerID +
                        " SET app.Status = 'Opiekun zrezygnował z oferty'"
                    );
                    await cursor2.ConsumeAsync();
                }
                await cursor.ConsumeAsync();
            }
            finally
            {
                await session.CloseAsync();
            }
            return RedirectToAction("MyApplications");
        }

        public async Task<ActionResult> EditDiscard(int offerID)
        {
            IAsyncSession session = db._driver.AsyncSession();
            try
            {
                string status=null;
                var cursor =
                        await session.RunAsync(
                            "match (p:Profile {Email: '" + User.Identity.Name + "'})-[rel1:GUARDIAN]->(app:Application)<-[rel2:OWNER]-(p2:Profile)," +
                            "(app)-[rel3:NOTIFICATION_TO_THE_OFFER]->(o:Offer) " +
                            "where id(o) = " + offerID +
                            " return app");

                List<IRecord> Records = await cursor.ToListAsync();
                foreach (var item in Records)
                {
                    INode nodeApplication = (INode)item.Values["app"];
                    status = nodeApplication.Properties.Where(a => a.Key == "Status").Select(a => a.Value).First().As<string>();
                    break;
                }

                if (status == "Opiekun zrezygnował z oferty")
                {
                    IResultCursor cursor1 = await session.RunAsync(
                        "match (k:Profile {Email: '" + User.Identity.Name + "'})-[rk:OWNER]->(app:Application)" +
                            "-[ro:NOTIFICATION_TO_THE_OFFER]->(o:Offer) " +
                            "where id(o) = " + offerID +
                        " SET app.Status = 'Odrzucone'"
                    );
                    await cursor1.ConsumeAsync();
                }
                else
                {
                    IResultCursor cursor2 = await session.RunAsync(
                        "match (k:Profile {Email: '" + User.Identity.Name + "'})-[rk:OWNER]->(app:Application)" +
                            "-[ro:NOTIFICATION_TO_THE_OFFER]->(o:Offer) " +
                            "where id(o) = " + offerID +
                        " SET app.Status = 'Właściciel odrzucił ofertę'"
                    );
                    await cursor2.ConsumeAsync();
                }
            }
            finally
            {
                await session.CloseAsync();
            }
            return RedirectToAction("ApplicationsToMyOffers");
        }

        public async Task<ActionResult> EditAccept(int offerID)
        {
            IAsyncSession session = db._driver.AsyncSession();
            try
            {
                IResultCursor cursor = await session.RunAsync(
                    "match (k:Profile {Email: '" + User.Identity.Name + "'})-[rk:OWNER]->(app:Application)" +
                        "-[ro:NOTIFICATION_TO_THE_OFFER]->(o:Offer) " +
                            "where id(o) = " + offerID +
                    " SET app.Status = 'Zaakceptowane'"
                );
                await cursor.ConsumeAsync();
            }
            finally
            {
                await session.CloseAsync();
            }
            return RedirectToAction("ApplicationsToMyOffers");
        }

        // GET: Applications/Delete/5
        public async Task<ActionResult> Delete(int offerID)
        {
            IAsyncSession session = db._driver.AsyncSession();
            try
            {
                string status = null;
                var cursorStatus =
                        await session.RunAsync(
                            "match (p:Profile {Email: '" + User.Identity.Name + "'})-[rel1:GUARDIAN]->(app:Application)<-[rel2:OWNER]-(p2:Profile)," +
                            "(app)-[rel3:NOTIFICATION_TO_THE_OFFER]->(o:Offer) " +
                            "where id(o) = " + offerID +
                            " return app");

                List<IRecord> Records = await cursorStatus.ToListAsync();
                foreach (var item in Records)
                {
                    INode nodeApplication = (INode)item.Values["app"];
                    status = nodeApplication.Properties.Where(a => a.Key == "Status").Select(a => a.Value).First().As<string>();
                    break;
                }
                if (status.Contains("Usunieta"))
                {
                    IResultCursor cursor = await session.RunAsync(
                        "match (app:Application)-[ro:NOTIFICATION_TO_THE_OFFER]->(o:Offer) " +
                            "where id(o) = " + offerID +
                        " SET app.Status = 'Usunieta'"
                    );
                    await cursor.ConsumeAsync();
                }
                else
                {
                    IResultCursor cursor = await session.RunAsync(
                        "match (app:Application)-[ro:NOTIFICATION_TO_THE_OFFER]->(o:Offer) " +
                            "where id(o) = " + offerID +
                        " SET app.Status = 'Usunieta przez " + User.Identity.Name +"'"
                    );
                    await cursor.ConsumeAsync();
                }
            }
            finally
            {
                await session.CloseAsync();
            }
            return RedirectToAction("History");
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
