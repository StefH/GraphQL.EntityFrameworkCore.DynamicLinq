using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Utils.Entities
{
    [ExcludeFromCodeCoverage]
    public class RoomDetail
    {
        [Key]
        public int Id { get; set; }

        public int Windows { get; set; }

        public int Beds  { get; set; }
    }
}