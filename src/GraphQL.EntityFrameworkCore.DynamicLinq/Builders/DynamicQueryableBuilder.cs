using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Text.RegularExpressions;
using GraphQL.EntityFrameworkCore.DynamicLinq.Enumerations;
using GraphQL.EntityFrameworkCore.DynamicLinq.Models;
using GraphQL.EntityFrameworkCore.DynamicLinq.Validation;
using GraphQL.Types;
using JetBrains.Annotations;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Builders
{
    internal class DynamicQueryableBuilder<T, TGraphQL>
    {
        private const string SortOrderAsc = "asc";
        private const string SortOrderDesc = "desc";

        private readonly Regex _orderByRegularExpression = new Regex(@"\w+", RegexOptions.Compiled);

        private readonly IQueryable<T> _query;
        private readonly QueryArgumentInfoList _list;
        private readonly ResolveFieldContext<TGraphQL> _context;

        private static bool IsSortOrder(string value) => string.Equals(value, SortOrderAsc, StringComparison.OrdinalIgnoreCase) || string.Equals(value, SortOrderDesc, StringComparison.OrdinalIgnoreCase);

        public DynamicQueryableBuilder([NotNull] IQueryable<T> query, [NotNull] QueryArgumentInfoList list, [NotNull] ResolveFieldContext<TGraphQL> context)
        {
            Guard.NotNull(query, nameof(query));
            Guard.HasNoNulls(list, nameof(list));
            Guard.NotNull(context, nameof(context));

            _query = query;
            _context = context;
            _list = list;
        }

        public IQueryable<T> Build()
        {
            if (_context.Arguments == null)
            {
                return _query;
            }

            var newQuery = _query;
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
                        newQuery = newQuery.Where(BuildPredicate(filterQueryArgumentInfo, argument.Value));
                    }
                }

                if (orderByItems.Any())
                {
                    var stringBuilder = new StringBuilder();
                    foreach (var orderByItem in orderByItems)
                    {
                        stringBuilder.AppendFormat("{0}{1}", orderByItem.type == QueryArgumentInfoType.DefaultGraphQL ? ',' : ' ', orderByItem.value);
                    }

                    newQuery = newQuery.OrderBy(stringBuilder.ToString().TrimStart(','));
                }
            }
            catch (Exception e)
            {
                _context.Errors.Add(new DynamicQueryableError(e.Message));

                // In case of an error, just return empty Queryable.
                return Enumerable.Empty<T>().AsQueryable();
            }

            return newQuery;
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
            if (info.IsNonNullGraphType)
            {
                return $"{info.EntityPath} == \"{value}\"";
            }

            if (info.QueryArgument.Type == typeof(DateGraphType) && value is DateTime date)
            {
                return $"np({info.EntityPath}) >= \"{date}\" && np({info.EntityPath}) < \"{date.AddDays(1)}\"";
            }

            return $"np({info.EntityPath}) == \"{value}\"";
        }
    }
}