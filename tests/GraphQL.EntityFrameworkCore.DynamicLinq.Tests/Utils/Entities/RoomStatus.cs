using System.Diagnostics.CodeAnalysis;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Utils.Entities
{
    [ExcludeFromCodeCoverage]
    public enum RoomStatus
    {
        Unavailable = 0,
        Available = 1,
    }
}