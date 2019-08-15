using System;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Resolvers
{
    /// <summary>
    /// Default implementation which just returns the same property path.
    /// </summary>
    /// <seealso cref="IPropertyPathResolver" />
    internal class DefaultPropertyPathResolver : IPropertyPathResolver
    {
        /// <inheritdoc cref="IPropertyPathResolver.Resolve(Type, string, Type)" />
        public string Resolve(Type sourceType, string sourcePropertyPath, Type? destinationType)
        {
            return sourcePropertyPath;
        }
    }
}