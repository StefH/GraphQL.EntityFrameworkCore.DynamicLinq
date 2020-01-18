using System.Collections.Generic;
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
        public void SupportPaging_Should_AddPagingByQueryArgumentInfos()
        {
            // Act
            var list = _sut.SupportPaging();

            // Assert
            list.Count.Should().Be(2);
            list[0].QueryArgument.Name.Should().Be("Page");
            list[0].QueryArgumentInfoType.Should().Be(QueryArgumentInfoType.Paging);
            list[1].QueryArgument.Name.Should().Be("PageSize");
            list[1].QueryArgumentInfoType.Should().Be(QueryArgumentInfoType.Paging);
        }

        [Fact]
        public void SupportPaging_WithCustomArgumentName_Should_AddPagingByQueryArgumentInfos()
        {
            // Act
            var list = _sut.SupportPaging("x", "y");

            // Assert
            list.Count.Should().Be(2);
            list[0].QueryArgument.Name.Should().Be("x");
            list[0].QueryArgumentInfoType.Should().Be(QueryArgumentInfoType.Paging);
            list[1].QueryArgument.Name.Should().Be("y");
            list[1].QueryArgumentInfoType.Should().Be(QueryArgumentInfoType.Paging);
        }

        [Fact]
        public void SupportOrderBy_Should_AddOrderByQueryArgumentInfo()
        {
            // Act
            var list = _sut.SupportOrderBy();

            // Assert
            list.Count.Should().Be(1);
            list[0].QueryArgument.Name.Should().Be("OrderBy");
            list[0].QueryArgumentInfoType.Should().Be(QueryArgumentInfoType.OrderBy);
        }

        [Fact]
        public void SupportOrderBy_WithCustomArgumentName_AddOrderByQueryArgumentInfo()
        {
            // Act
            var list = _sut.SupportOrderBy("o");

            // Assert
            list.Count.Should().Be(1);
            list[0].QueryArgument.Name.Should().Be("o");
            list[0].QueryArgumentInfoType.Should().Be(QueryArgumentInfoType.OrderBy);
        }

        [Fact]
        public void Include_ShouldKeepArgumentInList()
        {
            // Arrange
            var infoId = new QueryArgumentInfo
            {
                QueryArgumentInfoType = QueryArgumentInfoType.GraphQL,
                QueryArgument = new QueryArgument(typeof(IntGraphType)) { Name = "Id" },
                IsNonNullGraphType = true,
                GraphQLPath = "Id",
                EntityPath = new List<EntityPath> { new EntityPath { Path = "Id" } }
            };
            _sut.Add(infoId);
            var infoX = new QueryArgumentInfo
            {
                QueryArgumentInfoType = QueryArgumentInfoType.GraphQL,
                QueryArgument = new QueryArgument(typeof(IntGraphType)) { Name = "X" },
                IsNonNullGraphType = true,
                GraphQLPath = "X",
                EntityPath = new List<EntityPath> { new EntityPath { Path = "X" } }
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
                QueryArgumentInfoType = QueryArgumentInfoType.GraphQL,
                QueryArgument = new QueryArgument(typeof(IntGraphType)) { Name = "Id" },
                IsNonNullGraphType = true,
                GraphQLPath = "Id",
                EntityPath = new List<EntityPath> { new EntityPath { Path = "Id" } }
            };
            _sut.Add(infoId);
            var infoX = new QueryArgumentInfo
            {
                QueryArgumentInfoType = QueryArgumentInfoType.GraphQL,
                QueryArgument = new QueryArgument(typeof(IntGraphType)) { Name = "X" },
                IsNonNullGraphType = true,
                GraphQLPath = "X",
                EntityPath = new List<EntityPath> { new EntityPath { Path = "X" } }
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