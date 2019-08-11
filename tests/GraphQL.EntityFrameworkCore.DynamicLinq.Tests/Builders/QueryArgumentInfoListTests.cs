using FluentAssertions;
using GraphQL.EntityFrameworkCore.DynamicLinq.Enums;
using GraphQL.EntityFrameworkCore.DynamicLinq.Models;
using GraphQL.Types;
using Xunit;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Builders
{
    public class QueryArgumentInfoListTests
    {
        private readonly QueryArgumentInfoList _sut;

        public QueryArgumentInfoListTests()
        {
            _sut = new QueryArgumentInfoList();
        }

        [Fact]
        public void SupportOrderBy_ShouldAddOrderByQueryArgumentInfo()
        {
            // Act
            var list = _sut.SupportOrderBy();

            // Assert
            list.Count.Should().Be(1);
            list[0].QueryArgument.Name.Should().Be("OrderBy");
            list[0].QueryArgumentInfoType.Should().Be(QueryArgumentInfoType.OrderBy);
        }

        [Fact]
        public void Include_ShouldKeepArgumentInList()
        {
            // Arrange
            var infoId = new QueryArgumentInfo
            {
                QueryArgumentInfoType = QueryArgumentInfoType.DefaultGraphQL,
                QueryArgument = new QueryArgument(typeof(IntGraphType)) { Name = "Id" },
                IsNonNullGraphType = true,
                GraphQLPath = "Id",
                EntityPath = "Id"
            };
            _sut.Add(infoId);
            var infoX = new QueryArgumentInfo
            {
                QueryArgumentInfoType = QueryArgumentInfoType.DefaultGraphQL,
                QueryArgument = new QueryArgument(typeof(IntGraphType)) { Name = "X" },
                IsNonNullGraphType = true,
                GraphQLPath = "X",
                EntityPath = "X"
            };
            _sut.Add(infoX);

            // Act
            var list = _sut.Include("Id");

            // Assert
            list.Count.Should().Be(1);
            list[0].Should().Be(infoId);
        }

        [Fact]
        public void Exclude_ShouldRemoveArgumentFromList()
        {
            // Arrange
            var infoId = new QueryArgumentInfo
            {
                QueryArgumentInfoType = QueryArgumentInfoType.DefaultGraphQL,
                QueryArgument = new QueryArgument(typeof(IntGraphType)) { Name = "Id" },
                IsNonNullGraphType = true,
                GraphQLPath = "Id",
                EntityPath = "Id"
            };
            _sut.Add(infoId);
            var infoX = new QueryArgumentInfo
            {
                QueryArgumentInfoType = QueryArgumentInfoType.DefaultGraphQL,
                QueryArgument = new QueryArgument(typeof(IntGraphType)) { Name = "X" },
                IsNonNullGraphType = true,
                GraphQLPath = "X",
                EntityPath = "X"
            };
            _sut.Add(infoX);

            // Act
            var list = _sut.Exclude("Id");

            // Assert
            list.Count.Should().Be(1);
            list[0].Should().Be(infoX);
        }
    }
}
