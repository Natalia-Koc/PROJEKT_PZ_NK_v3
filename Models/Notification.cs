using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PROJEKT_PZ_NK_v3.Models
{
    public class Notification
    {
        public int ID { get; set; }
        public int ProfileID { get; set; }
        public int OfferID { get; set; } 
        public String Message { get; set; }
        public Boolean Removed { get; set; }
        public int WhoIRateID { get; set; }

        public virtual Profile Profile { get; set; }
        public virtual Offer Offer { get; set; }

    }
}