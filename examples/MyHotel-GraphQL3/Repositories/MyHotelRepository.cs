using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MyHotel.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyHotel.EntityFrameworkCore.Entities;

namespace MyHotel.Repositories
{
    public class MyHotelRepository
    {
        private readonly MyHotelDbContext _myHotelDbContext;
        private readonly IMapper _mapper;

        public MyHotelRepository(MyHotelDbContext myHotelDbContext, IMapper mapper)
        {
            _myHotelDbContext = myHotelDbContext;
            _mapper = mapper;
        }

        public async Task<List<T>> GetAll<T>()
        {
            return await GetReservationsQuery().ProjectTo<T>(_mapper.ConfigurationProvider).ToListAsync();
        }

        public Reservation Get(int id)
        {
            return GetReservationsQuery().Single(x => x.Id == id);
        }

        public IIncludableQueryable<Reservation, Guest> GetReservationsQuery()
        {
            return _myHotelDbContext
                .Reservations
                .Include(x => x.Room)
                .Include(x => x.Room.RoomDetail)
                .Include(x => x.Guest);
        }

        public IIncludableQueryable<Room, RoomDetail> GetRoomsQuery()
        {
            return _myHotelDbContext.Rooms
                .Include(x => x.RoomDetail);
        }

        public IQueryable<Guest> GetGuestsQuery()
        {
            return _myHotelDbContext.Guests;
        }
    }
}
