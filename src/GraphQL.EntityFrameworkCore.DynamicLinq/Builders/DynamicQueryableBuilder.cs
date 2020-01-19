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

            bool anyIsNullable = info.EntityPath.Any(ep => ep.IsNullable);

            string propertyPath = string.Join(".", info.EntityPath.Select(ep => ep.Path));

            string propertyPathNullPropagation = anyIsNullable ? $"np({propertyPath})" : propertyPath;

            string text = string.Join($" {Operators.And} ", predicates.Select(p => $"{propertyPathNullPropagation} {p.@operator} {p.placeHolder}"));

            return (text, values.ToArray());

            // Value       = "abc"
            // EntityPath  = Rooms.Reservation.Extras.Test.A
            // Linq        = Rooms.Any(r => r.Reservation.Extras.Any(e => e.Test == "abc"))
            // DynamicLinq = Rooms.Any(Reservation.Extras.Any(Test.A == @0))


            //int listGraphCount = info.EntityPath.Count(ep => ep.IsListGraphType);

            //var lastListGraphType = info.EntityPath
            //    .Select((ep, i) => (ep, i))
            //    .LastOrDefault(x => x.ep.IsListGraphType);

            //var list = new List<string>();
            //string propertyPath = null;
            //foreach (var entityPath in info.EntityPath)
            //{
            //    if (entityPath.IsListGraphType)
            //    {
            //        list.Add($"{entityPath.Path}.Any(");

            //        if (entityPath == lastListGraphType.ep)
            //        {
            //            var lastParts = info.EntityPath.Skip(lastListGraphType.i + 1).ToList();
            //            string pp = string.Join(".", lastParts.Select(lp => lp.Path));
            //            // propertyPath = lastParts.Last().GraphType.IsNullable() ? $"np({pp})" : pp;
            //            propertyPath = lastParts.Last().IsNullable ? $"np({pp})" : pp;
            //            break;
            //        }
            //    }
            //    else
            //    {
            //        list.Add($"{entityPath.Path}.");
            //    }
            //}

            //if (propertyPath == null)
            //{
            //    propertyPath = $"{string.Join("", list)}".TrimEnd('.');
            //    list.Clear();
            //}

            //string predicateText = string.Join($" {Operators.And} ", predicates.Select(p => $"{propertyPath} {p.@operator} {p.placeHolder}"));
            //string parentheses = new string(')', listGraphCount);

            //string text = $"{string.Join("", list)}{predicateText}{parentheses}";

            //return (text, values.ToArray());
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
}