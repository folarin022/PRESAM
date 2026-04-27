using System;
using System.Collections.Generic;
using System.Text;

namespace PRESAM.Application.DTOs
{
    public class CategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int ProductCount { get; set; }
        public bool IsActive { get; internal set; }
        public DateTime CreatedAt { get; internal set; }
    }
}
