namespace GraphQL.EntityFrameworkCore.DynamicLinq.Matchers
{
    /// <summary>
    /// IMatcher
    /// </summary>
    internal interface IMatcher
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the match behaviour.
        /// </summary>
        MatchBehaviour MatchBehaviour { get; }
    }
}