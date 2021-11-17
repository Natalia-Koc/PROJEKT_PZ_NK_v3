using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PROJEKT_PZ_NK_v3.Models
{
    public class Notification
    {
        public int ID { get; set; }
        public Profile Profile { get; set; }
        public Offer Offer { get; set; }
        public String Message { get; set; }

    }
}