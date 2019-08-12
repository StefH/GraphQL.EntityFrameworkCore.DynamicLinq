using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Text.RegularExpressions;
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
        private static string SortOrderAsc = "asc";
        private static string SortOrderDesc = "desc";

        private readonly Regex _orderByRegularExpression = new Regex(@"\w+", RegexOptions.Compiled);

        private readonly IQueryable<T> _queryable;
        private readonly QueryArgumentInfoList _list;
        private readonly ResolveFieldContext<TGraphQL> _context;

        private static bool IsSortOrder(string value) => string.Equals(value, SortOrderAsc, StringComparison.OrdinalIgnoreCase) || string.Equals(value, SortOrderDesc, StringComparison.OrdinalIgnoreCase);

        private static bool IsOrderBy(string argumentName) => string.Equals(argumentName, FieldNames.OrderByFieldName, StringComparison.OrdinalIgnoreCase);

        private static bool IsPaging(string argumentName) => string.Equals(argumentName, FieldNames.PageFieldName, StringComparison.OrdinalIgnoreCase) || string.Equals(argumentName, FieldNames.PageSizeFieldName, StringComparison.OrdinalIgnoreCase);

        public DynamicQueryableBuilder([NotNull] IQueryable<T> queryable, [NotNull] QueryArgumentInfoList list, [NotNull] ResolveFieldContext<TGraphQL> context)
        {
            Guard.NotNull(queryable, nameof(queryable));
            Guard.HasNoNulls(list, nameof(list));
            Guard.NotNull(context, nameof(context));

            _queryable = queryable;
            _context = context;
            _list = list;
        }

        public IQueryable<T> Build()
        {
            if (_context.Arguments == null)
            {
                return _queryable;
            }

            try
            {
                var queryableWhere = ApplyWhere(_queryable);

                var queryableOrderBy = ApplyOrderBy(queryableWhere);

                return ApplyPaging(queryableOrderBy);
            }
            catch (Exception e)
            {
                _context.Errors.Add(new DynamicQueryableError(e.Message, e));
            }

            // In case of an error, just return empty Queryable.
            return Enumerable.Empty<T>().AsQueryable();
        }

        private IQueryable<T> ApplyPaging(IQueryable<T> queryable)
        {
            if (_list.HasPaging)
            {
                var page = _context.GetArgument<int?>(FieldNames.PageFieldName);
                var pageSize = _context.GetArgument<int?>(FieldNames.PageSizeFieldName);
                if (page.HasValue && pageSize.HasValue)
                {
                    queryable = queryable.Page(page.Value, pageSize.Value);
                }
            }

            return queryable;
        }

        private IQueryable<T> ApplyWhere(IQueryable<T> queryable)
        {
            var wherePredicates = _context.Arguments
                .Where(argument => !IsPaging(argument.Key) && !IsOrderBy(argument.Key))
                .Select(argument =>
                {
                    var filterQueryArgumentInfo = GetQueryArgumentInfo(argument.Key);
                    return BuildPredicate(filterQueryArgumentInfo, argument.Value);
                }).ToArray();

            return wherePredicates.Any() ? queryable.Where(string.Join($" {Operators.And} ", wherePredicates)) : queryable;
        }

        private IQueryable<T> ApplyOrderBy(IQueryable<T> queryable)
        {
            if (_list.HasOrderBy && TryGetOrderBy(_context.GetArgument<string>(FieldNames.OrderByFieldName), out string orderByStatement))
            {
                var orderByItems = new List<(string value, QueryArgumentInfoType type, int index)>();
                ApplyOrderBy(orderByItems, orderByStatement);

                if (orderByItems.Any())
                {
                    var stringBuilder = new StringBuilder();
                    foreach (var orderByItem in orderByItems)
                    {
                        stringBuilder.AppendFormat("{0}{1}",
                            orderByItem.type == QueryArgumentInfoType.DefaultGraphQL ? ',' : ' ', orderByItem.value);
                    }

                    queryable = queryable.OrderBy(stringBuilder.ToString().TrimStart(','));
                }
            }

            return queryable;
        }

        private bool TryGetOrderBy(object argumentValue, out string orderByStatement)
        {
            orderByStatement = null;

            string orderByAsString = argumentValue.ToString();
            if (string.IsNullOrWhiteSpace(orderByAsString))
            {
                throw new ArgumentException($"The \"{FieldNames.OrderByFieldName}\" field is empty.");
            }

            orderByStatement = orderByAsString;
            return true;
        }

        private void ApplyOrderBy(ICollection<(string value, QueryArgumentInfoType type, int index)> orderByItems, string orderByStatement)
        {
            int index = 0;
            foreach (Match match in _orderByRegularExpression.Matches(orderByStatement))
            {
                if (IsSortOrder(match.Value))
                {
                    int orderByIndex = index - 1;
                    if (orderByItems.Count(o => o.type == QueryArgumentInfoType.DefaultGraphQL && o.index == orderByIndex) == 0)
                    {
                        throw new ArgumentException($"The \"{QueryArgumentInfoType.OrderBy}\" field with value \"{match.Value}\" cannot be used without a query field.");
                    }

                    orderByItems.Add((match.Value.ToLower(), QueryArgumentInfoType.OrderBy, orderByIndex));
                }
                else
                {
                    var queryArgumentInfo = _list.FirstOrDefault(l => match.Value.Equals(l.GraphQLPath, StringComparison.OrdinalIgnoreCase));
                    if (queryArgumentInfo == null)
                    {
                        throw new ArgumentException($"The \"{QueryArgumentInfoType.OrderBy}\" field uses an unknown field \"{match.Value}\".");
                    }

                    orderByItems.Add((queryArgumentInfo.EntityPath, QueryArgumentInfoType.DefaultGraphQL, index));
                }

                index++;
            }
        }

        private QueryArgumentInfo GetQueryArgumentInfo(string argumentName)
        {
            var queryArgumentInfo = _list.FirstOrDefault(i => i.QueryArgumentInfoType == QueryArgumentInfoType.DefaultGraphQL && i.QueryArgument.Name == argumentName);
            if (queryArgumentInfo != null)
            {
                return queryArgumentInfo;
            }

            // TODO: never thrown?
            throw new ArgumentException($"Unknown argument \"{argumentName}\" on type \"{typeof(T)}\".");
        }

        private string BuildPredicate(QueryArgumentInfo info, object value)
        {
            var predicates = new List<(string propertyPath, string @operator, object propertyValue)>();
            if (info.QueryArgument.Type == typeof(DateGraphType) && value is DateTime date)
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