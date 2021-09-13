using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PROJEKT_PZ_NK_v3.ViewModels
{
    public class profil
    {
        public int ID { get; set; }

        public string Login { get; set; }

        public String Email { get; set; }

        public String FirstName { get; set; }

        public String LastName { get; set; }

        public String PhoneNumber { get; set; }
        public String City { get; set; }
        public String Street { get; set; }
        public String HouseNumber { get; set; }

        public int Rate { get; set; }
    }
}