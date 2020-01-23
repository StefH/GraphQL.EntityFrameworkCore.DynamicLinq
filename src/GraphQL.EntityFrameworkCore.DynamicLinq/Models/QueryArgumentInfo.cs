using System;
using System.Collections.Generic;
using GraphQL.EntityFrameworkCore.DynamicLinq.Enums;
using GraphQL.Types;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Models
{
    public class QueryArgumentInfo
    {
        // public Type? ParentGraphType { get; internal set; }

        public QueryArgumentInfoType QueryArgumentInfoType { get; set; }

        public QueryArgument? QueryArgument { get; set; }

        public bool IsNonNullGraphType { get; set; }

        public string? GraphQLPath { get; set; }

        public List<EntityPath> EntityPath { get; set; } = new List<EntityPath>();
    }
}