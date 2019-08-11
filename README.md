# GraphQL.EntityFrameworkCore.DynamicLinq

Add EntityFramework Core Dynamic IQueryable support to GraphQL.


## NuGet
[![GraphQL.EntityFrameworkCore.DynamicLinq](https://buildstats.info/nuget/GraphQL.EntityFrameworkCore.DynamicLinq)](https://www.nuget.org/packages/GraphQL.EntityFrameworkCore.DynamicLinq)



# Information

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

#### Install NuGet
Instal the latest version from GraphQL.EntityFrameworkCore.DynamicLinq from NuGet.

#### Add the required dependency injections
``` diff
public void ConfigureServices(IServiceCollection services)
{
+    services.AddGraphQLEntityFrameworkCoreDynamicLinq();
}
```


#### Update your GraphQL Query
``` c#
public class MyHotelQuery : ObjectGraphType
{
    public MyHotelQuery(MyHotelRepository myHotelRepository, IQueryArgumentInfoListBuilder builder)
    {
1       var roomQueryArgumentList = builder.Build<RoomType>()
2           .Exclude("Id")
3           .SupportOrderBy();

        Field<ListGraphType<RoomType>>("rooms",
4           arguments: new QueryArguments(roomQueryArgumentList.Select(q => q.QueryArgument)),

            resolve: context => myHotelRepository.GetRoomsQuery()
5               .ApplyQueryArguments(roomQueryArgumentList, context)
                .ToList()
        );
    }
}
```

1. Use the `IQueryArgumentInfoListBuilder` to build all possible arguments based on the fields from the GraphQL type (e.g. `RoomType`)
2. Optionally exclude some properties which should not be searchable
3. Optionally add support for OrderBy 
4. Create a new `QueryArguments` which uses the list.
5. Call the `ApplyQueryArguments` extension method to apply the seacrh criteria and optionally the OrderBy


# References
- Sample project based on https://github.com/ebicoglu/AspNetCoreGraphQL-MyHotel