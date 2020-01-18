using System;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Models
{
    public class EntityPath
    {
        public bool IsListGraphType { get; set; }
        //public Type? ParentGraphType { get; set; }

        public Type GraphType { get; set; }

        public string Path { get; set; }
    }
}