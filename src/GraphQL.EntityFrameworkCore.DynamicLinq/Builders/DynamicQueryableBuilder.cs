using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using GraphQL.EntityFrameworkCore.DynamicLinq.Constants;
using GraphQL.EntityFrameworkCore.DynamicLinq.Enums;
using GraphQL.EntityFrameworkCore.DynamicLinq.Extensions;
using GraphQL.EntityFrameworkCore.DynamicLinq.Models;
using GraphQL.Execution;
using GraphQL.Types;
using Stef.Validation;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Builders;

internal class DynamicQueryableBuilder<T, TGraphQL>
{
    private readonly IQueryable<T> _queryable;
    private readonly QueryArgumentInfoList _list;
    private readonly ResolveFieldContext<TGraphQL> _context;
    private readonly IDictionary<string, ArgumentValue> _arguments;
    private readonly OrderByHelper<T> _helper;

    public DynamicQueryableBuilder(IQueryable<T> queryable, QueryArgumentInfoList list, ResolveFieldContext<TGraphQL> context)
    {
        Guard.NotNull(queryable);
        Guard.HasNoNulls(list);
        Guard.NotNull(context);

        _queryable = queryable;
        _list = list;
        _context = context;
        _arguments = context.Arguments != null ? new Dictionary<string, ArgumentValue>(context.Arguments, StringComparer.OrdinalIgnoreCase) : new Dictionary<string, ArgumentValue>();

        _helper = new OrderByHelper<T>(list, _arguments);
    }

    public IQueryable<T> Build()
    {
        try
        {
            var queryableWhere = ApplyWhere(_queryable);

            var queryableOrderBy = _helper.ApplyOrderBy(queryableWhere);

            return ApplyPaging(queryableOrderBy);
        }
        catch (Exception e)
        {
            _context.Errors.Add(new DynamicQueryableError(e.Message, e));

            // In case of an error, just return empty Queryable.
            return Enumerable.Empty<T>().AsQueryable();
        }
    }

    private IQueryable<T> ApplyPaging(IQueryable<T> queryable)
    {
        if (_list.HasPaging)
        {
            int page = Convert.ToInt32(_arguments[FieldNames.PageFieldName]);
            int pageSize = Convert.ToInt32(_arguments[FieldNames.PageSizeFieldName]);
            queryable = queryable.Page(page, pageSize);
        }

        return queryable;
    }

    private IQueryable<T> ApplyWhere(IQueryable<T> queryable)
    {
        var predicates = new List<string>();
        var values = new List<object>();

        var counter = new PlaceHolderCounter();

        foreach (var queryArgumentInfo in _list.FilterBy(QueryArgumentInfoType.GraphQL))
        {
            foreach (var argument in _arguments)
            {
                if (string.Equals(argument.Key, queryArgumentInfo.QueryArgument?.Name, StringComparison.OrdinalIgnoreCase))
                {
                    var predicate = BuildPredicate(queryArgumentInfo, argument.Value, counter);
                    if (predicate != null)
                    {
                        predicates.Add(predicate.Value.Text);
                        values.AddRange(predicate.Value.Values);
                    }
                }
            }
        }

        return predicates.Any() ? queryable.Where(string.Join($" {Operators.And} ", predicates), values.ToArray()) : queryable;
    }

    private (string Text, object[] Values)? BuildPredicate(QueryArgumentInfo info, object value, PlaceHolderCounter counter)
    {
        if (info.EntityPath == null)
        {
            return null;
        }

        var values = new List<object>();
        var predicates = new List<(string @operator, string placeHolder)>();
        if (info.QueryArgument?.Type == typeof(DateGraphType) && value is DateTime date)
        {
            predicates.Add((Operators.GreaterThanEqual, $"@{counter.GetNew()}"));
            predicates.Add((Operators.LessThan, $"@{counter.GetNew()}"));

            values.Add(date);
            values.Add(date.AddDays(1));
        }
        else
        {
            predicates.Add((Operators.Equal, $"@{counter.GetNew()}"));
            values.Add(value);
        }

        var text = string.Join($" {Operators.And} ", predicates.Select(p =>
        {
            if (!info.ParentGraphType.IsListGraphType())
            {
                string path = string.Join(".", info.EntityPath);
                string wrap = info.IsNonNullGraphType ? path : $"np({path})";
                return $"({wrap} {p.@operator} {p.placeHolder})";
            }

            var theRest = string.Join(".", info.EntityPath.Skip(1));
            return $"({info.EntityPath[0]} != null ? {info.EntityPath[0]}.Any({theRest} {p.@operator} {p.placeHolder}) : false)";
        }));

        return (text, values.ToArray());
    }
}

internal class PlaceHolderCounter
{
    private int _value = -1;

    public int GetNew()
    {
        _value++;

        return _value;
    }
}