using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using GraphQL.EntityFrameworkCore.DynamicLinq.Builders;
using GraphQL.EntityFrameworkCore.DynamicLinq.Enumerations;
using GraphQL.EntityFrameworkCore.DynamicLinq.Models;
using GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Utils.Entities;
using GraphQL.Types;
using Xunit;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Builders
{
    public class DynamicQueryableBuilderTests
    {
        private readonly ResolveFieldContext<object> _context = new ResolveFieldContext<object>
        {
            Arguments = new Dictionary<string, object>(),
            Errors = new ExecutionErrors()
        };

        [Fact]
        public void Build_When_ContextArgumentsIsNull_ReturnsSameQuery()
        {
            // Arrange
            var queryable = new[] { new Room() }.AsQueryable();
            var list = new QueryArgumentInfoList();
            var context = new ResolveFieldContext<object>();

            var builder = new DynamicQueryableBuilder<Room, object>(queryable, list, context);

            // Act
            var result = builder.Build();

            // Assert
            result.Should().Equal(queryable);
        }

        [Fact]
        public void Build_When_ContextArgumentMatchesListArgument_AppliesWhere()
        {
            // Arrange
            var queryable = new[] { new Room { Id = 1 }, new Room { Id = 42 } }.AsQueryable();
            var list = new QueryArgumentInfoList
            {
                new QueryArgumentInfo
                {
                    QueryArgumentInfoType = QueryArgumentInfoType.DefaultGraphQL,
                    QueryArgument = new QueryArgument(typeof(IntGraphType)) {Name = "Number"},
                    IsNonNullGraphType = true,
                    GraphQLPath = "Number",
                    EntityPath = "Number"
                }
            };
            _context.Arguments.Add("Number", 42);

            var builder = new DynamicQueryableBuilder<Room, object>(queryable, list, _context);

            // Act
            var result = builder.Build();

            // Assert
            _context.Errors.Count.Should().Be(0);
            result.ToString().Should().Contain("Where(Param_0 => (Param_0.Number == 42))");
        }

        [Fact]
        public void Build_When_ContextArgumentMatchesListArgumentAndIncludesOrdeBy_AppliesWhereAndOrderBy()
        {
            // Arrange
            var queryable = new[] { new Room { Id = 1 }, new Room { Id = 42 } }.AsQueryable();
            var list = new QueryArgumentInfoList
            {
                new QueryArgumentInfo
                {
                    QueryArgumentInfoType = QueryArgumentInfoType.DefaultGraphQL,
                    QueryArgument = new QueryArgument(typeof(IntGraphType)) { Name = "Number" },
                    IsNonNullGraphType = true,
                    GraphQLPath = "Number",
                    EntityPath = "Number"
                },
                new QueryArgumentInfo
                {
                    QueryArgumentInfoType = QueryArgumentInfoType.DefaultGraphQL,
                    QueryArgument = new QueryArgument(typeof(StringGraphType)) { Name = "Name" },
                    IsNonNullGraphType = true,
                    GraphQLPath = "Name",
                    EntityPath = "Name"
                },
                new QueryArgumentInfo
                {
                    QueryArgumentInfoType = QueryArgumentInfoType.OrderBy,
                    QueryArgument = new QueryArgument(typeof(StringGraphType)) { Name = "OrderBy" }
                }
            };
            _context.Arguments.Add("Number", 42);
            _context.Arguments.Add("OrderBy", "Name desc, Number asc");

            var builder = new DynamicQueryableBuilder<Room, object>(queryable, list, _context);

            // Act
            var result = builder.Build();

            // Assert
            _context.Errors.Count.Should().Be(0);
            result.ToString().Should().Contain("Where(Param_0 => (Param_0.Number == 42)).OrderByDescending(Param_1 => Param_1.Name).ThenBy(Param_1 => Param_1.Number)");
        }
    }
}