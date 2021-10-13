using PROJEKT_PZ_NK_v3.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace PROJEKT_PZ_NK_v3.DAL
{
    public class Initializer : DropCreateDatabaseIfModelChanges<OfferContext>
    {
        protected override void Seed(OfferContext context)
        {
            
            
        }
    }
}