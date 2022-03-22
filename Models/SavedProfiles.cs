using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PROJEKT_PZ_NK_v3.Models
{
    public enum Saved
    {
        blocked,
        favourite
    }
    public class SavedProfiles
    {
        public int ID { get; set; }
        public int MyProfileID { get; set; }
        public int SavedProfileID { get; set; }
        public Saved SavedAs { get; set; }

        public virtual Profile MyProfile { get; set; }
        public virtual Profile SavedProfile { get; set; }

    }
}