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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Builders
{
    internal class QueryArgumentInfoListBuilder : IQueryArgumentInfoListBuilder
    {
        private readonly IOptions<QueryArgumentInfoListBuilderOptions> _options;
        private readonly IServiceProvider _serviceProvider;
        private readonly IPropertyPathResolver _propertyPathResolver;

        public QueryArgumentInfoListBuilder(
            [NotNull] IServiceProvider serviceProvider,
            [NotNull] IOptions<QueryArgumentInfoListBuilderOptions> options,
            [NotNull] IPropertyPathResolver propertyPathResolver)
        {
            Guard.NotNull(serviceProvider, nameof(serviceProvider));
            Guard.NotNull(options, nameof(options));
            Guard.NotNull(propertyPathResolver, nameof(propertyPathResolver));

            _serviceProvider = serviceProvider;
            _propertyPathResolver = propertyPathResolver;
            _options = options;
        }

        /// <inheritdoc cref="IQueryArgumentInfoListBuilder.Build{T}" />
        public QueryArgumentInfoList Build<T>()
        {
            return Build(typeof(T));
        }

        /// <inheritdoc cref="IQueryArgumentInfoListBuilder.Build" />
        public QueryArgumentInfoList Build(Type graphQLType)
        {
            Guard.NotNull(graphQLType, nameof(graphQLType));

            return PopulateQueryArgumentInfoList(graphQLType, string.Empty, new List<EntityPath>(), 0);
        }

        public QueryArgumentInfoList Build(FieldType field, Type thisModel)
        {
            Guard.NotNull(field, nameof(field));

            var list = new QueryArgumentInfoList();

            AddSingleField(thisModel, string.Empty, new List<EntityPath>(), field, list);

            return list;
        }

        private QueryArgumentInfoList PopulateQueryArgumentInfoList(Type graphQLType,
            string parentGraphQLPath, IReadOnlyCollection<EntityPath> parentEntityPath, int level)
        {
            var list = new QueryArgumentInfoList();
            //if (level > _options.Value.MaxRecursionLevel || graphQLType.GetInterface("IComplexGraphType") == null)
            //{
            //    return list;
            //}

            if (graphQLType.GetInterface("IComplexGraphType") == null)
            {
                return list;
            }

            //var complexGraphQLInstance = (IComplexGraphType)ActivatorUtilities.CreateInstance(_serviceProvider, graphQLType);
            var complexGraphQLInstance = (IComplexGraphType) _serviceProvider.GetService(graphQLType);
            foreach (var field in complexGraphQLInstance.Fields)
            {
                Type thisModel = graphQLType.ModelType();
                AddSingleField(thisModel, parentGraphQLPath, parentEntityPath, field, list);
            }

            return list;
        }

        private void AddSingleField(Type thisModel, string parentGraphQLPath, IReadOnlyCollection<EntityPath> parentEntityPath, FieldType field, QueryArgumentInfoList list)
        {
            string graphPath = $"{parentGraphQLPath}{field.Name}";

            bool isNonNullGraphType = field.Type.IsNonNullGraphType();
            Type fieldGraphQLType = field.Type.GraphType();
            bool fieldIsList = fieldGraphQLType.IsListGraphType();

            var resolvedParentEntityPath = new EntityPath
            {
                IsListGraphType = fieldIsList,
                IsNullable = fieldGraphQLType.IsNullable(),
                Path = _propertyPathResolver.Resolve(thisModel, field.Name)
            };

            var entityPath = new List<EntityPath>(parentEntityPath) { resolvedParentEntityPath };

            if (fieldGraphQLType.IsObjectGraphType())
            {
                //list.AddRange(PopulateQueryArgumentInfoList(fieldGraphQLType, graphPath, entityPath, level + 1));
            }
            else if (fieldIsList && _options.Value.SupportListGraphType)
            {
                //list.AddRange(PopulateQueryArgumentInfoList(fieldGraphQLType.GenericTypeArguments.First(), graphPath, entityPath, level + 1));
            }
            else
            {
                var q = new QueryArgumentInfo
                {
                    QueryArgument = new QueryArgument(fieldGraphQLType) { Name = graphPath },
                    GraphQLPath = graphPath,
                    EntityPath = entityPath,
                    IsNonNullGraphType = isNonNullGraphType,
                    QueryArgumentInfoType = QueryArgumentInfoType.GraphQL
                };
                list.Add(q);
            }
        }
    }
}