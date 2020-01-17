using System;
using System.Linq;
using FluentAssertions;
using GraphQL.EntityFrameworkCore.DynamicLinq.Builders;
using GraphQL.EntityFrameworkCore.DynamicLinq.Enums;
using GraphQL.EntityFrameworkCore.DynamicLinq.Options;
using GraphQL.EntityFrameworkCore.DynamicLinq.Resolvers;
using GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Utils.Entities;
using GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Utils.Types;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Builders
{
    public class QueryArgumentInfoListBuilderTests
    {
        private readonly QueryArgumentInfoListBuilderOptions _options = new QueryArgumentInfoListBuilderOptions();
        private readonly Mock<IPropertyPathResolver> _propertyPathResolverMock;

        private readonly QueryArgumentInfoListBuilder _sut;

        public QueryArgumentInfoListBuilderTests()
        {
            var optionsMock = new Mock<IOptions<QueryArgumentInfoListBuilderOptions>>();
            optionsMock.Setup(o => o.Value).Returns(_options);

            _propertyPathResolverMock = new Mock<IPropertyPathResolver>();
            _propertyPathResolverMock.Setup(pr => pr.Resolve(It.IsAny<Type>(), It.IsAny<string>(), It.IsAny<Type>()))
                .Returns((Type sourceType, string sourcePath, Type destinationType) => sourcePath);

            _sut = new QueryArgumentInfoListBuilder(optionsMock.Object, _propertyPathResolverMock.Object);
        }

        [Fact]
        public void Build_With_NonGraphType_ReturnsEmptyQueryArgumentInfoList()
        {
            // Act
            var list = _sut.Build<Room>();

            // Assert
            list.Should().BeEmpty();
        }

        [Fact]
        public void Build_With_ListGraphType_AndSupportIsSetToTrueReturnsCorrectQueryArgumentInfoList()
        {
            // Arrange
            _options.MaxRecursionLevel = 3;
            _options.SupportListGraphType = true;

            // Act
            var list = _sut.Build<BuildingType>();

            // Assert
            list.Count(q => q.QueryArgumentInfoType == QueryArgumentInfoType.GraphQL).Should().Be(23);
            list.Select(q => q.GraphQLPath).Should().BeEquivalentTo(
                "Id",
                "Name",
                "RoomsId",
                "RoomsName",
                "RoomsNumber",
                "RoomsAllowedSmoking",
                "RoomsStatus",
                "RoomsRoomDetailId",
                "RoomsRoomDetailWindows",
                "RoomsRoomDetailBeds",
                "RoomsReservationId",
                "RoomsReservationCheckinDate",
                "RoomsReservationCheckoutDate",
                "RoomsReservationGuestId",
                "RoomsReservationGuestName",
                "RoomsReservationGuestRegisterDate",
                "RoomsReservationGuestNullableInt",
                "RoomsReservationRoomId",
                "RoomsReservationRoomName",
                "RoomsReservationRoomNumber",
                "RoomsReservationRoomAllowedSmoking",
                "RoomsReservationRoomStatus",
                "RoomsReservationExtrasTest"
            );
            list.Select(q => string.Join(".", q.EntityPath.Select(ep => ep.Path))).Should().BeEquivalentTo(
                "Id",
                "Name",
                "Rooms.Id",
                "Rooms.Name",
                "Rooms.Number",
                "Rooms.AllowedSmoking",
                "Rooms.Status",
                "Rooms.RoomDetail.Id",
                "Rooms.RoomDetail.Windows",
                "Rooms.RoomDetail.Beds",
                "Rooms.Reservation.Id",
                "Rooms.Reservation.CheckinDate",
                "Rooms.Reservation.CheckoutDate",
                "Rooms.Reservation.Guest.Id",
                "Rooms.Reservation.Guest.Name",
                "Rooms.Reservation.Guest.RegisterDate",
                "Rooms.Reservation.Guest.NullableInt",
                "Rooms.Reservation.Room.Id",
                "Rooms.Reservation.Room.Name",
                "Rooms.Reservation.Room.Number",
                "Rooms.Reservation.Room.AllowedSmoking",
                "Rooms.Reservation.Room.Status",
                "Rooms.Reservation.Extras.Test"
            );

            // Value       = "abc"
            // EntityPath  = Rooms.Reservation.Extras.Test
            // Linq        = Rooms.Any(r => r.Reservation.Extras.Any(e => e.Test == "abc"))
            // DynamicLinq = Rooms.Any(Reservation.Extras.Any(Test == @0))
            var buildings = new Building[0].AsQueryable();
            var linq = buildings.Where(b => b.Rooms.Any(r => r.Reservation.Extras.Any(e => e.Test == "abc")));

            list.Select(q => string.Join("_", q.EntityPath.Select(ep => $"{ep.ParentGraphType?.Name}:{ep.GraphType?.Name}"))).Should().BeEquivalentTo(
                ":IntGraphType",
                ":StringGraphType",
                ":ListGraphType`1_ListGraphType`1:IntGraphType",
                ":ListGraphType`1_ListGraphType`1:StringGraphType",
                ":ListGraphType`1_ListGraphType`1:IntGraphType",
                ":ListGraphType`1_ListGraphType`1:BooleanGraphType",
                ":ListGraphType`1_ListGraphType`1:RoomStatusType",
                ":ListGraphType`1_ListGraphType`1:RoomDetailType_ListGraphType`1:IntGraphType",
                ":ListGraphType`1_ListGraphType`1:RoomDetailType_ListGraphType`1:IntGraphType",
                ":ListGraphType`1_ListGraphType`1:RoomDetailType_ListGraphType`1:IntGraphType",
                ":ListGraphType`1_ListGraphType`1:ReservationType_ListGraphType`1:IntGraphType",
                ":ListGraphType`1_ListGraphType`1:ReservationType_ListGraphType`1:DateGraphType",
                ":ListGraphType`1_ListGraphType`1:ReservationType_ListGraphType`1:DateGraphType",
                ":ListGraphType`1_ListGraphType`1:ReservationType_ListGraphType`1:GuestType_ListGraphType`1:IntGraphType",
                ":ListGraphType`1_ListGraphType`1:ReservationType_ListGraphType`1:GuestType_ListGraphType`1:StringGraphType",
                ":ListGraphType`1_ListGraphType`1:ReservationType_ListGraphType`1:GuestType_ListGraphType`1:DateGraphType",
                ":ListGraphType`1_ListGraphType`1:ReservationType_ListGraphType`1:GuestType_ListGraphType`1:IntGraphType",
                ":ListGraphType`1_ListGraphType`1:ReservationType_ListGraphType`1:RoomType_ListGraphType`1:IntGraphType",
                ":ListGraphType`1_ListGraphType`1:ReservationType_ListGraphType`1:RoomType_ListGraphType`1:StringGraphType",
                ":ListGraphType`1_ListGraphType`1:ReservationType_ListGraphType`1:RoomType_ListGraphType`1:IntGraphType",
                ":ListGraphType`1_ListGraphType`1:ReservationType_ListGraphType`1:RoomType_ListGraphType`1:BooleanGraphType",
                ":ListGraphType`1_ListGraphType`1:ReservationType_ListGraphType`1:RoomType_ListGraphType`1:RoomStatusType",
                ":ListGraphType`1_ListGraphType`1:ReservationType_ListGraphType`1:ListGraphType`1_ListGraphType`1:StringGraphType"
            );
        }

        [Fact]
        public void Build_With_ListGraphType_AndSupportIsSetToFalseReturnsCorrectQueryArgumentInfoList()
        {
            // Arrange
            _options.SupportListGraphType = false;

            // Act
            var list = _sut.Build<BuildingType>();

            // Assert
            list.Count(q => q.QueryArgumentInfoType == QueryArgumentInfoType.GraphQL).Should().Be(3);
            list.Select(q => q.GraphQLPath).Should().BeEquivalentTo(
                "Rooms",
                "Id",
                "Name"
            );
            list.Select(q => string.Join(".", q.EntityPath)).Should().BeEquivalentTo(
                "Rooms",
                "Id",
                "Name"
            );
        }

        [Fact]
        public void Build_With_GraphType_ReturnsCorrectQueryArgumentInfoList()
        {
            // Arrange
            _propertyPathResolverMock.Setup(pr => pr.Resolve(It.IsAny<Type>(), "Id", It.IsAny<Type>())).Returns("Idee");

            // Act
            var list = _sut.Build<RoomType>();

            // Assert
            list.Count(q => q.QueryArgumentInfoType == QueryArgumentInfoType.GraphQL).Should().Be(8);
            list.Select(q => q.GraphQLPath).Should().BeEquivalentTo(
                "Id",
                "Name",
                "Number",
                "AllowedSmoking",
                "Status",
                "RoomDetailId",
                "RoomDetailWindows",
                "RoomDetailBeds"
            );
            list.Select(q => string.Join(".", q.EntityPath)).Should().BeEquivalentTo(
                "Idee",
                "Name",
                "Number",
                "AllowedSmoking",
                "Status",
                "RoomDetail.Idee",
                "RoomDetail.Windows",
                "RoomDetail.Beds"
            );
        }

        [Fact]
        public void Build_With_GraphType_With_Nullable_ReturnsCorrectQueryArgumentInfoList()
        {
            // Arrange
            _propertyPathResolverMock.Setup(pr => pr.Resolve(It.IsAny<Type>(), "Id", It.IsAny<Type>())).Returns("Idee");

            // Act
            var list = _sut.Build<GuestType>();

            // Assert
            list.Count(q => q.QueryArgumentInfoType == QueryArgumentInfoType.GraphQL).Should().Be(4);
            list.Count(q => q.QueryArgumentInfoType == QueryArgumentInfoType.GraphQL && q.IsNonNullGraphType).Should().Be(3);
            list.Select(q => q.GraphQLPath).Should().BeEquivalentTo("Id", "Name", "RegisterDate", "NullableInt");
            list.SelectMany(q => q.EntityPath).Should().BeEquivalentTo("Idee", "Name", "RegisterDate", "NullableInt");
        }

        [Fact]
        public void Build_With_GraphType_And_SupportOrderBy_ReturnsCorrectQueryArgumentInfoList()
        {
            // Arrange
            _propertyPathResolverMock.Setup(pr => pr.Resolve(It.IsAny<Type>(), "Id", It.IsAny<Type>())).Returns("Idee");

            // Act
            var list = _sut.Build<GuestType>().SupportOrderBy();

            // Assert
            list.Count(q => q.QueryArgumentInfoType == QueryArgumentInfoType.GraphQL).Should().Be(4);
            list.Count(q => q.QueryArgumentInfoType == QueryArgumentInfoType.OrderBy).Should().Be(1);
            list.First(q => q.QueryArgumentInfoType == QueryArgumentInfoType.OrderBy).QueryArgument.Name.Should().Be("OrderBy");
            list.Select(q => q.GraphQLPath).Should().BeEquivalentTo("Id", "Name", "RegisterDate", "NullableInt", null);
            //list.Select(q => q.EntityPath).Should().BeEquivalentTo("Idee", "Name", "RegisterDate", "NullableInt", null);
            list.SelectMany(q => q.EntityPath).Should().BeEquivalentTo("Idee", "Name", "RegisterDate", "NullableInt");
        }

        [Fact]
        public void Build_With_GraphType_And_SupportPaging_ReturnsCorrectQueryArgumentInfoList()
        {
            // Arrange
            _propertyPathResolverMock.Setup(pr => pr.Resolve(It.IsAny<Type>(), "Id", It.IsAny<Type>())).Returns("Idee");

            // Act
            var list = _sut.Build<GuestType>().SupportPaging();

            // Assert
            list.Count(q => q.QueryArgumentInfoType == QueryArgumentInfoType.GraphQL).Should().Be(4);
            list.Count(q => q.QueryArgumentInfoType == QueryArgumentInfoType.OrderBy).Should().Be(0);
            list.Count(q => q.QueryArgumentInfoType == QueryArgumentInfoType.Paging).Should().Be(2);
            list.Select(q => q.GraphQLPath).Should().BeEquivalentTo("Id", "Name", "RegisterDate", "NullableInt", null, null);
            list.SelectMany(q => q.EntityPath).Should().BeEquivalentTo("Idee", "Name", "RegisterDate", "NullableInt");
        }

        [Fact]
        public void Build_With_GraphType_With_NestedGraphTypes_ReturnsCorrectQueryArgumentInfoList()
        {
            // Act
            var list = _sut.Build<ReservationType>();

            // Assert
            list.Count(q => q.QueryArgumentInfoType == QueryArgumentInfoType.GraphQL).Should().Be(15);
            list.Select(q => q.GraphQLPath).Should().BeEquivalentTo(
                "Id",
                "CheckinDate",
                "CheckoutDate",
                "GuestId",
                "GuestName",
                "GuestRegisterDate",
                "GuestNullableInt",
                "RoomId",
                "RoomName",
                "RoomNumber",
                "RoomAllowedSmoking",
                "RoomStatus",
                "RoomRoomDetailId",
                "RoomRoomDetailWindows",
                "RoomRoomDetailBeds"
            );
            list.Select(q => string.Join(".", q.EntityPath)).Should().BeEquivalentTo(
                "Id",
                "CheckinDate",
                "CheckoutDate",
                "Guest.Id",
                "Guest.Name",
                "Guest.RegisterDate",
                "Guest.NullableInt",
                "Room.Id",
                "Room.Name",
                "Room.Number",
                "Room.AllowedSmoking",
                "Room.Status",
                "Room.RoomDetail.Id",
                "Room.RoomDetail.Windows",
                "Room.RoomDetail.Beds"
            );
        }

        [Fact]
        public void Build_With_GraphType_With_NestedGraphTypes_AndMaxRecurionsSetTo0_ReturnsCorrectQueryArgumentInfoList()
        {
            // Arrange
            _options.MaxRecursionLevel = 0;

            // Act
            var list = _sut.Build<ReservationType>();

            // Assert
            list.Count(q => q.QueryArgumentInfoType == QueryArgumentInfoType.GraphQL).Should().Be(3);
            list.Select(q => q.GraphQLPath).Should().BeEquivalentTo(
                "Id",
                "CheckinDate",
                "CheckoutDate"
            );
            list.SelectMany(q => q.EntityPath).Should().BeEquivalentTo(
                "Id",
                "CheckinDate",
                "CheckoutDate"
            );
        }

        [Fact]
        public void Build_With_GraphType_With_NestedGraphTypes_AndMaxRecurionsSetTo1_ReturnsCorrectQueryArgumentInfoList()
        {
            // Arrange
            _options.MaxRecursionLevel = 1;

            // Act
            var list = _sut.Build<ReservationType>();

            // Assert
            list.Count(q => q.QueryArgumentInfoType == QueryArgumentInfoType.GraphQL).Should().Be(12);
            list.Select(q => q.GraphQLPath).Should().BeEquivalentTo(
                "Id",
                "CheckinDate",
                "CheckoutDate",
                "GuestId",
                "GuestName",
                "GuestRegisterDate",
                "GuestNullableInt",
                "RoomId",
                "RoomName",
                "RoomNumber",
                "RoomAllowedSmoking",
                "RoomStatus"
            );
            list.Select(q => string.Join(".", q.EntityPath)).Should().BeEquivalentTo(
                "Id",
                "CheckinDate",
                "CheckoutDate",
                "Guest.Id",
                "Guest.Name",
                "Guest.RegisterDate",
                "Guest.NullableInt",
                "Room.Id",
                "Room.Name",
                "Room.Number",
                "Room.AllowedSmoking",
                "Room.Status"
            );
        }
    }
}
