using System;
using System.Collections.Generic;
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
        private readonly IOptions<QueryArgumentInfoListBuilderOptions> _options;
        private readonly IPropertyPathResolver _propertyPathResolver;

        public QueryArgumentInfoListBuilder([NotNull] IOptions<QueryArgumentInfoListBuilderOptions> options, [NotNull] IPropertyPathResolver propertyPathResolver)
        {
            Guard.NotNull(options, nameof(options));
            Guard.NotNull(propertyPathResolver, nameof(propertyPathResolver));

            _propertyPathResolver = propertyPathResolver;
            _options = options;
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

            return PopulateQueryArgumentInfoList(graphQLType, string.Empty, new List<EntityPath>(), 0);
        }

        private QueryArgumentInfoList PopulateQueryArgumentInfoList(Type graphQLType,
            string parentGraphQLPath, IReadOnlyCollection<EntityPath> parentEntityPath, int level)
        {
            var list = new QueryArgumentInfoList();
            if (level > _options.Value.MaxRecursionLevel || graphQLType.GetInterface("IComplexGraphType") == null)
            {
                return list;
            }

            var complexGraphQLInstance = (IComplexGraphType)Activator.CreateInstance(graphQLType);
            foreach (var ft in complexGraphQLInstance.Fields)
            {
                string graphPath = $"{parentGraphQLPath}{ft.Name}";

                Type thisModel = graphQLType.ModelType();
                bool isNonNullGraphType = ft.Type.IsNonNullGraphType();
                Type fieldGraphQLType = ft.Type.GraphType();
                bool fieldIsList = fieldGraphQLType.IsListGraphType();

                var resolvedParentEntityPath = new EntityPath
                {
                    IsListGraphType = fieldIsList,
                    IsNullable = fieldGraphQLType.IsNullable(),
                    Path = _propertyPathResolver.Resolve(thisModel, ft.Name)
                };

                var entityPath = new List<EntityPath>(parentEntityPath) { resolvedParentEntityPath };

                if (fieldGraphQLType.IsObjectGraphType())
                {
                    list.AddRange(PopulateQueryArgumentInfoList(fieldGraphQLType, graphPath, entityPath, level + 1));
                }
                else if (fieldIsList && _options.Value.SupportListGraphType)
                {
                    list.AddRange(PopulateQueryArgumentInfoList(fieldGraphQLType.GenericTypeArguments.First(), graphPath, entityPath, level + 1));
                }
                else
                {
                    list.Add(new QueryArgumentInfo
                    {
                        QueryArgument = new QueryArgument(fieldGraphQLType) { Name = graphPath },
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