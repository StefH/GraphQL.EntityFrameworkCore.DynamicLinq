using System;
using System.Linq;
using FluentAssertions;
using GraphQL.EntityFrameworkCore.DynamicLinq.Builders;
using GraphQL.EntityFrameworkCore.DynamicLinq.Enums;
using GraphQL.EntityFrameworkCore.DynamicLinq.Resolvers;
using GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Utils.Entities;
using GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Utils.Types;
using Moq;
using Xunit;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Builders
{
    public class QueryArgumentInfoListBuilderTests
    {
        private readonly Mock<IPropertyPathResolver> _propertyPathResolverMock;

        private readonly QueryArgumentInfoListBuilder _sut;

        public QueryArgumentInfoListBuilderTests()
        {
            _propertyPathResolverMock = new Mock<IPropertyPathResolver>();
            _propertyPathResolverMock.Setup(pr => pr.Resolve(It.IsAny<Type>(), It.IsAny<string>(), It.IsAny<Type>()))
                .Returns((Type sourceType, string sourcePath, Type destinationType) => sourcePath);

            _sut = new QueryArgumentInfoListBuilder(_propertyPathResolverMock.Object);
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
        public void Build_With_GraphType_ReturnsCorrectQueryArgumentInfoList()
        {
            // Arrange
            _propertyPathResolverMock.Setup(pr => pr.Resolve(It.IsAny<Type>(), "Id", It.IsAny<Type>())).Returns("Idee");

            // Act
            var list = _sut.Build<RoomType>();

            // Assert
            list.Count(q => q.QueryArgumentInfoType == QueryArgumentInfoType.DefaultGraphQL).Should().Be(8);
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
            list.Select(q => q.EntityPath).Should().BeEquivalentTo(
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
            list.Count(q => q.QueryArgumentInfoType == QueryArgumentInfoType.DefaultGraphQL).Should().Be(4);
            list.Count(q => q.QueryArgumentInfoType == QueryArgumentInfoType.DefaultGraphQL && q.IsNonNullGraphType).Should().Be(3);
            list.Select(q => q.GraphQLPath).Should().BeEquivalentTo("Id", "Name", "RegisterDate", "NullableInt");
            list.Select(q => q.EntityPath).Should().BeEquivalentTo("Idee", "Name", "RegisterDate", "NullableInt");
        }

        [Fact]
        public void Build_With_GraphType_And_SupportOrderBy_ReturnsCorrectQueryArgumentInfoList()
        {
            // Arrange
            _propertyPathResolverMock.Setup(pr => pr.Resolve(It.IsAny<Type>(), "Id", It.IsAny<Type>())).Returns("Idee");

            // Act
            var list = _sut.Build<GuestType>().SupportOrderBy();

            // Assert
            list.Count(q => q.QueryArgumentInfoType == QueryArgumentInfoType.DefaultGraphQL).Should().Be(4);
            list.Count(q => q.QueryArgumentInfoType == QueryArgumentInfoType.OrderBy).Should().Be(1);
            list.First(q => q.QueryArgumentInfoType == QueryArgumentInfoType.OrderBy).QueryArgument.Name.Should().Be("OrderBy");
            list.Select(q => q.GraphQLPath).Should().BeEquivalentTo("Id", "Name", "RegisterDate", "NullableInt", null);
            list.Select(q => q.EntityPath).Should().BeEquivalentTo("Idee", "Name", "RegisterDate", "NullableInt", null);
        }

        [Fact]
        public void Build_With_GraphType_With_NestedGraphTypes_ReturnsCorrectQueryArgumentInfoList()
        {
            // Act
            var list = _sut.Build<ReservationType>();

            // Assert
            list.Count(q => q.QueryArgumentInfoType == QueryArgumentInfoType.DefaultGraphQL).Should().Be(15);
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
            list.Select(q => q.EntityPath).Should().BeEquivalentTo(
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
    }
}
