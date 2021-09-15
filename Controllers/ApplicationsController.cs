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
                            StartingDate = nodeOffer.Properties.Values.First().As<string>(),
                            Title = nodeOffer.Properties.Values.Skip(1).First().As<string>(),
                            ID = ((int)nodeOffer.Id),
                            Description = nodeOffer.Properties.Values.Skip(2).First().As<string>(),
                            EndDate = nodeOffer.Properties.Values.Skip(3).First().As<string>(),
                        };

                        Applications application = new Applications
                        {
                            Message = nodeApplication.Properties.Values.First().As<string>(),
                            Status = nodeApplication.Properties.Values.Skip(1).First().As<string>(),
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
                            StartingDate = nodeOffer.Properties.Values.First().As<string>(),
                            Title = nodeOffer.Properties.Values.Skip(1).First().As<string>(),
                            ID = ((int)nodeOffer.Id),
                            Description = nodeOffer.Properties.Values.Skip(2).First().As<string>(),
                            EndDate = nodeOffer.Properties.Values.Skip(3).First().As<string>(),
                        };

                        Applications application = new Applications
                        {
                            Message = nodeApplication.Properties.Values.First().As<string>(),
                            Status = nodeApplication.Properties.Values.Skip(1).First().As<string>(),
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
                            StartingDate = nodeOffer.Properties.Values.First().As<string>(),
                            Title = nodeOffer.Properties.Values.Skip(1).First().As<string>(),
                            ID = ((int)nodeOffer.Id),
                            Description = nodeOffer.Properties.Values.Skip(2).First().As<string>(),
                            EndDate = nodeOffer.Properties.Values.Skip(3).First().As<string>(),
                        };

                        Applications application = new Applications
                        {
                            Message = nodeApplication.Properties.Values.First().As<string>(),
                            Status = nodeApplication.Properties.Values.Skip(1).First().As<string>(),
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
                var cursor =
                        await session.RunAsync(
                            "match (p:Profile {Email: '" + User.Identity.Name + "'})-[rel1:GUARDIAN]->(app:Application)<-[rel2:OWNER]-(p2:Profile)," +
                            "(app)-[rel3:NOTIFICATION_TO_THE_OFFER]->(o:Offer {OfferID:" + offerID + "}) " +
                            "return app");

                IRecord Record = await cursor.SingleAsync();
                INode nodeApplication = (INode)Record.Values["app"];
                string status = nodeApplication.Properties.Values.Skip(1).First().As<string>();

                if (status == "Właściciel odrzucił ofertę")
                {
                    IResultCursor cursor1 = await session.RunAsync(
                        "match (k:Profile {Email: '"+ User.Identity.Name + "'})-[rk:GUARDIAN]->(app:Application)" +
                            "-[ro:NOTIFICATION_TO_THE_OFFER]->(o:Offer {OfferID:"+ offerID + "})" +
                        "SET app.Status = 'Odrzucone'"
                    );
                    await cursor1.ConsumeAsync();
                }
                else
                {
                    IResultCursor cursor2 = await session.RunAsync(
                        "match (k:Profile {Email: '" + User.Identity.Name + "'})-[rk:GUARDIAN]->(app:Application)" +
                            "-[ro:NOTIFICATION_TO_THE_OFFER]->(o:Offer {OfferID:" + offerID + "})" +
                        "SET app.Status = 'Opiekun zrezygnował z oferty'"
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
                var cursor =
                        await session.RunAsync(
                            "match (p:Profile {Email: '" + User.Identity.Name + "'})-[rel1:GUARDIAN]->(app:Application)<-[rel2:OWNER]-(p2:Profile)," +
                            "(app)-[rel3:NOTIFICATION_TO_THE_OFFER]->(o:Offer {OfferID:" + offerID + "}) " +
                            "return app");

                IRecord Record = await cursor.SingleAsync();
                INode nodeApplication = (INode)Record.Values["app"];
                string status = nodeApplication.Properties.Values.Skip(1).First().As<string>();

                if (status == "Opiekun zrezygnował z oferty")
                {
                    IResultCursor cursor1 = await session.RunAsync(
                        "match (k:Profile {Email: '" + User.Identity.Name + "'})-[rk:OWNER]->(app:Application)" +
                            "-[ro:NOTIFICATION_TO_THE_OFFER]->(o:Offer {OfferID:" + offerID + "})" +
                        "SET app.Status = 'Odrzucone'"
                    );
                    await cursor1.ConsumeAsync();
                }
                else
                {
                    IResultCursor cursor2 = await session.RunAsync(
                        "match (k:Profile {Email: '" + User.Identity.Name + "'})-[rk:OWNER]->(app:Application)" +
                            "-[ro:NOTIFICATION_TO_THE_OFFER]->(o:Offer {OfferID:" + offerID + "})" +
                        "SET app.Status = 'Właściciel odrzucił ofertę'"
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
                        "-[ro:NOTIFICATION_TO_THE_OFFER]->(o:Offer {OfferID:" + offerID + "})" +
                    "SET app.Status = 'Zaakceptowane'"
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
                var cursorStatus =
                        await session.RunAsync(
                            "match (p:Profile {Email: '" + User.Identity.Name + "'})-[rel1:GUARDIAN]->(app:Application)<-[rel2:OWNER]-(p2:Profile)," +
                            "(app)-[rel3:NOTIFICATION_TO_THE_OFFER]->(o:Offer {OfferID:" + offerID + "}) " +
                            "return app");

                IRecord Record = await cursorStatus.SingleAsync();
                INode nodeApplication = (INode)Record.Values["app"];
                string status = nodeApplication.Properties.Values.Skip(1).First().As<string>();

                if (status.Contains("Usunieta"))
                {
                    IResultCursor cursor = await session.RunAsync(
                        "match (app:Application)-[ro:NOTIFICATION_TO_THE_OFFER]->(o:Offer {OfferID:" + offerID + "})" +
                        "SET app.Status = 'Usunieta'"
                    );
                    await cursor.ConsumeAsync();
                }
                else
                {
                    IResultCursor cursor = await session.RunAsync(
                        "match (app:Application)-[ro:NOTIFICATION_TO_THE_OFFER]->(o:Offer {OfferID:" + offerID + "})" +
                        "SET app.Status = 'Usunieta przez " + User.Identity.Name +"'"
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
