using System;
using System.Collections;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Builders
{
    internal class DynamicQueryableError : ExecutionError
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicQueryableError"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public DynamicQueryableError(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicQueryableError"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="data">The data.</param>
        public DynamicQueryableError(string message, IDictionary data) : base(message, data)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicQueryableError"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public DynamicQueryableError(string message, Exception exception) : base(message, exception)
        {
        }
    }
}