using System;
using GraphQL.EntityFrameworkCore.DynamicLinq.Models;
using JetBrains.Annotations;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Builders;

[PublicAPI]
public interface IQueryArgumentInfoListBuilder
{
    /// <summary>
    /// Builds a QueryArgumentInfoList for the generic type {T}.
    /// </summary>
    /// <typeparam name="T">The generic type (must be a ObjectGraphType or ObjectGraphType`1)</typeparam>
    QueryArgumentInfoList Build<T>();

    /// <summary>
    /// Builds a QueryArgumentInfoList for the specified type.
    /// </summary>
    /// <param name="graphQLType">The type (must be a ObjectGraphType or ObjectGraphType`1)</param>
    QueryArgumentInfoList Build(Type graphQLType);
}