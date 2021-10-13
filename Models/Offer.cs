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
        [StringLength(80, MinimumLength = 3, ErrorMessage = "Title cannot be longer than 30 characters.")]
        public String Title { get; set; }

        [Required]
        [Display(Name = "Description")]
        [StringLength(1500, ErrorMessage = "Description cannot be longer than 1500 characters.")]
        public String Description { get; set; }

        [DisplayFormat(DataFormatString = "{yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public string StartingDate { get; set; }

        [DisplayFormat(DataFormatString = "{yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public string EndDate { get; set; }
        public string AnimalName { get; set; }

        public Profile Profile { get; set; }
        public Animal Animal { get; set; }
    }
}