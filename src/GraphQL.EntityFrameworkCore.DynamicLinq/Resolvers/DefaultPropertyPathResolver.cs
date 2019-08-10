using System;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Resolvers
{
    internal class DefaultPropertyPathResolver : IPropertyPathResolver
    {
        public string Resolve(Type sourceType, string sourcePropertyPath, Type destinationType)
        {
            return sourcePropertyPath;
        }
    }
}