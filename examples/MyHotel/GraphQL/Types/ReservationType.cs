using GraphQL.Types;
using MyHotel.EntityFrameworkCore.Entities;
using MyHotel.Models;

namespace MyHotel.GraphQL.Types
{
    public class ReservationType : ObjectGraphType<ReservationModel>
    {
        public ReservationType()
        {
            Field(x => x.Id);
            // Field(x => x.CheckinDate).Description("The first day of the stay");
            Field(typeof(DateGraphType), "CheckinDate", "The first day of the stay");
            Field(x => x.CheckoutDate).Description("The leaving day");
            Field<GuestType>(nameof(Reservation.Guest));
            Field<RoomType>(nameof(Reservation.Room));
        }
    }
}