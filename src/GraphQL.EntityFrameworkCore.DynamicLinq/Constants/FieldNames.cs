using GraphQL.EntityFrameworkCore.DynamicLinq.Enums;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Constants
{
    internal static class FieldNames
    {
        public static string OrderByFieldName = QueryArgumentInfoType.OrderBy.ToString();

        public static string PageFieldName = QueryArgumentInfoType.Page.ToString();

        public static string PageSizeFieldName = QueryArgumentInfoType.PageSize.ToString();
    }
}