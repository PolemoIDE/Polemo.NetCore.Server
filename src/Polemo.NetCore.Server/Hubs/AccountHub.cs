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
            var DB = Context.Request.HttpContext.RequestServices.GetRequiredService<PolemoContext>();
            var SignInManager = Context.Request.HttpContext.RequestServices.GetRequiredService<SignInManager<User>>();
            var UserManager = Context.Request.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();

            // 检查是否请求次数过多
            var time = DateTime.Now.AddHours(-1);
            var cnt = DB.RequestLists
                .Count(x => x.IP == Context.Request.HttpContext.Connection.RemoteIpAddress.ToString() && x.Time >= time);
            if (cnt > 10)
                return false;

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
                DB.RequestLists.Add(new RequestDeniedLog { IP = Context.Request.HttpContext.Connection.RemoteIpAddress.ToString(), Time = DateTime.Now });
                DB.SaveChanges();
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
            var DB = Context.Request.HttpContext.RequestServices.GetRequiredService<PolemoContext>();
            var SignInManager = Context.Request.HttpContext.RequestServices.GetRequiredService<SignInManager<User>>();
            
            // 检查是否请求次数过多
            var time = DateTime.Now.AddHours(-1);
            var cnt = DB.RequestLists
                .Count(x => x.IP == Context.Request.HttpContext.Connection.RemoteIpAddress.ToString() && x.Time >= time);
            if (cnt > 10)
                return false;

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
            var DB = Context.Request.HttpContext.RequestServices.GetRequiredService<PolemoContext>();
            var EmailSender = Context.Request.HttpContext.RequestServices.GetRequiredService<IEmailSender>();
            
            // 检查Email是否存在
            if (DB.Users.Count(x => x.Email == Email) == 0)
                return false;

            // 将先前的验证码作废
            var codes = DB.VerifyCodes
                .Where(x => x.Email == Email && x.Type == VerifyCodeType.Forgot && x.Expire > DateTime.Now && !x.IsUsed)
                .ToList();
            foreach (var c in codes)
                c.Expire = DateTime.Now;

            // 将验证码信息写入数据库
            var VerifyCode = new VerifyCode { Expire = DateTime.Now.AddHours(1), Email = Email, Code = (new Random()).Next(1000, 9999), Type = VerifyCodeType.Forgot };
            DB.VerifyCodes.Add(VerifyCode);

            DB.SaveChanges();

            await EmailSender.SendEmailAsync(Email, "Retrieve Password Letter", "Your code is: " + VerifyCode.Code);
            return true;
        }

        public string Register(string email)
        {
            
        }
    }
}
