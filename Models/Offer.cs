using PROJEKT_PZ_NK_v3.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using PagedList;

namespace PROJEKT_PZ_NK_v3.Models
{
    public class Offer
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [Display(Name = "Title")]
        [StringLength(80, MinimumLength = 3, ErrorMessage = "Tytuł musi się składać z minimum 3 znaków")]
        public String Title { get; set; }

        [Required]
        [Display(Name = "Description")]
        [StringLength(1500, ErrorMessage = "Opis może sie składać z maksymalnie 1500 znaków")]
        public String Description { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat( DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime StartingDate { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }

        /*private int lastingDay;

        public int LastingDays
        {
            get { return lastingDay = EndDate.DayOfYear - StartingDate.DayOfYear; }
        }*/


        public int AnimalID { get; set; }
        public virtual Profile Profile { get; set; }
        public virtual Animal Animal { get; set; }

        public int MyProperty { get; set; }
    }
}