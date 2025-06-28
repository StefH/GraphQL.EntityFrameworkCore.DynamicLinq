## GraphQL.EntityFrameworkCore.DynamicLinq

Add EntityFramework Core Dynamic IQueryable support to GraphQL.

## How To

With this project you can easily expose all properties from the EF Entities as searchable fields on the GraphQL query.

### Entity Type

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

### GraphQL Type

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

## Execute the Query

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
It's also possible to add support for an **OrderBy** field, just add the `.SupportOrderBy();` in the code.

``` js
query {
  rooms (orderBy: "name desc") {
    name
    number
    status
  }
}
```

#### Paging
It's also possible to add support for **Paging**, just add the `.SupportPaging();` in the code.

``` js
query {
  roomsWithPaging (page: 1, pageSize: 2) {
    id
    name
    number
    status
  }
}
```

## How to use

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
4           .SupportPaging();

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
2. Optionally include/exclude some properties which should not be searchable (this can also be a wildcard like `*Id`)
3. Optionally add support for OrderBy (argument-name will be `OrderBy`)
4. Optionally add support for Paging (argument-names will be `Page` and `PageSize`)
5. Call the `.ToQueryArguments()` to create a new `QueryArguments` object.
6. Call the `ApplyQueryArguments` extension method to apply the search criteria (optionally the OrderBy and Paging)


### Sponsors

[Entity Framework Extensions](https://entityframework-extensions.net/?utm_source=StefH) and [Dapper Plus](https://dapper-plus.net/?utm_source=StefH) are major sponsors and proud to contribute to the development of **GraphQL.EntityFrameworkCore.DynamicLinq**.

[![Entity Framework Extensions](https://raw.githubusercontent.com/StefH/resources/main/sponsor/entity-framework-extensions-sponsor.png)](https://entityframework-extensions.net/bulk-insert?utm_source=StefH)

[![Dapper Plus](https://raw.githubusercontent.com/StefH/resources/main/sponsor/dapper-plus-sponsor.png)](https://dapper-plus.net/bulk-insert?utm_source=StefH)