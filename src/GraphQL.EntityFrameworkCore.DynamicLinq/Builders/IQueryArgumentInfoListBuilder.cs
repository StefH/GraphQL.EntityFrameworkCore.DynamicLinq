using System;
using GraphQL.EntityFrameworkCore.DynamicLinq.Models;
using JetBrains.Annotations;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Builders
{
    [PublicAPI]
    public interface IQueryArgumentInfoListBuilder
    {
        QueryArgumentInfoList Build<T>();

        QueryArgumentInfoList Build([NotNull] Type graphQLType);
    }
}