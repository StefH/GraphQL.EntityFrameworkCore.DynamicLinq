using System.Linq;
using GraphQL.EntityFrameworkCore.DynamicLinq.Builders;
using GraphQL.EntityFrameworkCore.DynamicLinq.Models;
using JetBrains.Annotations;
using Stef.Validation;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Extensions;

public static class QueryableExtensions
{
    [PublicAPI]
    public static IQueryable<T> ApplyQueryArguments<T>(this IQueryable<T> query, QueryArgumentInfoList list, ResolveFieldContext<object> context)
    {
        return ApplyQueryArguments<T, object>(query, list, context);
    }

    [PublicAPI]
    public static IQueryable<T> ApplyQueryArguments<T, TGraphQL>(this IQueryable<T> query, QueryArgumentInfoList list, ResolveFieldContext<TGraphQL> context)
    {
        Guard.NotNull(query);
        Guard.HasNoNulls(list);
        Guard.NotNull(context);

        return new DynamicQueryableBuilder<T, TGraphQL>(query, list, context).Build();
    }
}