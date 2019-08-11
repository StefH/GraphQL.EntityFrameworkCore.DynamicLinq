using GraphQL.EntityFrameworkCore.DynamicLinq.Enums;
using GraphQL.Types;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Models
{
    public class QueryArgumentInfo
    {
        public QueryArgumentInfoType QueryArgumentInfoType { get; internal set; }

        public QueryArgument QueryArgument { get; internal set; }

        public bool IsNonNullGraphType { get; internal set; }

        public string GraphQLPath { get; internal set; }

        public string EntityPath { get; internal set; }
    }
}