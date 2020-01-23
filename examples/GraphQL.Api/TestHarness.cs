using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GraphQL.Builders;
using GraphQL.EntityFrameworkCore.DynamicLinq.Builders;
using GraphQL.EntityFrameworkCore.DynamicLinq.Enums;
using GraphQL.EntityFrameworkCore.DynamicLinq.Extensions;
using GraphQL.EntityFrameworkCore.DynamicLinq.Models;
using GraphQL.EntityFrameworkCore.DynamicLinq.Resolvers;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.Api
{
    public class Customer
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public ICollection<Order> Orders { get; set; }
    }

    public class Order
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public int CustomerID { get; set; }
        public Customer Customer { get; set; }
        public virtual ICollection<OrderLine> OrderLines { get; set; }
    }

    public class OrderLine
    {
        public int Id { get; set; }
        public string Details { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
    }

    //public class FilterInput<T, T2> : InputObjectGraphType
    //    where T : EnumerationGraphType
    //    where T2 : class
    //{
    //    public FilterInput()
    //    {
    //        Name = "FilterInput" + typeof(T2).Name;
    //        Field<T>("where");
    //        Field<T>("and");
    //        Field<T>("or");
    //        Field<ListGraphType<FilterInput<T, T2>>>("andFilter");
    //        Field<ListGraphType<FilterInput<T, T2>>>("orFilter");
    //        Field<StringGraphType>("eq");
    //        Field<StringGraphType>("not");
    //    }
    //}

    public interface IEfGraphQLService<TDbContext> where TDbContext : DbContext
    {
        QueryArgumentInfoList Build(Type type);

        QueryArgumentInfoList Build(FieldType type, Type thisModel);

        TDbContext DbContext { get; }
    }

    public class EfGraphQLService<TDbContext> : IEfGraphQLService<TDbContext> where TDbContext : DbContext
    {
        // private readonly TDbContext _dbcontext;
        private readonly IQueryArgumentInfoListBuilder _builder;

        public EfGraphQLService(TDbContext dbcontext, IQueryArgumentInfoListBuilder builder)
        {
            DbContext = dbcontext;
            _builder = builder;
        }

        public QueryArgumentInfoList Build(Type type)
        {
            return _builder.Build(type);
        }

        public QueryArgumentInfoList Build(FieldType field, Type thisModel)
        {
            return _builder.Build(field, thisModel);
        }

        public TDbContext DbContext { get; }
    }

    public class EfObjectGraphType<TDbContext, TSource> :
        ObjectGraphType<TSource>
        where TDbContext : DbContext
    {
        private readonly IEfGraphQLService<TDbContext> _service;

        private readonly IPropertyPathResolver _propertyPathResolver;
        //private readonly TestDBContext _dbcontext;
        //private readonly IQueryArgumentInfoListBuilder _builder;

        private readonly QueryArgumentInfoList _list = new QueryArgumentInfoList();

        public EfObjectGraphType(IEfGraphQLService<TDbContext> service, IPropertyPathResolver propertyPathResolver)
        {
            _service = service;
            _propertyPathResolver = propertyPathResolver;
            //_dbcontext = dbcontext;
            //_builder = builder;
        }

        public FieldBuilder<TSource, TProperty> QueryField<TProperty>(
            Expression<Func<TSource, TProperty>> expression,
            bool nullable = false,
            Type type = null)
        {
            var field = Field(expression, nullable, type);

            var l = _service.Build(field.FieldType, typeof(TSource));
            _list.AddRange(l);
            return field;
        }

        public FieldType QueryField<TGraphType, TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, TGraphType>, IQueryable<TReturn>> resolve)
            where TReturn : class
        {
            //QueryArgumentInfoList list = new QueryArgumentInfoList();
            bool fieldIsList = typeof(TGraphType).IsListGraphType();
            Type t = fieldIsList ? typeof(TGraphType).GenericTypeArguments.First() : typeof(TGraphType);
            bool isNonNullGraphType = t.IsNonNullGraphType();

            //var queryArgument = new QueryArgument(typeof(TGraphType)) { Name = name };

            //var resolvedParentEntityPath = new EntityPath
            //{
            //    IsListGraphType = fieldIsList,
            //    IsNullable = t.IsNullable(),
            //    Path = _propertyPathResolver.Resolve(typeof(TSource), name)
            //};
            //var entityPath = new List<EntityPath> { resolvedParentEntityPath };

            //if (typeof(TGraphType).GetInterface("IComplexGraphType") == null)
            //{

            //}

            //list.Add(new QueryArgumentInfo
            //{
            //    QueryArgument = queryArgument,
            //    GraphQLPath = name,
            //    EntityPath = entityPath,
            //    IsNonNullGraphType = isNonNullGraphType,
            //    QueryArgumentInfoType = QueryArgumentInfoType.GraphQL
            //});

            var list = _service.Build(t);

            var rss = new ResolveEfFieldContext<TDbContext, TGraphType>
            {
                DbContext = _service.DbContext
            };

            //var f = resolve(rss);
            //var q = f.ApplyQueryArguments(list, null);
            

            return AddField(new FieldType
            {
                Name = name,
                
                Type = typeof(TGraphType),
                //Arguments = new QueryArguments(queryArgument),

                // Func<ResolveFieldContext<TSourceType>, TReturnType> resolver
                //Resolver = resolve != null ? (IFieldResolver)new FuncFieldResolver<TEntityType, IQueryable<TEntityType>>(resolve) : (IFieldResolver)null
                Resolver = new FuncFieldResolver<TGraphType, IEnumerable<TReturn>>(context =>
                {
                    var queryable = resolve(rss);

                    return queryable.ApplyQueryArguments(list, context);
                })
            });


            //return Field<ListGraphType<TGraphType>>(name, description,
            //    arguments: orderArguments.ToQueryArguments(),
            //    resolve: context => .ApplyQueryArguments(orderArguments, context)
            //);
        }
    }

    public class ResolveEfFieldContext<TDbContext, TSource> :
        ResolveFieldContext<TSource>
        where TDbContext : DbContext
    {
        public TDbContext DbContext { get; set; } = null!;
        //public Filters Filters { get; set; } = null!;
    }

    public class Customer1Graph : EfObjectGraphType<TestDBContext, Customer>
    {
        public Customer1Graph(IEfGraphQLService<TestDBContext> service, IPropertyPathResolver r) : base(service,r)
        {
            Name = nameof(Customer);

            QueryField(x => x.CustomerID);
            QueryField(x => x.CustomerName);

            //var orderArguments = builder.Build<OrderGraph>().SupportOrderBy();
            //Field<ListGraphType<OrderGraph>>(nameof(Customer.Orders),
            //    arguments: orderArguments.ToQueryArguments(),
            //    resolve: context => dbcontext.Orders
            //        .Include(o => o.OrderLines)
            //        .ApplyQueryArguments(orderArguments, context)
            //);

            //QueryField<OrderGraph, Order>("orders", c => c.DbContext.Orders);
        }
    }

    public class CustomerGraph : ObjectGraphType<Customer>
    {
        public CustomerGraph(TestDBContext dbcontext, IQueryArgumentInfoListBuilder builder)
        {
            Name = nameof(Customer);

            Field(x => x.CustomerID);
            Field(x => x.CustomerName);

            var orderArguments = builder.Build<OrderGraph>().SupportOrderBy();
            Field<ListGraphType<OrderGraph>>(nameof(Customer.Orders),
                arguments: orderArguments.ToQueryArguments(),
                resolve: context => dbcontext.Orders
                    .Include(o => o.OrderLines)
                    .ApplyQueryArguments(orderArguments, context)
            );
        }
    }

    public class OrderGraph : ObjectGraphType<Order>
    {
        public OrderGraph(TestDBContext dbcontext, IQueryArgumentInfoListBuilder builder)
        {
            Name = nameof(Order);

            Field(x => x.OrderID);
            Field(x => x.OrderDate);
            Field(x => x.CustomerID);

            //var customerArguments = builder.Build<CustomerGraph>().SupportOrderBy();
            //Field<CustomerGraph>(nameof(Order.Customer),
            //    arguments: customerArguments.ToQueryArguments(),
            //    resolve: context => dbcontext.Customers
            //        .Where(c => c.CustomerID == context.Source.CustomerID)
            //        .ApplyQueryArguments(customerArguments, context)
            //);

            //Field<CustomerGraph>("Customer", resolve: context => context.Source.Customer);
            //Field<ListGraphType<OrderLineGraph>>("OrderLines", resolve: context => context.Source.OrderLines);

            var orderLineArguments = builder.Build<OrderLineGraph>().SupportOrderBy();
            Field<ListGraphType<OrderLineGraph>>(nameof(Order.OrderLines),
                arguments: orderLineArguments.ToQueryArguments(),
                resolve: context => dbcontext.OrderLines
                    .ApplyQueryArguments(orderLineArguments, context)
            );
        }
    }

    public class OrderLineGraph : ObjectGraphType<OrderLine>
    {
        public OrderLineGraph()
        {
            Name = "OrderLine";
            Field(x => x.Id);
            Field(x => x.Details);
            Field(x => x.OrderId);
            Field<OrderGraph>("Order", resolve: context => context.Source.Order);
        }
    }

    public class TestDBContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }

        public TestDBContext(DbContextOptions<TestDBContext> options) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            if (builder != null)
            {
                builder.Entity<Customer>().ToTable("Customers");
                builder.Entity<Customer>().HasKey(x => x.CustomerID);

                builder.Entity<Order>().ToTable("Orders");
                builder.Entity<Order>().HasKey(x => x.OrderID);

                builder.Entity<OrderLine>().ToTable("OrderLines");
                builder.Entity<OrderLine>().HasKey(x => x.Id);

                builder.Entity<Customer>().HasMany(x => x.Orders).WithOne(x => x.Customer);
                builder.Entity<Order>().HasMany(x => x.OrderLines).WithOne(x => x.Order);
            }
        }
    }

    public class SchemaTest : Schema
    {
        public SchemaTest(IDependencyResolver resolver) : base(resolver)
        {
            // Query = resolver.Resolve<QueryTest>();
            Query = resolver.Resolve<QueryTestEf>();
        }
    }

    public class QueryTestEf : EfObjectGraphType<TestDBContext, object>
    {
        public QueryTestEf(IEfGraphQLService<TestDBContext> service, IPropertyPathResolver propertyPathResolver) : base(service, propertyPathResolver)
        {
            QueryField<ListGraphType<Customer1Graph>, Customer>("customers", c => c.DbContext.Customers);
        }
    }

    public class QueryTest : ObjectGraphType
    {
        public QueryTest(TestDBContext dbcontext, IQueryArgumentInfoListBuilder builder)
        {
            Name = "Query";

            //var customerArguments = builder.Build<CustomerGraph>().SupportOrderBy();
            //Field<ListGraphType<CustomerGraph>>("customers",
            //    arguments: customerArguments.ToQueryArguments(),
            //    resolve: context => dbcontext.Customers
            //        .ApplyQueryArguments(customerArguments, context)
            //);

            //var orderArguments = builder.Build<OrderGraph>().SupportOrderBy();
            //Field<ListGraphType<OrderGraph>>("Orders",
            //    arguments: orderArguments.ToQueryArguments(),
            //    resolve: context => dbcontext.Orders
            //        //.Include(o => o.Customer).ThenInclude(c => c.Orders)
            //        //.Include(o => o.OrderLines)
            //        .ApplyQueryArguments(orderArguments, context)
            //);

            //var orderLineArguments = builder.Build<OrderLineGraph>().SupportOrderBy();
            //Field<ListGraphType<OrderLineGraph>>("orderlines",
            //    arguments: orderLineArguments.ToQueryArguments(),
            //    resolve: context => dbcontext.OrderLines
            //        //.Include(ol => ol.Order)
            //        .ApplyQueryArguments(orderLineArguments, context)
            //);
        }
    }
}