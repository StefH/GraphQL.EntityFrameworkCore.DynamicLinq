using System.Linq;
using System.Text.RegularExpressions;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Matchers
{
    /// <summary>
    /// WildcardMatcher
    /// </summary>
    /// <seealso cref="RegexMatcher" />
    internal class WildcardMatcher : RegexMatcher
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WildcardMatcher"/> class.
        /// </summary>
        /// <param name="matchBehaviour">The match behaviour.</param>
        /// <param name="patterns">The patterns.</param>
        /// <param name="ignoreCase">IgnoreCase</param>
        public WildcardMatcher(MatchBehaviour matchBehaviour, string[] patterns, bool ignoreCase = false) : base(matchBehaviour, patterns.Select(pattern => "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$").ToArray(), ignoreCase)
        {
        }

        /// <inheritdoc cref="IMatcher.Name"/>
        public override string Name => "WildcardMatcher";
    }
}