namespace GraphQL.EntityFrameworkCore.DynamicLinq.Matchers;

/// <summary>
/// IIgnoreCaseMatcher
/// </summary>
/// <inheritdoc cref="IMatcher"/>
internal interface IIgnoreCaseMatcher : IMatcher
{
    /// <summary>
    /// Ignore the case from the pattern.
    /// </summary>
    bool IgnoreCase { get; }
}