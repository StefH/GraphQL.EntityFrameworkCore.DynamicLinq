using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.EntityFrameworkCore.DynamicLinq.Constants;
using GraphQL.EntityFrameworkCore.DynamicLinq.Enums;
using GraphQL.EntityFrameworkCore.DynamicLinq.Validation;
using GraphQL.Types;
using JetBrains.Annotations;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Models
{
    public class QueryArgumentInfoList : List<QueryArgumentInfo>
    {
        #region QueryArgumentInfo Fields
        private static readonly QueryArgumentInfo OrderByQueryArgumentInfo = new QueryArgumentInfo
        {
            QueryArgument = new QueryArgument(typeof(StringGraphType))
            {
                Name = FieldNames.OrderByFieldName,
                Description = "Sorts the elements of a sequence in ascending or descending order according to a key."
            },
            QueryArgumentInfoType = QueryArgumentInfoType.OrderBy
        };

        private static readonly QueryArgumentInfo PageQueryArgumentInfo = new QueryArgumentInfo
        {
            QueryArgument = new QueryArgument(typeof(IntGraphType))
            {
                Name = FieldNames.PageFieldName,
                Description = "The page to return."
            },
            QueryArgumentInfoType = QueryArgumentInfoType.Page
        };

        private static readonly QueryArgumentInfo PageSizeQueryArgumentInfo = new QueryArgumentInfo
        {
            QueryArgument = new QueryArgument(typeof(IntGraphType))
            {
                Name = FieldNames.PageSizeFieldName,
                Description = "The number of elements per page."
            },
            QueryArgumentInfoType = QueryArgumentInfoType.PageSize
        };
        #endregion

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
        public bool HasOrderBy => this.Any(x => x.QueryArgumentInfoType == QueryArgumentInfoType.OrderBy);

        [PublicAPI]
        public bool HasPaging => this.Any(x => x.QueryArgumentInfoType == QueryArgumentInfoType.Page) && this.Any(x => x.QueryArgumentInfoType == QueryArgumentInfoType.PageSize);

        [PublicAPI]
        public QueryArgumentInfoList SupportOrderBy()
        {
            if (!HasOrderBy)
            {
                Add(OrderByQueryArgumentInfo);
            }

            return this;
        }

        [PublicAPI]
        public QueryArgumentInfoList SupportPaging()
        {
            if (!HasPaging)
            {
                Add(PageQueryArgumentInfo);
                Add(PageSizeQueryArgumentInfo);
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