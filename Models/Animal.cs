using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace PROJEKT_PZ_NK_v3.Models
{
    public class Animal
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [Display(Name = "Name")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Name cannot be longer than 30 characters.")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Species")]
        public string Species { get; set; }//spiszis

        [Display(Name = "Race")]
        public string Race { get; set; }

        [Display(Name = "Gender")]
        public string Gender { get; set; }
        public string Weight { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [Display(Name = "Description")]
        [StringLength(1500, MinimumLength = 10, ErrorMessage = "Description cannot be longer than 1500 characters.")]
        public string Description { get; set; }
        public string Image { get; set; }

        public virtual Profile Profiles { get; set; }
    }
}