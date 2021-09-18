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
    public class CommentsController : Controller
    {
        private OfferContext db = new OfferContext();

        // POST: Comments/Create
        // Aby zapewnić ochronę przed atakami polegającymi na przesyłaniu dodatkowych danych, włącz określone właściwości, z którymi chcesz utworzyć powiązania.
        // Aby uzyskać więcej szczegółów, zobacz https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<ActionResult> Create(Comments comments)
        {
            if (ModelState.IsValid)
            {
                IAsyncSession session = db._driver.AsyncSession();
                try
                {
                    IResultCursor cursorCom = await session.RunAsync(
                        "MATCH (n:Profile {Email: '" + User.Identity.Name + "'}), (p2:Profile {Email: '" + comments.ProfilEmail + "'}) " +
                        "CREATE(n) -[r:AUTHOR]-> (p:Comment {contents: '" + comments.Contents + "', rate: " + comments.Grade + "})," +
                        "(p) -[t:COMMENTED_PROFILE]-> (p2)"
                    );
                    await cursorCom.ConsumeAsync();

                    if (comments.Grade != 0)
                    {
                        var cursor =
                            await session.RunAsync(
                                "match (c:Comment)-[rel:COMMENTED_PROFILE]->(p:Profile {Email: '"+ comments.ProfilEmail + "'}) return avg(c.rate)");
                            
                        IResultCursor cursorProfile = await session.RunAsync("match (a:Profile {Email: '" + comments.ProfilEmail + "'})" +
                            "SET a.Rate = " + cursor.SingleAsync().Result.Values.First().Value.As<int>()*20);
                        await cursor.ConsumeAsync();
                        await cursorProfile.ConsumeAsync();
                    }
                        
                }
                finally
                {
                    await session.CloseAsync();
                }
            }

            return RedirectToAction("DetailsAnotherProfile", "Profiles", new { email = comments.ProfilEmail });
        }

        public async Task<ActionResult> Delete(string email)
        {
            IAsyncSession session = db._driver.AsyncSession();
            try
            {
                IResultCursor cursorCom = await session.RunAsync(
                    "match (p:Profile {Email: '"+ User.Identity.Name +"'})" +
                        "-[rel:AUTHOR]->(c:Comment)" +
                        "-[rell:COMMENTED_PROFILE]->(p2:Profile {Email: '" + email + "'})" +
                        " with c limit 1 detach delete c"
                );
                await cursorCom.ConsumeAsync();

                IResultCursor cursorAllCom = await session.RunAsync(
                    "match (c:Comment)" +
                        "-[rel:COMMENTED_PROFILE]->(p2:Profile {Email: '" + email + "'})" +
                        " return count(c)"
                );

                if (cursorAllCom.SingleAsync().Result.Values.First().Value.As<int>() > 0)
                {
                    var cursor =
                        await session.RunAsync(
                            "match (c:Comment)-[rel:COMMENTED_PROFILE]->(p:Profile {Email: '"+ email +"'}) return avg(c.rate)");

                    IResultCursor cursorProfile = await session.RunAsync("match (a:Profile {Email: '" + email + "'})" +
                        "SET a.Rate = " + cursor.SingleAsync().Result.Values.First().Value.As<int>());
                    await cursorProfile.ConsumeAsync();
                }
                else
                {
                    IResultCursor cursorProfile = await session.RunAsync("match (a:Profile {Email: '" + email + "'})" +
                        "SET a.Rate = 0");
                    await cursorProfile.ConsumeAsync();
                }
            }
            finally
            {
                await session.CloseAsync();
            }


            return RedirectToAction("DetailsAnotherProfile", "Profiles", new { email });
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
