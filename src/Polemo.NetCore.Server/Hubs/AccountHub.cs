using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.Identity;
using Microsoft.Extensions.DependencyInjection;
using Polemo.NetCore.Server.Models;

namespace Polemo.NetCore.Server.Hubs
{
    public partial class PolemoHub : Hub
    {
        public async Task<bool> SignIn(string username, string password)
        {
            var SignInManager = Context.Request.HttpContext.RequestServices.GetRequiredService<SignInManager<User>>();
            var result = await SignInManager.PasswordSignInAsync(username, password, true, false);
            if (result.Succeeded)
                return true;
            else
                return false;
        }

        public async void SignOut()
        {
            var SignInManager = Context.Request.HttpContext.RequestServices.GetRequiredService<SignInManager<User>>();
            await SignInManager.SignOutAsync();
        }

        public string Register(string email)
        {
            var DB = Context.Request.HttpContext.RequestServices.GetRequiredService<PolemoContext>();
            if (DB.Users.SingleOrDefault(x => x.Email == email) != null)
                return "抱歉，该邮箱已经注册过Polemo账号，请更换后重试！";
            return $"我们已经向电子邮箱{email}中发送了验证码，请您查阅邮件，并继续完成后续的注册步骤！";
        }
    }
}
