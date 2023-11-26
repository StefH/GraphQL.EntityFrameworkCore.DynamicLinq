using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.EntityFrameworkCore.DynamicLinq.Enums;
using GraphQL.EntityFrameworkCore.DynamicLinq.Extensions;
using GraphQL.EntityFrameworkCore.DynamicLinq.Models;
using GraphQL.EntityFrameworkCore.DynamicLinq.Options;
using GraphQL.EntityFrameworkCore.DynamicLinq.Resolvers;
using GraphQL.Types;
using Microsoft.Extensions.Options;
using Stef.Validation;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Builders;

internal class QueryArgumentInfoListBuilder : IQueryArgumentInfoListBuilder
{
    private readonly QueryArgumentInfoListBuilderOptions _options;
    private readonly IPropertyPathResolver _propertyPathResolver;

    public QueryArgumentInfoListBuilder(IOptions<QueryArgumentInfoListBuilderOptions> options, IPropertyPathResolver propertyPathResolver)
    {
        _options = Guard.NotNull(options).Value;
        _propertyPathResolver = Guard.NotNull(propertyPathResolver);
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

        return PopulateQueryArgumentInfoList(null, graphQLType, string.Empty, new List<string>(), 0);
    }

    private QueryArgumentInfoList PopulateQueryArgumentInfoList(Type? parentGraphType, Type graphQLType,
        string parentGraphQLPath, IReadOnlyCollection<string> parentEntityPath, int level)
    {
        var list = new QueryArgumentInfoList();
        if (level > _options.MaxRecursionLevel || Activator.CreateInstance(graphQLType) is not IComplexGraphType complexGraphQLInstance)
        {
            return list;
        }

        foreach (var ft in complexGraphQLInstance.Fields)
        {
            var graphPath = $"{parentGraphQLPath}{ft.Name}";

            var thisModel = graphQLType.ModelType();
            string resolvedParentEntityPath = _propertyPathResolver.Resolve(thisModel, ft.Name);

            var entityPath = new List<string>(parentEntityPath) { resolvedParentEntityPath };

            bool isNonNullGraphType = ft.Type.IsNonNullGraphType();
            var childGraphQLType = ft.Type?.GraphType();
            if (childGraphQLType.IsObjectGraphType())
            {
                list.AddRange(PopulateQueryArgumentInfoList(parentGraphType, childGraphQLType!, graphPath, entityPath, level + 1));
            }
            else if (childGraphQLType.IsListGraphType() && _options.SupportListGraphType)
            {
                var genericType = childGraphQLType?.GenericTypeArguments.FirstOrDefault();
                list.AddRange(PopulateQueryArgumentInfoList(childGraphQLType, genericType, graphPath, entityPath, level + 1));
            }
            else
            {
                list.Add(new QueryArgumentInfo
                {
                    ParentGraphType = parentGraphType,
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