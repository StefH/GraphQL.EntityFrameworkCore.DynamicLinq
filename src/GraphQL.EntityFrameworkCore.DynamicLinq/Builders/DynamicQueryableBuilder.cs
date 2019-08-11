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

            var newQueryable = _queryable;
            try
            {
                var orderByItems = new List<(string value, QueryArgumentInfoType type, int index)>();

                foreach (var argument in _context.Arguments)
                {
                    if (TryGetOrderBy(argument.Key, argument.Value, out string orderByStatement))
                    {
                        ApplyOrderBy(orderByItems, orderByStatement);
                    }
                    else
                    {
                        var filterQueryArgumentInfo = GetQueryArgumentInfo(argument.Key);
                        newQueryable = newQueryable.Where(BuildPredicate(filterQueryArgumentInfo, argument.Value));
                    }
                }

                if (orderByItems.Any())
                {
                    var stringBuilder = new StringBuilder();
                    foreach (var orderByItem in orderByItems)
                    {
                        stringBuilder.AppendFormat("{0}{1}", orderByItem.type == QueryArgumentInfoType.DefaultGraphQL ? ',' : ' ', orderByItem.value);
                    }

                    newQueryable = newQueryable.OrderBy(stringBuilder.ToString().TrimStart(','));
                }
            }
            catch (Exception e)
            {
                _context.Errors.Add(new DynamicQueryableError(e.Message, e));

                // In case of an error, just return empty Queryable.
                return Enumerable.Empty<T>().AsQueryable();
            }

            return newQueryable;
        }

        private bool TryGetOrderBy(string argumentName, object argumentValue, out string orderByStatement)
        {
            orderByStatement = null;
            var orderByQueryArgumentInfo = _list.FirstOrDefault(info => info.QueryArgumentInfoType == QueryArgumentInfoType.OrderBy && info.QueryArgument.Name == argumentName);
            if (orderByQueryArgumentInfo == null)
            {
                return false;
            }

            string orderByAsString = argumentValue.ToString();
            if (string.IsNullOrWhiteSpace(orderByAsString))
            {
                throw new ArgumentException($"The \"{argumentName}\" field is empty.");
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
            throw new ArgumentException($"Unknown argument \"{argumentName}\" on field \"{_context.FieldName}\" of type \"{_context.Operation.Name}\".");
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
                return $"{wrap} {p.@operator} \"{p.propertyValue}\"";
            }));
        }
    }
}