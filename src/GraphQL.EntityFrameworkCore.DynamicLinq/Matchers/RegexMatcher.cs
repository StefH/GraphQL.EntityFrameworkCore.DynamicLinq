using System;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Matchers
{
    /// <summary>
    /// Regular Expression Matcher
    /// </summary>
    /// <inheritdoc cref="IStringMatcher"/>
    /// <inheritdoc cref="IIgnoreCaseMatcher"/>
    internal class RegexMatcher : IStringMatcher, IIgnoreCaseMatcher
    {
        private readonly string[] _patterns;
        private readonly Regex[] _expressions;

        /// <inheritdoc cref="IMatcher.MatchBehaviour"/>
        public MatchBehaviour MatchBehaviour { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegexMatcher"/> class.
        /// </summary>
        /// <param name="matchBehaviour">The match behaviour.</param>
        /// <param name="patterns">The patterns.</param>
        /// <param name="ignoreCase">Ignore the case from the pattern.</param>
        public RegexMatcher(MatchBehaviour matchBehaviour, [RegexPattern] string[] patterns, bool ignoreCase = false)
        {
            _patterns = patterns;
            IgnoreCase = ignoreCase;
            MatchBehaviour = matchBehaviour;

            RegexOptions options = RegexOptions.Compiled;
            if (ignoreCase)
            {
                options |= RegexOptions.IgnoreCase;
            }

            _expressions = patterns.Select(p => new Regex(p, options)).ToArray();
        }

        /// <inheritdoc cref="IStringMatcher.IsMatch"/>
        public double IsMatch(string input)
        {
            double match = MatchScores.Mismatch;
            if (input != null)
            {
                try
                {
                    match = MatchScores.ToScore(_expressions.Select(e => e.IsMatch(input)));
                }
                catch (Exception)
                {
                    // just ignore exception
                }
            }
            
            return MatchBehaviourHelper.Convert(MatchBehaviour, match);
        }

        /// <inheritdoc cref="IMatcher.Name"/>
        public virtual string Name => "RegexMatcher";

        /// <inheritdoc cref="IIgnoreCaseMatcher.IgnoreCase"/>
        public bool IgnoreCase { get; }
    }
}