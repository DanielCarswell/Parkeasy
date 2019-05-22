using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Parkeasy.Data;
using Parkeasy.Models;
using Parkeasy.Utility;
using Parkeasy.Services;
using Stripe;

namespace Parkeasy
{
    /// <summary>
    /// Startup class, configures data and services upon launch.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Startup Class Constructor with Configuration parameter.
        /// </summary>
        public Startup(IConfiguration configuration)
        {
            //Initialises global Getter Configuration equal to configuration parameter.
            Configuration = configuration;
        }

        /// <summary>
        /// Global Getter propertie Configuration of type IConfiguration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">Instance of IServiceCollection</param>
        public void ConfigureServices(IServiceCollection services)
        {
            //Initialises DbContext service with ApplicationDbContext class as its parameter, also Sqlite database type.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            //Initialises Identity with parameters ApplicationUser and IdentityRole.
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add application services.
            services.AddTransient<Seed>();
            services.AddTransient<Automation>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddSingleton<IEmailSender, EmailSender>();
            services.Configure<StripeSettings>(Configuration.GetSection("Stripe"));
            services.Configure<SendGridOptions>(Configuration);

            //Adding Google+ API External Login Authentication.
            services.AddAuthentication().AddGoogle(googleOptions => 
            {
                googleOptions.ClientId = "396877038571-l2td3t3ng1e79boeugdnela4rr1tk898.apps.googleusercontent.com";
                googleOptions.ClientSecret = "mgUkabIJZi6SuABUK5OaK66X";
            });

            //Changing Password complexity for faster testing.
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 5;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
            });

            //Initialising MVC service.
            services.AddMvc();
services.AddDistributedMemoryCache(); // Adds a default in-memory implementation of IDistributedCache
services.AddSession();
services.AddNodeServices();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">Instance of IApplicationBuilder</param>
        /// <param name="env">Instance of IHostingEnvironment</param>
        /// <param name="seeder">Instance of Seed Class</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, Seed seeder, Automation automate)
        {
            //If the environment is in Development mode.
            if (env.IsDevelopment())
            {
                //Allow DeveloperExceptionPage and DatabaseErrorPage.
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            StripeConfiguration.SetApiKey(Configuration.GetSection("Stripe")["SecretKey"]);

            //Initialising StaticFiles and Authentication.
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseSession();

            //Setting initial startup page.
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            
            //Running SeedUsers method to seed database if necessary.
            //This code will have problems if database does not yet exist.
            //To solve this comment line out, Generate Migrations and database then uncomment and run code.
            seeder.SeedUsers();
            seeder.SeedSlotData();
            seeder.SeedPricing();
        }
    }
}