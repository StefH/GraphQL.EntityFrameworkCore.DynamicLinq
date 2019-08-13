using System;
using System.Linq;
using GraphQL.EntityFrameworkCore.DynamicLinq.Enums;
using GraphQL.EntityFrameworkCore.DynamicLinq.Extensions;
using GraphQL.EntityFrameworkCore.DynamicLinq.Models;
using GraphQL.EntityFrameworkCore.DynamicLinq.Resolvers;
using GraphQL.EntityFrameworkCore.DynamicLinq.Validation;
using GraphQL.Types;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Builders
{
    internal class QueryArgumentInfoListBuilder : IQueryArgumentInfoListBuilder
    {
        private readonly IPropertyPathResolver _propertyPathResolver;

        public QueryArgumentInfoListBuilder(IPropertyPathResolver propertyPathResolver)
        {
            Guard.NotNull(propertyPathResolver, nameof(propertyPathResolver));

            _propertyPathResolver = propertyPathResolver;
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

            return PopulateQueryArgumentInfoList(graphQLType, string.Empty, string.Empty);
        }

        private QueryArgumentInfoList PopulateQueryArgumentInfoList(Type graphQLType, string parentGraphQLPath, string parentEntityPath)
        {
            var list = new QueryArgumentInfoList();
            if (!(Activator.CreateInstance(graphQLType) is IComplexGraphType complexGraphQLInstance))
            {
                return list;
            }

            complexGraphQLInstance.Fields.ToList().ForEach(ft =>
            {
                string graphPath = $"{parentGraphQLPath}{ft.Name}";

                Type thisModel = graphQLType.ModelType();
                string resolvedParentEntityPath = _propertyPathResolver.Resolve(thisModel, ft.Name);
                string entityPath = !string.IsNullOrEmpty(parentEntityPath) ? $"{parentEntityPath}.{resolvedParentEntityPath}" : resolvedParentEntityPath;

                bool isNonNullGraphType = ft.Type.IsNonNullGraphType();
                Type childGraphQLType = ft.Type.GraphType();
                if (childGraphQLType.IsObjectGraphType())
                {
                    list.AddRange(PopulateQueryArgumentInfoList(childGraphQLType, graphPath, entityPath));
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
            });

            return list;
        }
    }
}