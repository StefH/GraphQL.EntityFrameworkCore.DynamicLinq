# GraphQL.EntityFrameworkCore.DynamicLinq

Add EntityFramework Core Dynamic IQueryable support to GraphQL.

## Info
| | |
|-|-|
| &nbsp;&nbsp;**Build Azure** | [![Build Status](https://dev.azure.com/stef/GraphQL.EntityFrameworkCore.DynamicLinq/_apis/build/status/StefH.GraphQL.EntityFrameworkCore.DynamicLinq)](https://dev.azure.com/stef/GraphQL.EntityFrameworkCore.DynamicLinq/_build/latest?definitionId=26) |
| &nbsp;&nbsp;**Codecov** | [![codecov](https://codecov.io/gh/StefH/GraphQL.EntityFrameworkCore.DynamicLinq/branch/master/graph/badge.svg)](https://codecov.io/gh/StefH/GraphQL.EntityFrameworkCore.DynamicLinq) |
| &nbsp;&nbsp;**NuGet** | [![NuGet: GraphQL.EntityFrameworkCore.DynamicLinq](https://buildstats.info/nuget/GraphQL.EntityFrameworkCore.DynamicLinq)](https://www.nuget.org/packages/GraphQL.EntityFrameworkCore.DynamicLinq)
| &nbsp;&nbsp;**MyGet (preview)** | [![MyGet: GraphQL.EntityFrameworkCore.DynamicLinq](https://buildstats.info/myget/graphql_entityframeworkcore_dynamiclinq/GraphQL.EntityFrameworkCore.DynamicLinq)](https://www.myget.org/feed/graphql_entityframeworkcore_dynamiclinq/package/nuget/GraphQL.EntityFrameworkCore.DynamicLinq) |


# How To

With this project you can easily expose all properties from the EF Entities as searchable fields on the GraphQL query.

## Entity Type

``` c#
public class Room
{
    [Key]
    public int Id { get; set; }
    public int Number { get; set; }
    public string Name { get; set; }
    public bool AllowedSmoking { get; set; }
    public RoomStatus Status { get; set; }
}
```

## GraphQL Type

``` c#
public class RoomType : ObjectGraphType<Room>
{
    public RoomType()
    {
        Field(x => x.Id);
        Field(x => x.Name);
        Field(x => x.Number);
        Field(x => x.AllowedSmoking);
        Field<RoomStatusType>(nameof(RoomModel.Status));
    }
}
```

# Execute the Query

#### Filter on `allowedSmoking`
``` js
query {
  rooms (allowedSmoking: false) {
    name
    number
    allowedSmoking
    status
  }
}
```

#### OrderBy `name`
It's even possible to add support for an **OrderBy** field, just add the `.SupportOrderBy();` in the code.

``` js
query {
  rooms (orderBy: "name desc") {
    name
    number
    status
  }
}
```

# How to use

### Add the required dependency injections
``` diff
public void ConfigureServices(IServiceCollection services)
{
+    services.Configure<QueryArgumentInfoListBuilderOptions>(Configuration.GetSection("QueryArgumentInfoListBuilderOptions"));
+    services.AddGraphQLEntityFrameworkCoreDynamicLinq();
}
```

### Update your GraphQL Query
``` c#
public class MyHotelQuery : ObjectGraphType
{
    public MyHotelQuery(MyHotelRepository myHotelRepository, IQueryArgumentInfoListBuilder builder)
    {
1       var roomQueryArgumentList = builder.Build<RoomType>()
2           .Exclude("Id")
3           .SupportOrderBy()
4           .Supportpaging();

        Field<ListGraphType<RoomType>>("rooms",
5           arguments: roomQueryArgumentList.ToQueryArguments(),

            resolve: context => myHotelRepository.GetRoomsQuery()
6               .ApplyQueryArguments(roomQueryArgumentList, context)
                .ToList()
        );
    }
}
```

1. Use the `IQueryArgumentInfoListBuilder` to build all possible arguments based on the fields from the GraphQL type (e.g. `RoomType`)
2. Optionally exclude some properties which should not be searchable
3. Optionally add support for OrderBy (argument-name will be `OrderBy`)
4. Optionally add support for Paging (argument-names will be `Page` and `PageSize`)
5. Call the `.ToQueryArguments()` to craete a new `QueryArguments`.
6. Call the `ApplyQueryArguments` extension method to apply the seacrh criteria and optionally the OrderBy

### Example
See example projec: [examples/MyHotel](https://github.com/StefH/GraphQL.EntityFrameworkCore.DynamicLinq/tree/master/examples/MyHotel) for more details.


# References
- [Microsoft.EntityFrameworkCore.DynamicLinq](https://github.com/StefH/System.Linq.Dynamic.Core)
- [EntityFramework Core IQueryable](https://docs.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbset-1.system-linq-iqueryable-provider)
- [GraphQL](https://github.com/graphql-dotnet/graphql-dotnet)
- My example project is based on [ebicoglu/AspNetCoreGraphQL-MyHotel](https://github.com/ebicoglu/AspNetCoreGraphQL-MyHotel).