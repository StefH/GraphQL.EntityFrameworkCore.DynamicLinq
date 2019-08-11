using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GraphQL;
using GraphQL.EntityFrameworkCore.DynamicLinq.Builders;
using GraphQL.EntityFrameworkCore.DynamicLinq.Extensions;
using GraphQL.Types;
using MyHotel.EntityFrameworkCore.Entities;
using MyHotel.GraphQL.Types;
using MyHotel.Models;
using MyHotel.Repositories;
using Newtonsoft.Json;

namespace MyHotel.GraphQL
{
    public class MyHotelQuery : ObjectGraphType
    {
        public MyHotelQuery(MyHotelRepository myHotelRepository, IQueryArgumentInfoListBuilder builder, IMapper mapper)
        {
            var guestQueryArgumentList = builder.Build<GuestType>().SupportOrderBy();
            Field<ListGraphType<GuestType>>("guests",
                arguments: new QueryArguments(guestQueryArgumentList.Select(q => q.QueryArgument)),
                resolve: context => myHotelRepository.GetGuestsQuery()
                    .ApplyQueryArguments(guestQueryArgumentList, context)
                    .ProjectTo<GuestModel>(mapper.ConfigurationProvider)
                    .ToList()
            );

            var roomQueryArgumentList = builder.Build<RoomType>().SupportOrderBy().Exclude("Id");
            Field<ListGraphType<RoomType>>("rooms",
                arguments: new QueryArguments(roomQueryArgumentList.Select(q => q.QueryArgument)),
                resolve: context => myHotelRepository.GetRoomsQuery()
                    .ApplyQueryArguments(roomQueryArgumentList, context)
                    .ProjectTo<RoomModel>(mapper.ConfigurationProvider)
                    .ToList()
            );

            var flatRoomQueryArgumentList = builder.Build<FlatRoomType>().SupportOrderBy();
            Field<ListGraphType<FlatRoomType>>("flatrooms",
                arguments: new QueryArguments(flatRoomQueryArgumentList.Select(q => q.QueryArgument)),
                resolve: context => myHotelRepository.GetRoomsQuery()
                    .ApplyQueryArguments(flatRoomQueryArgumentList, context)
                    .ProjectTo<FlatRoomModel>(mapper.ConfigurationProvider)
                    .ToList()
            );

            var reservationQueryArgumentList = builder.Build<ReservationType>().SupportOrderBy();
            Field<ListGraphType<ReservationType>>("reservations",
                arguments: new QueryArguments(reservationQueryArgumentList.Select(q => q.QueryArgument)),
                resolve: context => myHotelRepository.GetReservationsQuery()
                    .ApplyQueryArguments(reservationQueryArgumentList, context)
                    .ProjectTo<ReservationModel>(mapper.ConfigurationProvider)
                    .ToList()
            );

            Field<ListGraphType<ReservationType>>("reservations2",
                arguments: new QueryArguments(new List<QueryArgument>
                {
                    new QueryArgument<IdGraphType>
                    {
                        Name = "id"
                    },
                    new QueryArgument<DateGraphType>
                    {
                        Name = "checkinDate"
                    },
                    new QueryArgument<DateGraphType>
                    {
                        Name = "checkoutDate"
                    },
                    new QueryArgument<BooleanGraphType>
                    {
                        Name = "roomAllowedSmoking"
                    },
                    new QueryArgument<RoomStatusType>
                    {
                        Name = "roomStatus"
                    },
                    new QueryArgument<IntGraphType>
                    {
                        Name = "roomWindows"
                    },
                    new QueryArgument<IntGraphType>
                    {
                        Name = "roomBeds"
                    }
                }),
                resolve: context =>
                {
                    var query = myHotelRepository.GetReservationsQuery().AsQueryable();

                    Console.WriteLine("context.Arguments = " + JsonConvert.SerializeObject(context.Arguments));

                    var user = (ClaimsPrincipal)context.UserContext;
                    var isUserAuthenticated = ((ClaimsIdentity)user.Identity).IsAuthenticated;

                    var reservationId = context.GetArgument<int?>("id");
                    if (reservationId.HasValue)
                    {
                        if (reservationId.Value <= 0)
                        {
                            context.Errors.Add(new ExecutionError("reservationId must be greater than zero!"));
                            return new List<Reservation>();
                        }

                        query = query.Where(r => r.Id == reservationId.Value);
                    }

                    var checkinDate = context.GetArgument<DateTime?>("checkinDate");
                    if (checkinDate.HasValue)
                    {
                        query = query.Where(r => r.CheckinDate.Date == checkinDate.Value.Date);
                    }

                    var checkoutDate = context.GetArgument<DateTime?>("checkoutDate");
                    if (checkoutDate.HasValue)
                    {
                        query = query.Where(r => r.CheckoutDate.Date >= checkoutDate.Value.Date);
                    }

                    var allowedSmoking = context.GetArgument<bool?>("roomAllowedSmoking");
                    if (allowedSmoking.HasValue)
                    {
                        query = query.Where(r => r.Room.AllowedSmoking == allowedSmoking.Value);
                    }

                    var roomStatus = context.GetArgument<RoomStatus?>("roomStatus");
                    if (roomStatus.HasValue)
                    {
                        query = query.Where(r => r.Room.Status == roomStatus.Value);
                    }

                    var roomWindows = context.GetArgument<int?>("roomWindows");
                    if (roomWindows.HasValue)
                    {
                        query = query.Where(r => r.Room.RoomDetail.Windows == roomWindows);
                    }

                    var roomBeds = context.GetArgument<int?>("roomBeds");
                    if (roomBeds.HasValue)
                    {
                        query = query.Where(r => r.Room.RoomDetail.Beds == roomBeds);
                    }

                    return query.ProjectTo<ReservationModel>(mapper.ConfigurationProvider).ToList();
                }
            );

        }
    }
}
