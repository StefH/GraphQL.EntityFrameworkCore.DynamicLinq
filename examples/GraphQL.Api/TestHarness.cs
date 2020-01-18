using System;
using System.Collections.Generic;
using System.Linq;
// using System.Linq.Dynamic.Core;
using GraphQL.EntityFrameworkCore.DynamicLinq.Builders;
using GraphQL.EntityFrameworkCore.DynamicLinq.Extensions;
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
            Query = resolver.Resolve<QueryTest>();
        }
    }

    public class QueryTest : ObjectGraphType
    {
        public QueryTest(TestDBContext dbcontext, IQueryArgumentInfoListBuilder builder)
        {
            Name = "Query";

            var customerArguments = builder.Build<CustomerGraph>().SupportOrderBy();
            Field<ListGraphType<CustomerGraph>>("Customers",
                arguments: customerArguments.ToQueryArguments(),
                resolve: context => dbcontext.Customers
                    .ApplyQueryArguments(customerArguments, context)
            );

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