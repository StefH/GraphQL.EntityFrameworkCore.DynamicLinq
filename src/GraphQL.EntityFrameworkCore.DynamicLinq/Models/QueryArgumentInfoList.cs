using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.EntityFrameworkCore.DynamicLinq.Constants;
using GraphQL.EntityFrameworkCore.DynamicLinq.Enumerations;
using GraphQL.EntityFrameworkCore.DynamicLinq.Validation;
using GraphQL.Types;
using JetBrains.Annotations;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Models
{
    public class QueryArgumentInfoList : List<QueryArgumentInfo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryArgumentInfoList"/> class.
        /// </summary>
        public QueryArgumentInfoList()
        {
        }

        private QueryArgumentInfoList(IEnumerable<QueryArgumentInfo> collection)
        {
            AddRange(collection);
        }

        [PublicAPI]
        public QueryArgumentInfoList SupportOrderBy()
        {
            if (this.All(x => x.QueryArgumentInfoType != QueryArgumentInfoType.OrderBy))
            {
                var orderBy = new QueryArgumentInfo
                {
                    QueryArgument = new QueryArgument(typeof(StringGraphType))
                    {
                        Name = FieldNames.OrderByFieldName,
                        Description = "Sorts the elements of a sequence in ascending or descending order according to a key."
                    },
                    QueryArgumentInfoType = QueryArgumentInfoType.OrderBy
                };

                Add(orderBy);
            }

            return this;
        }

        [PublicAPI]
        public QueryArgumentInfoList Include([NotNull] params string[] includedGraphQLPropertyPaths)
        {
            Guard.HasNoNulls(includedGraphQLPropertyPaths, nameof(includedGraphQLPropertyPaths));

            return new QueryArgumentInfoList(this.Where(q => includedGraphQLPropertyPaths.Contains(q.GraphQLPath)));
        }

        [PublicAPI]
        public QueryArgumentInfoList Exclude([NotNull] params string[] excludedGraphQLPropertyPaths)
        {
            Guard.HasNoNulls(excludedGraphQLPropertyPaths, nameof(excludedGraphQLPropertyPaths));

            return new QueryArgumentInfoList(this.Where(q => !excludedGraphQLPropertyPaths.Contains(q.GraphQLPath)));
        }

        [PublicAPI]
        public QueryArgumentInfoList Filter([NotNull] Predicate<string> predicate)
        {
            Guard.NotNull(predicate, nameof(predicate));

            return new QueryArgumentInfoList(this.Where(q => predicate(q.GraphQLPath)));
        }
    }
}