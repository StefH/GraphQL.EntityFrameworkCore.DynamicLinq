using System.Diagnostics.CodeAnalysis;
using GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Utils.Entities;
using GraphQL.Types;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Utils.Types;

[ExcludeFromCodeCoverage]
public class GuestType : ObjectGraphType<Guest>
{
    public GuestType()
    {
        Field(x => x.Id);
        Field(x => x.Name);
        Field(x => x.RegisterDate);
        Field(x => x.NullableInt, nullable: true);
    }
}