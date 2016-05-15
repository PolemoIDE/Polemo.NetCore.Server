using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

            services.AddDbContext<PolemoContext>(x => 
                x.UseNpgsql("User ID=postgres;Password=123456;Host=localhost;Port=5432;Database=polemo;"));


            services.AddIdentity<User, IdentityRole>(x =>
            {
                x.Password.RequireDigit = false;
                x.Password.RequiredLength = 0;
                x.Password.RequireLowercase = false;
                x.Password.RequireNonAlphanumeric = false;
                x.Password.RequireUppercase = false;
                x.User.AllowedUserNameCharacters = null;
            })
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<PolemoContext>();

            services.AddLogging();
            services.AddSmtpEmailSender("smtp.exmail.qq.com", 25, "码锋科技", "service@codecomb.com", "service@codecomb.com", "Yuuko19931101");
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(minLevel: LogLevel.Warning);
            app.UseCors("Polemo");
            app.UseSignalR();
            app.UseIdentity();
        }
    }
}
