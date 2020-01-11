using System;
using System.Collections.Generic;
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
    }

    public class CustomerGraph : ObjectGraphType<Customer>
    {
        public CustomerGraph()
        {
            Name = "Customer";
            Field(x => x.CustomerID);
            Field(x => x.CustomerName);
            Field<ListGraphType<OrderGraph>>("Orders", resolve: context => context.Source.Orders);
        }
    }

    public class OrderGraph : ObjectGraphType<Order>
    {
        public OrderGraph()
        {
            Name = "Order";
            Field(x => x.OrderID);
            Field(x => x.OrderDate);
            Field(x => x.CustomerID);
            Field<CustomerGraph>("Customer", resolve: context => context.Source.Customer);
        }
    }

    public class TestDBContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }

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

                builder.Entity<Customer>().HasMany(x => x.Orders).WithOne(x => x.Customer);
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
            Field<ListGraphType<CustomerGraph>>("customers",
                arguments: customerArguments.ToQueryArguments(),
                resolve: context => dbcontext.Customers.ApplyQueryArguments(customerArguments, context)
            );

            var orderArguments = builder.Build<OrderGraph>().SupportOrderBy();
            Field<ListGraphType<OrderGraph>>("orders",
                arguments: orderArguments.ToQueryArguments(),
                resolve: context => dbcontext.Orders.Include(x => x.Customer).ApplyQueryArguments(orderArguments, context)
            );
        }
    }
}