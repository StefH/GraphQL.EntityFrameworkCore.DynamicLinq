using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyHotel.EntityFrameworkCore.Entities
{
    public class Building
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual ICollection<Room> Rooms { get; set; }
    }
}