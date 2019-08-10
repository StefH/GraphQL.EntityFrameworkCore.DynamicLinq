using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Utils.Entities
{
    [ExcludeFromCodeCoverage]
    public class Guest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(300)]
        public string Name { get; set; }

        public DateTime RegisterDate { get; set; }

        public int? NullableInt { get; set; }
    }
}