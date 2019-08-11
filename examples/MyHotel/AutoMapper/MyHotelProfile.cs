using AutoMapper;
using MyHotel.EntityFrameworkCore.Entities;
using MyHotel.Models;

namespace MyHotel.AutoMapper
{
    public class MyHotelProfile : Profile
    {
        public MyHotelProfile()
        {
            CreateMap<Guest, GuestModel>();

            CreateMap<Room, RoomModel>()
                .ForMember(m => m.Detail, opt => opt.MapFrom(e => e.RoomDetail))
            ;

            CreateMap<RoomDetail, RoomDetailModel>()
                .ForMember(m => m.Identifier, opt => opt.MapFrom(e => e.Id))
            ;

            CreateMap<Reservation, ReservationModel>();

            CreateMap<Room, FlatRoomModel>()
                .ForMember(m => m.Beds, opt => opt.MapFrom(e => e.RoomDetail.Beds))
                .ForMember(m => m.Windows, opt => opt.MapFrom(e => e.RoomDetail.Windows))
            ;
        }
    }
}