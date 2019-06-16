using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.HttpOverrides;
using EcommerceApi.Services;
using EcommerceApi.Extensions;
using EcommerceApi.Repositories;
using DinkToPdf;
using DinkToPdf.Contracts;
using EcommerceApi.Untilities;
using System.IO;
using EcommerceApi.Middleware;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace EcommerceApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddJsonOptions(options => 
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

            try
            {
                var context = new CustomAssemblyLoadContext();
                context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox.dll"));

                services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            }
            catch
            {
                // cannot load this locally, catching and ignoring it for now
            }

            services.AddEntityFrameworkSqlServer().AddDbContext<EcommerceContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("defaultConnection"));
            });

            // Configure Entity Framework Initializer for seeding
            services.AddTransient<IDefaultDbContextInitializer, DefaultDbContextInitializer>();

            // Repositories
            services.AddTransient<IProductRepository, ProductRepository>();
            services.AddTransient<IOrderRepository, OrderRepository>();
            services.AddTransient<IPurchaseRepository, PurchaseRepository>();
            services.AddTransient<ICustomerRepository, CustomerRepository>();
            services.AddTransient<IReportRepository, ReportRepository>();
            services.AddTransient(_ => new AppDb(Configuration.GetConnectionString("mysqlConnection")));

            services.AddTransient<IEmailSender, EmailSender>();

            // Configure Entity Framework Identity for Auth
            services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<EcommerceContext>()
            .AddDefaultTokenProviders();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(config =>
            {
                config.RequireHttpsMetadata = false;
                config.SaveToken = true;

                config.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidIssuer = Configuration["jwt:issuer"],
                    ValidAudience = Configuration["jwt:issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["jwt:key"]))
                };
            });

            services.Configure<JwtOptions>(Configuration.GetSection("jwt"));
            services.AddResponseCompression();
            services.AddCors();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                // Configure Webpack Middleware (Ref: http://blog.stevensanderson.com/2016/05/02/angular2-react-knockout-apps-on-aspnet-core/)
                //  - Intercepts requests for webpack bundles and routes them through Webpack - this prevents needing to run Webpack file watcher separately
                //  - Enables Hot module replacement (HMR)
                //app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                //{
                //    HotModuleReplacement = true,
                //    HotModuleReplacementClientOptions = new Dictionary<string, string> {{ "reload", "true" }},
                //    ReactHotModuleReplacement = true,
                //    ConfigFile = System.IO.Path.Combine(Configuration["webClientPath"], "webpack.config.js")
                //});

                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            // If not requesting /api*, rewrite to / so SPA app will be returned
            //app.UseSpaFallback(new SpaFallbackOptions()
            //{
            //    ApiPathPrefix = "/api",
            //    RewritePath = "/"
            //});

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.ConfigureExceptionHandler();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                // Read and use headers coming from reverse proxy: X-Forwarded-For X-Forwarded-Proto
                // This is particularly important so that HttpContet.Request.Scheme will be correct behind a SSL terminating proxy
                ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto
            });

            app.UseAuthentication();

            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());

            app.UseResponseCompression();
            // app.UseMiddleware<AdminSafeListMiddleware>();
            app.UseMvc();
        }
    }
}
