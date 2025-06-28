using System.Diagnostics.CodeAnalysis;
using GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Utils.Entities;
using GraphQL.Types;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Utils.Types;

[ExcludeFromCodeCoverage]
public class RoomDetailType : ObjectGraphType<RoomDetail>
{
    public RoomDetailType()
    {
        Field(x => x.Id);
        Field(x => x.Windows);
        Field(x => x.Beds);
    }
}