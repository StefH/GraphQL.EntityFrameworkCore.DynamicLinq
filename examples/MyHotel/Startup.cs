using System;
using AutoMapper;
using GraphQL;
using GraphQL.Client;
using GraphQL.EntityFrameworkCore.DynamicLinq.DependencyInjection;
using GraphQL.EntityFrameworkCore.DynamicLinq.Options;
using GraphQL.EntityFrameworkCore.DynamicLinq.Resolvers;
using GraphQL.Server;
using GraphQL.Server.Ui.Playground;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyHotel.AutoMapper;
using MyHotel.EntityFrameworkCore;
using MyHotel.GraphQL;
using MyHotel.GraphQL.Client;
using MyHotel.Repositories;

namespace MyHotel
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            //***< My services >*** 
            services.AddHttpClient<ReservationHttpGraphqlClient>(x => x.BaseAddress = new Uri(Configuration["GraphQlEndpoint"]));
            services.AddSingleton(t => new GraphQLClient(Configuration["GraphQlEndpoint"]));
            services.AddSingleton<ReservationGraphqlClient>();

            services.Configure<QueryArgumentInfoListBuilderOptions>(Configuration.GetSection("QueryArgumentInfoListBuilderOptions"));
            services.AddGraphQLEntityFrameworkCoreDynamicLinq();
            services.AddScoped<IPropertyPathResolver, AutoMapperPropertyPathResolver>();
            //***</ My services >*** 

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            services.AddDbContext<MyHotelDbContext>(options => options.UseSqlServer(Configuration["ConnectionStrings:MyHotelDb"]));

            services.AddTransient<MyHotelRepository>();

            //***< GraphQL Services >***
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            //services.AddAuthentication(option =>
            //{
            //    option.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //    option.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //    option.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //}).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

            services.AddScoped<IDependencyResolver>(x => new FuncDependencyResolver(x.GetRequiredService));

            services.AddScoped<MyHotelSchema>();

            services.AddGraphQL(x =>
                {
                    x.EnableMetrics = true;
                    x.ExposeExceptions = true; //set true only in dev mode.
                })
                .AddGraphTypes(ServiceLifetime.Scoped)
                .AddUserContextBuilder(httpContext => httpContext.User)
                .AddDataLoader()
                //.AddGraphQLAuthorization(options =>
                //{
                //    options.AddPolicy("Authorized", p => p.RequireAuthenticatedUser());
                //    //var policy = new AuthorizationPolicyBuilder();
                //    //                    .
                //    //options.AddPolicy("Authorized", p => p.RequireClaim(ClaimTypes.Name, "Tom"));
                //})
                ;
            //.AddUserContextBuilder(context => new GraphQLUserContext { User = context.User });

            //***</ GraphQL Services >*** 
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, MyHotelDbContext dbContext, IMapper mapper)
        {
            mapper.ConfigurationProvider.AssertConfigurationIsValid();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseCookiePolicy();
            app.UseAuthentication();

            app.UseGraphQL<MyHotelSchema>();
            app.UseGraphQLPlayground(new GraphQLPlaygroundOptions()); // to explore API navigate https://*DOMAIN*/ui/playground

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";
                spa.Options.StartupTimeout = TimeSpan.FromSeconds(120);

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}
