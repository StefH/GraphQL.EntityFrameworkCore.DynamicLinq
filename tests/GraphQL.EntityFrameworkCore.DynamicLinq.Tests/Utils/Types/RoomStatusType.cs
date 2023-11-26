using System.Diagnostics.CodeAnalysis;
using GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Utils.Entities;
using GraphQL.Types;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Utils.Types;

[ExcludeFromCodeCoverage]
public class RoomStatusType : EnumerationGraphType<RoomStatus>
{
    public RoomStatusType()
    {
        Description = "Shows if the room is available or not.";
    }
}