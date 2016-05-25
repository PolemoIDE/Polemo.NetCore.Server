using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.NetCore.Server.Models;

namespace Pomelo.NetCore.Server.Hubs
{
    public partial class PomeloHub : Hub
    {
        public async Task<object> GetProjects()
        {
            var DB = Context.Request.HttpContext.RequestServices.GetRequiredService<PomeloContext>();
            var UserManager = Context.Request.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
            var user = await UserManager.FindByNameAsync(Context.Request.HttpContext.User.Identity.Name);
            var ids = (await UserManager.GetClaimsAsync(user)).Where(x => x.Type == "Owned Project").Select(x => Guid.Parse(x.Value)).ToList();
            var projects = DB.Projects
                .Where(x => ids.Contains(x.Id))
                .OrderByDescending(x => x.UpdatedTime)
                .ToList();
            var ret = new List<object>();
            if (projects.Count() != 0)
            {
                foreach (var x in projects)
                {
                    var obj = new
                    {
                        Project = x.Title,
                        Git = x.Git,
                        Password = x.Password,
                        Name = x.UserName,
                        Email = x.Email
                    };
                    ret.Add(obj);
                }
                return ret;
            }
            else
            {
                return false;
            }
        }

        public bool CreateProject(string Username, string Git, string Password, string Email)
        {
            var DB = Context.Request.HttpContext.RequestServices.GetRequiredService<PomeloContext>();
            var title = Git.Substring(Git.LastIndexOf('/') + 1);
            var project = new Project
            {
                Title = title,
                Git = Git,
                Password = Password,
                UserName = Username,
                Email = Email,
                UpdatedTime = DateTime.Now,
            };
            DB.Projects.Add(project);
            DB.SaveChanges();
            return true;
        }

        public bool OpenProject(string Git)
        {
            var DB = Context.Request.HttpContext.RequestServices.GetRequiredService<PomeloContext>();
            var project = DB.Projects
                .Where(x => x.Git == Git)
                .SingleOrDefault();
            project.UpdatedTime = DateTime.Now;
            DB.SaveChanges();
            return true;
        }
    }
}
