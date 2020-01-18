namespace GraphQL.EntityFrameworkCore.DynamicLinq.Models
{
    public class EntityPath
    {
        public bool IsListGraphType { get; set; }

        public bool IsNullable { get; set; }

        public string Path { get; set; }
    }
}