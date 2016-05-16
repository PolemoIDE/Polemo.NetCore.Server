using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Pomelo.NetCore.Server.Models
{
    public class PomeloContext : IdentityDbContext<User>
    {
        public DbSet<Project> Projects { get; set; }

        public DbSet<Node> Nodes { get; set; }

        public DbSet<Template> Templates { get; set; }

        public DbSet<RequestDeniedLog> RequestLists { get; set; }

        public DbSet<VerifyCode> VerifyCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Template>(e =>
            {
                e.HasIndex(x => x.PRI);
            });

            builder.Entity<RequestDeniedLog>(e =>
            {
                e.HasIndex(x => x.Time);
                e.HasIndex(x => x.IP);
            });

            builder.Entity<VerifyCode>(e =>
            {
                e.HasIndex(x => x.Code);
                e.HasIndex(x => x.Email);
                e.HasIndex(x => x.Expire);
                e.HasIndex(x => x.IsUsed);
            });
        }
    }
}
