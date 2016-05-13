using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.Identity;
using Microsoft.Extensions.DependencyInjection;
using Polemo.NetCore.Server.Models;
using CodeComb.Net.EmailSender;

namespace Polemo.NetCore.Server.Hubs
{
    public partial class PolemoHub : Hub
    {
        public async Task<object> SignIn(string username, string password)
        {
            // TODO: 需要建立一个HashTable，来记录每个IP的失败次数，1小时内失败次数超过10次则拒绝接受来自该IP的请求

            var SignInManager = Context.Request.HttpContext.RequestServices.GetRequiredService<SignInManager<User>>();
            var UserManager = Context.Request.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
            var result = await SignInManager.PasswordSignInAsync(username, password, true, false);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByNameAsync(username);
                var token = Guid.NewGuid().ToString();
                await UserManager.AddLoginAsync(user, new UserLoginInfo("Polemo", token, "Polemo"));
                return new { IsSucceeded = true, Token = token };
            }
            else
            {
                return new { IsSucceeded = false, Token = string.Empty };
            }
        }

        public async void SignOut()
        {
            var SignInManager = Context.Request.HttpContext.RequestServices.GetRequiredService<SignInManager<User>>();
            await SignInManager.SignOutAsync();
        }

        public async Task<bool> TokenSignIn(string token)
        {
            // TODO: 需要建立一个HashTable，来记录每个IP的失败次数，1小时内失败次数超过10次则拒绝接受来自该IP的请求
            var SignInManager = Context.Request.HttpContext.RequestServices.GetRequiredService<SignInManager<User>>();
            var result = await SignInManager.ExternalLoginSignInAsync("Polemo", token, true);
            if (result.Succeeded)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> ForgotVerifyEmail(string Email)
        {
            // TODO: 需要建立HashTable记录一小时内验证信请求次数，超过10次拒绝请求，否则会被视为垃圾邮件被封禁SMTP服务。
            var DB = Context.Request.HttpContext.RequestServices.GetRequiredService<PolemoContext>();
            var time = DateTime.Now.AddHours(-1);
            var cnt = DB.RequestLists
                .Count(x => x.IP == Context.Request.HttpContext.Connection.RemoteIpAddress.ToString() && x.IsFailed && x.Time >= time);
            if (cnt > 10)
                return false;
            var EmailSender = Context.Request.HttpContext.RequestServices.GetRequiredService<IEmailSender>();
            var VerifyCode = (new Random()).Next(1000, 9999);
            await EmailSender.SendEmailAsync(Email, "Retrieve Password Letter", );
            return true;
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
