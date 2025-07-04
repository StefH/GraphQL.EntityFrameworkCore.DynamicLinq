﻿using System;
using System.Collections.Generic;
using System.Linq;
using AwesomeAssertions;
using GraphQL.EntityFrameworkCore.DynamicLinq.Builders;
using GraphQL.EntityFrameworkCore.DynamicLinq.Enums;
using GraphQL.EntityFrameworkCore.DynamicLinq.Models;
using GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Utils.Entities;
using GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Utils.Types;
using GraphQL.Types;
using Xunit;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Builders;

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
        var queryable = Enumerable.Empty<Room>().AsQueryable();
        var list = new QueryArgumentInfoList();
        var context = new ResolveFieldContext<object>();

        var builder = new DynamicQueryableBuilder<Room, object>(queryable, list, context);

        // Act
        var result = builder.Build();

        // Assert
        result.Should().Equal(queryable);
    }

    [Fact]
    public void Build_For_BooleanGraphType_When_ContextArgumentMatchesListArgument_AppliesWhere()
    {
        // Arrange
        var queryable = Enumerable.Empty<Room>().AsQueryable();
        var list = new QueryArgumentInfoList
        {
            new QueryArgumentInfo
            {
                QueryArgumentInfoType = QueryArgumentInfoType.GraphQL,
                QueryArgument = new QueryArgument(typeof(BooleanGraphType)) { Name = "AllowedSmoking" },
                IsNonNullGraphType = true,
                GraphQLPath = "AllowedSmoking",
                EntityPath = new List<string> { "AllowedSmoking" }
            }
        };
        _context.Arguments.Add("allowedSmoking", false);

        var builder = new DynamicQueryableBuilder<Room, object>(queryable, list, _context);

        // Act
        var result = builder.Build();

        // Assert
        _context.Errors.Count.Should().Be(0);
        result.ToString().Should().Contain("Where(Param_0 => (Param_0.AllowedSmoking == False))");
    }

    [Fact]
    public void Build_For_ListGraphType_When_ContextArgumentMatchesListArgument_AppliesWhere()
    {
        // Arrange
        var queryable = Enumerable.Empty<Building>().AsQueryable();
        var list = new QueryArgumentInfoList
        {
            new QueryArgumentInfo
            {
                ParentGraphType = typeof(ListGraphType<RoomType>),
                QueryArgumentInfoType = QueryArgumentInfoType.GraphQL,
                QueryArgument = new QueryArgument(typeof(IntGraphType)) { Name = "RoomsId" },
                IsNonNullGraphType = true,
                GraphQLPath = "RoomsId",
                EntityPath = new List<string> { "Rooms", "Id" }
            }
        };
        _context.Arguments.Add("roomsId", 1);

        var builder = new DynamicQueryableBuilder<Building, object>(queryable, list, _context);

        // Act
        var result = builder.Build();

        // Assert
        _context.Errors.Count.Should().Be(0);
        result.ToString().Should().Contain("Where(Param_0 => IIF((Param_0.Rooms != null), Param_0.Rooms.Any(Param_1 => (Param_1.Id == 1)), False))");
    }

    [Fact]
    public void Build_When_ContextArgumentMatchesListArgumentAndIncludesOrderBy_AppliesWhereAndOrderBy()
    {
        // Arrange
        var queryable = Enumerable.Empty<Room>().AsQueryable();
        var list = new QueryArgumentInfoList
        {
            new QueryArgumentInfo
            {
                QueryArgumentInfoType = QueryArgumentInfoType.GraphQL,
                QueryArgument = new QueryArgument(typeof(IntGraphType)) { Name = "Number" },
                IsNonNullGraphType = true,
                GraphQLPath = "Number",
                EntityPath = new List<string> { "Number" }
            },
            new QueryArgumentInfo
            {
                QueryArgumentInfoType = QueryArgumentInfoType.GraphQL,
                QueryArgument = new QueryArgument(typeof(StringGraphType)) { Name = "Name" },
                IsNonNullGraphType = true,
                GraphQLPath = "Name",
                EntityPath = new List<string> { "Name" }
            },
            new QueryArgumentInfo
            {
                QueryArgumentInfoType = QueryArgumentInfoType.OrderBy,
                QueryArgument = new QueryArgument(typeof(StringGraphType)) { Name = "OrderBy" }
            }
        };
        _context.Arguments.Add("number", 42);
        _context.Arguments.Add("orderBy", "Name desc, Number asc");

        var builder = new DynamicQueryableBuilder<Room, object>(queryable, list, _context);

        // Act
        var result = builder.Build();

        // Assert
        _context.Errors.Count.Should().Be(0);
        result.ToString().Should().Contain("Where(Param_0 => (Param_0.Number == 42)).OrderByDescending(Param_1 => Param_1.Name).ThenBy(Param_1 => Param_1.Number)");
    }

    [Fact]
    public void Build_When_ContextArgumentMatchesListArgumentAndIncludesPaging_AppliesWhereAndPaging()
    {
        // Arrange
        var queryable = Enumerable.Empty<Room>().AsQueryable();
        var list = new QueryArgumentInfoList
        {
            new QueryArgumentInfo
            {
                QueryArgumentInfoType = QueryArgumentInfoType.GraphQL,
                QueryArgument = new QueryArgument(typeof(IntGraphType)) { Name = "Number" },
                IsNonNullGraphType = true,
                GraphQLPath = "Number",
                EntityPath = new List<string> { "Number" }
            },
            new QueryArgumentInfo
            {
                QueryArgumentInfoType = QueryArgumentInfoType.GraphQL,
                QueryArgument = new QueryArgument(typeof(StringGraphType)) { Name = "Name" },
                IsNonNullGraphType = true,
                GraphQLPath = "Name",
                EntityPath = new List<string> { "Name" }
            },
            new QueryArgumentInfo
            {
                QueryArgumentInfoType = QueryArgumentInfoType.Paging,
                QueryArgument = new QueryArgument(typeof(IntGraphType)) { Name = "Page" }
            },
            new QueryArgumentInfo
            {
                QueryArgumentInfoType = QueryArgumentInfoType.Paging,
                QueryArgument = new QueryArgument(typeof(IntGraphType)) { Name = "PageSize" }
            }
        };
        _context.Arguments.Add("number", 42);
        _context.Arguments.Add("page", 7);
        _context.Arguments.Add("pageSize", 3);

        var builder = new DynamicQueryableBuilder<Room, object>(queryable, list, _context);

        // Act
        var result = builder.Build();

        // Assert
        _context.Errors.Count.Should().Be(0);
        result.ToString().Should().Contain("Where(Param_0 => (Param_0.Number == 42)).Skip(18).Take(3)");
    }

    [Fact]
    public void Build_When_ContextArgumentIsDateGraphType_AppliesCorrectWhere()
    {
        // Arrange
        var date = new DateTime(2019, 2, 3).Date;
        var queryable = Enumerable.Empty<Reservation>().AsQueryable();
        var list = new QueryArgumentInfoList
        {
            new QueryArgumentInfo
            {
                QueryArgumentInfoType = QueryArgumentInfoType.GraphQL,
                QueryArgument = new QueryArgument(typeof(DateGraphType)) { Name = "CheckinDate" },
                IsNonNullGraphType = true,
                GraphQLPath = "CheckinDate",
                EntityPath = new List<string> { "CheckinDate" }
            }
        };
        _context.Arguments.Add("checkinDate", date);

        var builder = new DynamicQueryableBuilder<Reservation, object>(queryable, list, _context);

        // Act
        var result = builder.Build();

        // Assert
        _context.Errors.Count.Should().Be(0);
        result.ToString().Should().Contain("Where(Param_0 => ((Param_0.CheckinDate >=");
        result.ToString().Should().Contain("AndAlso (Param_0.CheckinDate <");
    }

    [Fact]
    public void Build_When_OrderByIsNotPresent_SkipsOrderBy()
    {
        // Arrange
        var queryable = Enumerable.Empty<Room>().AsQueryable();
        var list = new QueryArgumentInfoList
        {
            new QueryArgumentInfo
            {
                QueryArgumentInfoType = QueryArgumentInfoType.OrderBy,
                QueryArgument = new QueryArgument(typeof(StringGraphType)) { Name = "OrderBy" }
            }
        };

        var builder = new DynamicQueryableBuilder<Room, object>(queryable, list, _context);

        // Act
        var result = builder.Build();

        // Assert
        _context.Errors.Count.Should().Be(0);
        result.ToString().Should().Be("System.Linq.EmptyPartition`1[GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Utils.Entities.Room]");
    }

    [Fact]
    public void Build_When_OrderByIsEmpty_AddsError()
    {
        // Arrange
        var queryable = Enumerable.Empty<Room>().AsQueryable();
        var list = new QueryArgumentInfoList
        {
            new QueryArgumentInfo
            {
                QueryArgumentInfoType = QueryArgumentInfoType.OrderBy,
                QueryArgument = new QueryArgument(typeof(StringGraphType)) { Name = "OrderBy" }
            }
        };
        _context.Arguments.Add("orderBy", "");

        var builder = new DynamicQueryableBuilder<Room, object>(queryable, list, _context);

        // Act
        builder.Build();

        // Assert
        _context.Errors.Count.Should().Be(1);
        _context.Errors[0].Message.Should().Be("The \"OrderBy\" field is empty.");
    }

    [Fact]
    public void Build_When_OrderByContainsUndefinedSearchField_AddsError()
    {
        // Arrange
        var queryable = Enumerable.Empty<Room>().AsQueryable();
        var list = new QueryArgumentInfoList
        {
            new QueryArgumentInfo
            {
                QueryArgumentInfoType = QueryArgumentInfoType.OrderBy,
                QueryArgument = new QueryArgument(typeof(StringGraphType)) { Name = "OrderBy" }
            }
        };
        _context.Arguments.Add("orderBy", "test");

        var builder = new DynamicQueryableBuilder<Room, object>(queryable, list, _context);

        // Act
        builder.Build();

        // Assert
        _context.Errors.Count.Should().Be(1);
        _context.Errors[0].Message.Should().Be("The \"OrderBy\" field uses an unknown field \"test\".");
    }

    [Fact]
    public void Build_When_OrderByHasAscButIsMissingSearchField_AddsError()
    {
        // Arrange
        var queryable = Enumerable.Empty<Room>().AsQueryable();
        var list = new QueryArgumentInfoList
        {
            new QueryArgumentInfo
            {
                QueryArgumentInfoType = QueryArgumentInfoType.OrderBy,
                QueryArgument = new QueryArgument(typeof(StringGraphType)) { Name = "OrderBy" }
            }
        };
        _context.Arguments.Add("orderBy", "asc");

        var builder = new DynamicQueryableBuilder<Room, object>(queryable, list, _context);

        // Act
        builder.Build();

        // Assert
        _context.Errors.Count.Should().Be(1);
        _context.Errors[0].Message.Should().Be("The \"OrderBy\" field with value \"asc\" cannot be used without a query field.");
    }

    [Fact]
    public void Build_When_OrderByHasNullEntityPath_AddsError()
    {
        // Arrange
        var queryable = Enumerable.Empty<Reservation>().AsQueryable();
        var list = new QueryArgumentInfoList
        {
            new QueryArgumentInfo
            {
                QueryArgumentInfoType = QueryArgumentInfoType.OrderBy,
                QueryArgument = new QueryArgument(typeof(StringGraphType)) { Name = "OrderBy" }
            },
            new QueryArgumentInfo
            {
                QueryArgumentInfoType = QueryArgumentInfoType.GraphQL,
                QueryArgument = new QueryArgument(typeof(DateGraphType)) { Name = "CheckinDate" },
                IsNonNullGraphType = true,
                GraphQLPath = "CheckinDate",
                EntityPath = null
            }
        };
        _context.Arguments.Add("orderBy", "CheckinDate");

        var builder = new DynamicQueryableBuilder<Reservation, object>(queryable, list, _context);

        // Act
        var result = builder.Build();

        // Assert
        _context.Errors.Count.Should().Be(1);
        _context.Errors[0].Message.Should().Be("The \"EntityPath\" for field \"CheckinDate\" is null.");
    }
}