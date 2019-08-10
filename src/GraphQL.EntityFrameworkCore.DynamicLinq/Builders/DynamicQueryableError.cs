using System;
using System.Collections;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Builders
{
    internal class DynamicQueryableError : ExecutionError
    {
        public DynamicQueryableError(string message) : base(message)
        {
        }

        public DynamicQueryableError(string message, IDictionary data) : base(message, data)
        {
        }

        public DynamicQueryableError(string message, Exception exception) : base(message, exception)
        {
        }
    }
}