namespace GraphQL.EntityFrameworkCore.DynamicLinq.Options
{
    public class QueryArgumentInfoListBuilderOptions
    {
        public int MaxRecursionLevel { get; set; } = 2;

        public bool SupportListGraphType { get; set; }
    }
}