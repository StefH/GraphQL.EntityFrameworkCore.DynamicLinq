using System;
using JetBrains.Annotations;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Resolvers
{
    [PublicAPI]
    public interface IPropertyPathResolver
    {
        /// <summary>
        /// Resolves the specified source type + source property path into the destination path.
        /// </summary>
        /// <param name="sourceType">Type of the source.</param>
        /// <param name="sourcePropertyPath">The source property path.</param>
        /// <param name="destinationType">Type of the destination (optional).</param>
        string Resolve([NotNull] Type sourceType, [NotNull] string sourcePropertyPath, [CanBeNull] Type? destinationType = null);
    }
}