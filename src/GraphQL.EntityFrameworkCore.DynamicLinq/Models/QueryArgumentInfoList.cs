using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.EntityFrameworkCore.DynamicLinq.Constants;
using GraphQL.EntityFrameworkCore.DynamicLinq.Enums;
using GraphQL.EntityFrameworkCore.DynamicLinq.Matchers;
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

        internal QueryArgumentInfoList FilterBy(QueryArgumentInfoType type)
        {
            return new QueryArgumentInfoList(this.Where(q => q.QueryArgumentInfoType == type));
        }

        [PublicAPI]
        public bool HasOrderBy => FilterBy(QueryArgumentInfoType.OrderBy).Count > 0;

        [PublicAPI]
        public bool HasPaging => FilterBy(QueryArgumentInfoType.Paging).Count > 0;

        /// <summary>
        /// Converts this object into a <see cref="QueryArguments"/> object.
        /// </summary>
        [PublicAPI]
        public QueryArguments ToQueryArguments()
        {
            return new QueryArguments(this.Select(info => info.QueryArgument));
        }

        [PublicAPI]
        public QueryArgumentInfoList SupportOrderBy([CanBeNull] string orderByArgumentName = null)
        {
            if (!HasOrderBy)
            {
                Add(new QueryArgumentInfo
                {
                    QueryArgument = new QueryArgument(typeof(StringGraphType))
                    {
                        Name = string.IsNullOrEmpty(orderByArgumentName) ? FieldNames.OrderByFieldName : orderByArgumentName,
                        Description = "Sorts the elements of a sequence in ascending or descending order according to a key."
                    },
                    QueryArgumentInfoType = QueryArgumentInfoType.OrderBy
                });
            }

            return this;
        }

        [PublicAPI]
        public QueryArgumentInfoList SupportPaging([CanBeNull] string pageArgumentName = null, [CanBeNull] string pageSizeArgumentName = null)
        {
            if (!HasPaging)
            {
                Add(new QueryArgumentInfo
                {
                    QueryArgument = new QueryArgument(typeof(IntGraphType))
                    {
                        Name = string.IsNullOrEmpty(pageArgumentName) ? FieldNames.PageFieldName : pageArgumentName,
                        Description = "The page to return."
                    },
                    QueryArgumentInfoType = QueryArgumentInfoType.Paging
                });

                Add(new QueryArgumentInfo
                {
                    QueryArgument = new QueryArgument(typeof(IntGraphType))
                    {
                        Name = string.IsNullOrEmpty(pageSizeArgumentName) ? FieldNames.PageSizeFieldName : pageSizeArgumentName,
                        Description = "The number of elements per page."
                    },
                    QueryArgumentInfoType = QueryArgumentInfoType.Paging
                });
            }

            return this;
        }

        [PublicAPI]
        public QueryArgumentInfoList Include([NotNull] params string[] includedGraphQLPropertyPaths)
        {
            Guard.HasNoNulls(includedGraphQLPropertyPaths, nameof(includedGraphQLPropertyPaths));

            var matcher = new WildcardMatcher(MatchBehaviour.AcceptOnMatch, includedGraphQLPropertyPaths);

            return new QueryArgumentInfoList(this.Where(q => MatchScores.IsPerfect(matcher.IsMatch(q.GraphQLPath))));
        }

        [PublicAPI]
        public QueryArgumentInfoList Exclude([NotNull] params string[] excludedGraphQLPropertyPaths)
        {
            Guard.HasNoNulls(excludedGraphQLPropertyPaths, nameof(excludedGraphQLPropertyPaths));

            var matcher = new WildcardMatcher(MatchBehaviour.RejectOnMatch, excludedGraphQLPropertyPaths);

            return new QueryArgumentInfoList(this.Where(q => MatchScores.IsPerfect(matcher.IsMatch(q.GraphQLPath))));
        }

        [PublicAPI]
        public QueryArgumentInfoList Filter([NotNull] Predicate<string> predicate)
        {
            Guard.NotNull(predicate, nameof(predicate));

            return new QueryArgumentInfoList(this.Where(q => predicate(q.GraphQLPath)));
        }
    }
}