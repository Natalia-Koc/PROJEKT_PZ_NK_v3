using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PROJEKT_PZ_NK_v3.Models
{
    public class Applications
    {
        public int ID { get; set; }
        public Profile Owner { get; set; }
        public int OwnerID { get; set; }
        public Profile Guardian { get; set; }
        public int GuardianID { get; set; }
        public Offer Offer { get; set; }
        public int OfferID { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
}