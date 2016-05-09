using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Data.Entity;
using Microsoft.Extensions.DependencyInjection;
using Polemo.NetCore.Server.Models;

namespace Polemo.NetCore.Server
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(c => c.AddPolicy("Polemo", x =>
                x.AllowCredentials()
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
            ));

            services.AddEntityFramework()
                .AddNpgsql()
                .AddDbContext<PolemoContext>(x => x.UseNpgsql("User ID=postgres;Password=123456;Host=localhost;Port=5432;Database=polemo;"));

            services.AddIdentity<User, IdentityRole>(x =>
            {
                x.Password.RequireDigit = false;
                x.Password.RequiredLength = 0;
                x.Password.RequireLowercase = false;
                x.Password.RequireNonLetterOrDigit = false;
                x.Password.RequireUppercase = false;
                x.User.AllowedUserNameCharacters = null;
            })
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<PolemoContext>();

            services.AddAesCrypto();
            services.AddSmtpEmailSender("smtp.exmail.qq.com", 25, "码锋科技", "service@codecomb.com", "service@codecomb.com", "Yuuko19931101");
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCors("Polemo");
            app.UseIISPlatformHandler();
            app.UseSignalR();
            app.UseIdentity();
        }

        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
