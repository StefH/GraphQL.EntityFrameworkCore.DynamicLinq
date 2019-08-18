using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Text.RegularExpressions;
using GraphQL.EntityFrameworkCore.DynamicLinq.Constants;
using GraphQL.EntityFrameworkCore.DynamicLinq.Enums;
using GraphQL.EntityFrameworkCore.DynamicLinq.Models;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Builders
{
    internal class OrderByHelper<T> where T : notnull
    {
        private static string SortOrderAsc = "asc";
        private static string SortOrderDesc = "desc";

        private readonly Regex _orderByRegularExpression = new Regex(@"\w+", RegexOptions.Compiled);

        private readonly QueryArgumentInfoList _list;
        private readonly IDictionary<string, object> _arguments;

        private static bool IsSortOrder(string value) => string.Equals(value, SortOrderAsc, StringComparison.OrdinalIgnoreCase) || string.Equals(value, SortOrderDesc, StringComparison.OrdinalIgnoreCase);

        public OrderByHelper(QueryArgumentInfoList list, IDictionary<string, object> arguments)
        {
            _list = list;
            _arguments = arguments;
        }

        public IQueryable<T> ApplyOrderBy(IQueryable<T> queryable)
        {
            if (!_list.HasOrderBy)
            {
                return queryable;
            }

            var orderByStatement = TryGetOrderBy();
            if (orderByStatement != null)
            {
                var orderByItems = new List<(string value, QueryArgumentInfoType type, int index)>();
                AddOrderByToList(orderByItems, orderByStatement);

                if (orderByItems.Any())
                {
                    var stringBuilder = new StringBuilder();
                    foreach (var orderByItem in orderByItems)
                    {
                        stringBuilder.AppendFormat("{0}{1}", orderByItem.type == QueryArgumentInfoType.GraphQL ? ',' : ' ', orderByItem.value);
                    }

                    queryable = queryable.OrderBy(stringBuilder.ToString().TrimStart(','));
                }
            }

            return queryable;
        }

        private string? TryGetOrderBy()
        {
            if (!_arguments.TryGetValue(FieldNames.OrderByFieldName, out object value))
            {
                return null;
            }

            string orderByStatement = Convert.ToString(value);
            if (string.IsNullOrWhiteSpace(orderByStatement))
            {
                throw new ArgumentException($"The \"{FieldNames.OrderByFieldName}\" field is empty.");
            }

            return orderByStatement;
        }

        private void AddOrderByToList(ICollection<(string value, QueryArgumentInfoType type, int index)> orderByItems, string orderByStatement)
        {
            int index = 0;
            foreach (Match match in _orderByRegularExpression.Matches(orderByStatement))
            {
                if (IsSortOrder(match.Value))
                {
                    int orderByIndex = index - 1;
                    if (orderByItems.Count(o => o.type == QueryArgumentInfoType.GraphQL && o.index == orderByIndex) == 0)
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

                    if (queryArgumentInfo.EntityPath == null)
                    {
                        throw new ArgumentException($"The \"EntityPath\" for field \"{match.Value}\" is null.");
                    }

                    orderByItems.Add((queryArgumentInfo.EntityPath, QueryArgumentInfoType.GraphQL, index));
                }

                index++;
            }
        }
    }
}