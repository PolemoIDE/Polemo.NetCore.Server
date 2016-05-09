using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Data.Entity;

namespace Polemo.NetCore.Server.Models
{
    public class PolemoContext : IdentityDbContext<User>
    {
        public DbSet<Project> Projects { get; set; }

        public DbSet<Node> Nodes { get; set; }
    }
}
