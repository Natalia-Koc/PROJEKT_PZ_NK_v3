using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PROJEKT_PZ_NK_v3.Models
{
    public class Applications
    {
        public int ID { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }

        public Profile Owner { get; set; }
        public Profile Guardian { get; set; }
        public Offer Offer { get; set; }
    }
}