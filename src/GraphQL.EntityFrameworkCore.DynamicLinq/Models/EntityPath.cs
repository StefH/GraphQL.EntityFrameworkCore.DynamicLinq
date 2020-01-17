using System;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Models
{
    public class EntityPath
    {
        public Type? ParentGraphType { get; set; }

        public Type GraphType { get; set; }

        public string Path { get; set; }
    }
}