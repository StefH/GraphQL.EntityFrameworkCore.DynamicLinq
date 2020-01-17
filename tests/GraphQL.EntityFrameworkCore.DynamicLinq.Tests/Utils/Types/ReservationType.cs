using System.Diagnostics.CodeAnalysis;
using GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Utils.Entities;
using GraphQL.Types;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Utils.Types
{
    [ExcludeFromCodeCoverage]
    public class ReservationType : ObjectGraphType<Reservation>
    {
        public ReservationType()
        {
            Field(x => x.Id);
            // Field(x => x.CheckinDate).Description("The first day of the stay");
            Field(typeof(DateGraphType), "CheckinDate", "The first day of the stay");
            Field(x => x.CheckoutDate).Description("The leaving day");
            Field<GuestType>(nameof(Reservation.Guest));
            Field<RoomType>(nameof(Reservation.Room));
            Field<ListGraphType<ExtraType>>("Extras", resolve: context => context.Source.Extras);
        }
    }
}