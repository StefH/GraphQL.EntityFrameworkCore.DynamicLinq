using System.Diagnostics.CodeAnalysis;
using GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Utils.Entities;
using GraphQL.Types;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Tests.Utils.Types
{
    [ExcludeFromCodeCoverage]
    public class ExtraType : ObjectGraphType<Extra>
    {
        public ExtraType()
        {
            Field(x => x.Test);
        }
    }
}