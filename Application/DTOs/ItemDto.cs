using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class ItemDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; }
        [StringLength(1000, ErrorMessage = "About section cannot exceed 1000 characters.")]
        public string About { get; set; }
        [Required(ErrorMessage = "ImageUrl is required.")]
        public string Type { get; set; }
        public double? Rating { get; set; }
    }
}
