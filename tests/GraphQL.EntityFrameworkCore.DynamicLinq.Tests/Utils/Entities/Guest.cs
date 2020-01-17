using System;
using System.Diagnostics.CodeAnalysis;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Utils.Entities
{
    [ExcludeFromCodeCoverage]
    public class Guest
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime RegisterDate { get; set; }

        public int? NullableInt { get; set; }

        public Reservation Reservation { get; set; }
    }
}