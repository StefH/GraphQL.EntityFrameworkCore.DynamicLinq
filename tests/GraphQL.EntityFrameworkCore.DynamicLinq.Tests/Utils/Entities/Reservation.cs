using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Utils.Entities
{
    [ExcludeFromCodeCoverage]
    public class Reservation
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("RoomId")]
        public Room Room { get; set; }
        public int RoomId { get; set; }

        [ForeignKey("GuestId")]
        public Guest Guest { get; set; }
        public int GuestId { get; set; }

        [Required]
        public DateTime CheckinDate { get; set; }

        public DateTime CheckoutDate { get; set; }

        public ICollection<Extra> Extras { get; set; }
    }
}