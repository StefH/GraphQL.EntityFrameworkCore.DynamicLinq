using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using GraphQL.EntityFrameworkCore.DynamicLinq.Constants;
using GraphQL.EntityFrameworkCore.DynamicLinq.Enums;
using GraphQL.EntityFrameworkCore.DynamicLinq.Models;
using GraphQL.EntityFrameworkCore.DynamicLinq.Validation;
using GraphQL.Types;
using JetBrains.Annotations;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Builders
{
    internal class DynamicQueryableBuilder<T, TGraphQL>
    {
        private readonly IQueryable<T> _queryable;
        private readonly QueryArgumentInfoList _list;
        private readonly ResolveFieldContext<TGraphQL> _context;
        private readonly IDictionary<string, object> _arguments;
        private readonly OrderByHelper<T> _helper;

        public DynamicQueryableBuilder([NotNull] IQueryable<T> queryable, [NotNull] QueryArgumentInfoList list, [NotNull] ResolveFieldContext<TGraphQL> context)
        {
            Guard.NotNull(queryable, nameof(queryable));
            Guard.HasNoNulls(list, nameof(list));
            Guard.NotNull(context, nameof(context));

            _queryable = queryable;
            _list = list;
            _context = context;
            _arguments = context.Arguments != null ? new Dictionary<string, object>(context.Arguments, StringComparer.OrdinalIgnoreCase) : new Dictionary<string, object>();

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
            foreach (var queryArgumentInfo in _list.FilterBy(QueryArgumentInfoType.GraphQL))
            {
                foreach (var argument in _arguments)
                {
                    if (string.Equals(argument.Key, queryArgumentInfo.QueryArgument?.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        var predicate = BuildPredicate(queryArgumentInfo, argument.Value);
                        if (predicate != null)
                        {
                            predicates.Add(predicate);
                        }
                    }
                }
            }

            return predicates.Any() ? queryable.Where(string.Join($" {Operators.And} ", predicates)) : queryable;
        }

        private string? BuildPredicate(QueryArgumentInfo info, object value)
        {
            if (info.EntityPath == null)
            {
                return null;
            }

            var predicates = new List<(string propertyPath, string @operator, object propertyValue)>();
            if (info.QueryArgument?.Type == typeof(DateGraphType) && value is DateTime date)
            {
                predicates.Add((info.EntityPath, Operators.GreaterThanEqual, date));
                predicates.Add((info.EntityPath, Operators.LessThan, date.AddDays(1)));
            }
            else
            {
                predicates.Add((info.EntityPath, Operators.Equal, value));
            }

            return string.Join($" {Operators.And} ", predicates.Select(p =>
            {
                string wrap = info.IsNonNullGraphType ? p.propertyPath : $"np({p.propertyPath})";
                return $"({wrap} {p.@operator} \"{p.propertyValue}\")";
            }));
        }
    }
}