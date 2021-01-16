using GraphQL;
using GraphQL.Types;
using System;
using GraphQL.Utilities;
namespace MyHotel.GraphQL
{
    public class MyHotelSchema : Schema
    {
        public MyHotelSchema(IServiceProvider resolver) : base(resolver)
        {
            Query = resolver.GetRequiredService<MyHotelQuery>();
        }
    }
}