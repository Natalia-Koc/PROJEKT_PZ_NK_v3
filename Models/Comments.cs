using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PROJEKT_PZ_NK_v3.Models
{
    public class Comments
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [Display(Name = "Contents")]
        [StringLength(500, ErrorMessage = "Contents cannot be longer than 500 characters.")]
        public String Contents { get; set; }
        public int Grade { get; set; }
        public int AuthorID { get; set; }
        public int ProfileID { get; set; }

        public virtual Profile Author { get; set; }
        public virtual Profile Profile { get; set; }
    }
}