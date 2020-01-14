using System;
using System.Linq;
using GraphQL.EntityFrameworkCore.DynamicLinq.Enums;
using GraphQL.EntityFrameworkCore.DynamicLinq.Extensions;
using GraphQL.EntityFrameworkCore.DynamicLinq.Models;
using GraphQL.EntityFrameworkCore.DynamicLinq.Options;
using GraphQL.EntityFrameworkCore.DynamicLinq.Resolvers;
using GraphQL.EntityFrameworkCore.DynamicLinq.Validation;
using GraphQL.Types;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Builders
{
    internal class QueryArgumentInfoListBuilder : IQueryArgumentInfoListBuilder
    {
        private readonly QueryArgumentInfoListBuilderOptions _options;
        private readonly IPropertyPathResolver _propertyPathResolver;

        public QueryArgumentInfoListBuilder([NotNull] IOptions<QueryArgumentInfoListBuilderOptions> options, [NotNull] IPropertyPathResolver propertyPathResolver)
        {
            Guard.NotNull(options, nameof(options));
            Guard.NotNull(propertyPathResolver, nameof(propertyPathResolver));

            _propertyPathResolver = propertyPathResolver;
            _options = options.Value;
        }

        /// <inheritdoc cref="IQueryArgumentInfoListBuilder.Build{T}" />
        public QueryArgumentInfoList Build<T>()
        {
            return Build(typeof(T));
        }

        /// <inheritdoc cref="IQueryArgumentInfoListBuilder.Build(Type)" />
        public QueryArgumentInfoList Build(Type graphQLType)
        {
            Guard.NotNull(graphQLType, nameof(graphQLType));

            return PopulateQueryArgumentInfoList(graphQLType, string.Empty, string.Empty, 0);
        }

        private QueryArgumentInfoList PopulateQueryArgumentInfoList(Type graphQLType, string parentGraphQLPath, string parentEntityPath, int level)
        {
            var list = new QueryArgumentInfoList();
            if (level > _options.MaxRecursionLevel || !(Activator.CreateInstance(graphQLType) is IComplexGraphType complexGraphQLInstance))
            {
                return list;
            }

            foreach (var ft in complexGraphQLInstance.Fields)
            {
                string graphPath = $"{parentGraphQLPath}{ft.Name}";

                Type thisModel = graphQLType.ModelType();
                string resolvedParentEntityPath = _propertyPathResolver.Resolve(thisModel, ft.Name);
                string entityPath = !string.IsNullOrEmpty(parentEntityPath) ? $"{parentEntityPath}.{resolvedParentEntityPath}" : resolvedParentEntityPath;

                bool isNonNullGraphType = ft.Type.IsNonNullGraphType();
                Type childGraphQLType = ft.Type.GraphType();
                if (childGraphQLType.IsObjectGraphType())
                {
                    list.AddRange(PopulateQueryArgumentInfoList(childGraphQLType, graphPath, entityPath, level + 1));
                }
                else if (childGraphQLType.IsListGraphType())
                {
                    // var genericType = childGraphQLType.GenericTypeArguments.First();
                    // list.AddRange(PopulateQueryArgumentInfoList(genericType, graphPath, entityPath, level + 1));
                }
                else
                {
                    list.Add(new QueryArgumentInfo
                    {
                        QueryArgument = new QueryArgument(childGraphQLType) { Name = graphPath },
                        GraphQLPath = graphPath,
                        EntityPath = entityPath,
                        IsNonNullGraphType = isNonNullGraphType,
                        QueryArgumentInfoType = QueryArgumentInfoType.GraphQL
                    });
                }
            }

            return list;
        }
    }
}