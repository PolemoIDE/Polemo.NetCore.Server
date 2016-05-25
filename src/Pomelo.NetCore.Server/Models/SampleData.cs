using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Pomelo.NetCore.Server.Models
{
    public static class SampleData
    {
        public static async Task InitDB(IServiceProvider services)
        {
            var DB = services.GetRequiredService<PomeloContext>();
            var UserManager = services.GetRequiredService<UserManager<User>>();
            var user = new User { UserName = "admin", Email = "1@1234.sh" };
            await UserManager.CreateAsync(user, "123456");
            var project1 = new Project { Id = Guid.NewGuid(), Email = "1@1234.sh", UserName = "Kagamine", Git = "https://github.com/kagamine/AzureHackathonSample", Password = "Yuuko19931101", UpdatedTime = DateTime.Now, Title = "AzureHackathonSample" };
            var project2 = new Project { Id = Guid.NewGuid(), Email = "1@1234.sh", UserName = "Kagamine", Git = "https://github.com/kagamine/pomelo.git.test", Password = "Yuuko19931101", UpdatedTime = DateTime.Now, Title = "pomelo.git.test" };
            DB.Projects.Add(project1);
            DB.Projects.Add(project2);
            await UserManager.AddClaimAsync(user, new System.Security.Claims.Claim("Owned Project", project1.Id.ToString()));
            await UserManager.AddClaimAsync(user, new System.Security.Claims.Claim("Owned Project", project2.Id.ToString()));
            DB.SaveChanges();
        }
    }
}
