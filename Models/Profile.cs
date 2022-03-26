using PROJEKT_PZ_NK_v3.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PROJEKT_PZ_NK_v3.Models
{
    public class Profile
    {
        [Key]
        public int ID { get; set; }

        [StringLength(30, ErrorMessage = "Login cannot be longer than 30 characters.")]
        public string Login { get; set; }

        [DataType(DataType.EmailAddress)]
        public String Email { get; set; }

        [StringLength(30, ErrorMessage = "First name cannot be longer than 30 characters.")]
        public String FirstName { get; set; }

        [StringLength(30, ErrorMessage = "Last name cannot be longer than 30 characters.")]
        public String LastName { get; set; }

        [StringLength(9, ErrorMessage = "Phone number cannot be longer than 9 characters.")]
        public String PhoneNumber { get; set; }
        public String City { get; set; }
        public String Street { get; set; }
        public String HouseNumber { get; set; }
        public int Rate { get; set; }

        public virtual List<Animal> Animals { get; set; }
        public virtual List<Comments> Comments { get; set; }
        public virtual List<Applications> OwnerApplications { get; set; }
        public virtual List<Applications> GuardianApplications { get; set; }
        public virtual List<Offer> Offers { get; set; }
        public virtual List<SavedProfiles> MySavedProfiles { get; set; }
        public virtual List<SavedProfiles> SavedProfiles { get; set; }
        public virtual List<Notification> Notifications { get; set; }
    }
}