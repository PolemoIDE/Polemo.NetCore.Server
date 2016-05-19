using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.NetCore.Server.Models;
using CodeComb.Net.EmailSender;

namespace Pomelo.NetCore.Server.Hubs
{
    public partial class PomeloHub : Hub
    {
        public async Task<object> SignIn(string username, string password)
        {
            var DB = Context.Request.HttpContext.RequestServices.GetRequiredService<PomeloContext>();
            var SignInManager = Context.Request.HttpContext.RequestServices.GetRequiredService<SignInManager<User>>();
            var UserManager = Context.Request.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();

            // 检查是否请求次数过多
            var time = DateTime.Now.AddHours(-1);
            var cnt = DB.RequestLists
                .Count(x => x.IP == Context.Request.HttpContext.Connection.RemoteIpAddress.ToString() && x.Time >= time);
            if (cnt > 10)
                return false;

            // 执行登录
            var result = await SignInManager.PasswordSignInAsync(username, password, true, false);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByNameAsync(username);
                var token = Guid.NewGuid().ToString();
                await UserManager.AddLoginAsync(user, new UserLoginInfo("Pomelo", token, "Pomelo"));

                // Create & Start VM
                var claims = await UserManager.GetClaimsAsync(user);
                var nodeClaim = claims.Where(x => x.Type == "Owned VM");

                string vmIpAddr;
                if (nodeClaim.Count() == 0)
                {
                    // Create VM
                    var vmcreated = await Program.VMManagetment.CreateVirtualMachineAsync(username, "pomelo", "Pomelo123!@#");
                    var ipStatus = await Program.VMManagetment.GetPublicIPAddressAsync(username);
                    if (!vmcreated || !ipStatus.Item1)
                        return new { IsSucceeded = false, Token = string.Empty, VMIP = string.Empty };
                    vmIpAddr = ipStatus.Item2.ToString();

                    // Create Node, save, add to claim
                    var random = Guid.NewGuid().ToString();
                    var node = new Node { IP = vmIpAddr, PrivateKey = random, Id = Guid.NewGuid(), Alias = random };
                    DB.Nodes.Add(node);
                    await DB.SaveChangesAsync();
                    await UserManager.AddClaimAsync(user, new System.Security.Claims.Claim("Owned VM", node.Id.ToString()));
                }
                else
                {
                    var nodeId = Guid.Parse(nodeClaim.First().Value);
                    var node = DB.Nodes.Where(x => x.Id == nodeId).First();
                    vmIpAddr = node.IP;

                    // Start VM
                    var vmStarted = await Program.VMManagetment.StartVirtualMachineAsync(username);
                    if (!vmStarted)
                        return new { IsSucceeded = false, Token = string.Empty, VMIP = string.Empty };
                }

                return new { IsSucceeded = true, Token = token, VMIP = vmIpAddr };
            }
            else
            {
                DB.RequestLists.Add(new RequestDeniedLog { IP = Context.Request.HttpContext.Connection.RemoteIpAddress.ToString(), Time = DateTime.Now });
                DB.SaveChanges();
                return new { IsSucceeded = false, Token = string.Empty, VMIP = string.Empty };
            }

        }

        public async void SignOut()
        {
            await Program.VMManagetment.DeallocateVirtualMachineAsync(Context.User.Identity.Name);

            var SignInManager = Context.Request.HttpContext.RequestServices.GetRequiredService<SignInManager<User>>();
            await SignInManager.SignOutAsync();
        }

        public async Task<object> TokenSignIn(string token)
        {
            var DB = Context.Request.HttpContext.RequestServices.GetRequiredService<PomeloContext>();
            var SignInManager = Context.Request.HttpContext.RequestServices.GetRequiredService<SignInManager<User>>();
            var UserManager = Context.Request.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();

            // 检查是否请求次数过多
            var time = DateTime.Now.AddHours(-1);
            var cnt = DB.RequestLists
                .Count(x => x.IP == Context.Request.HttpContext.Connection.RemoteIpAddress.ToString() && x.Time >= time);
            if (cnt > 10)
                return new { IsSucceeded = false, VMIP = string.Empty };

            // 执行登录
            var result = await SignInManager.ExternalLoginSignInAsync("Pomelo", token, true);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByLoginAsync("Pomelo", token);
                var claims = await UserManager.GetClaimsAsync(user);
                var nodeClaim = claims.Where(x => x.Type == "Owned VM");

                string vmIpAddr;
                if (nodeClaim.Count() == 0)
                {
                    // Create VM
                    var vmcreated = await Program.VMManagetment.CreateVirtualMachineAsync(user.UserName, "pomelo", "Pomelo123!@#");
                    var ipStatus = await Program.VMManagetment.GetPublicIPAddressAsync(user.UserName);
                    if (!vmcreated || !ipStatus.Item1)
                        return new { IsSucceeded = false, VMIP = string.Empty };
                    vmIpAddr = ipStatus.Item2.ToString();

                    // Create Node, save, add to claim
                    var random = Guid.NewGuid().ToString();
                    var node = new Node { IP = vmIpAddr, PrivateKey = random, Id = Guid.NewGuid(), Alias = random };
                    DB.Nodes.Add(node);
                    await DB.SaveChangesAsync();
                    await UserManager.AddClaimAsync(user, new System.Security.Claims.Claim("Owned VM", node.Id.ToString()));
                }
                else
                {
                    var nodeId = Guid.Parse(nodeClaim.First().Value);
                    var node = DB.Nodes.Where(x => x.Id == nodeId).First();
                    vmIpAddr = node.IP;

                    // Start VM
                    var vmStarted = await Program.VMManagetment.StartVirtualMachineAsync(user.UserName);
                    if (!vmStarted)
                        return new { IsSucceeded = false, VMIP = string.Empty };
                }
                return new { IsSucceeded = true, VMIP = vmIpAddr };
            }
            else
            {
                return new { IsSucceeded = false, VMIP = string.Empty };
            }
        }

        public async Task<bool> ForgotVerifyEmail(string Email)
        {
            var DB = Context.Request.HttpContext.RequestServices.GetRequiredService<PomeloContext>();
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

        public async Task<bool> Forgot(string Email, int Code, string Password)
        {
            var DB = Context.Request.HttpContext.RequestServices.GetRequiredService<PomeloContext>();
            var UserManager = Context.Request.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();

            // 查找验证码
            var code = DB.VerifyCodes.SingleOrDefault(x => x.Email == Email && x.Code == Code && DateTime.Now < x.Expire && !x.IsUsed);
            if (code == null)
                return false;

            // 更新验证码信息
            code.IsUsed = true;
            code.Expire = DateTime.Now;
            DB.SaveChanges();

            // 更改密码
            var user = await UserManager.FindByEmailAsync(Email);
            var token = await UserManager.GeneratePasswordResetTokenAsync(user);
            await UserManager.ResetPasswordAsync(user, token, Password);
            return true;
        }

        public async Task<bool> ResetPassword(string currentpwd, string newpwd)
        {
            var UserManager = Context.Request.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
            var SignInManager = Context.Request.HttpContext.RequestServices.GetRequiredService<SignInManager<User>>();

            var result = await UserManager.ChangePasswordAsync(await UserManager.FindByNameAsync(Context.Request.HttpContext.User.Identity.Name), currentpwd, newpwd);
            if (result.Succeeded)
            {
                await SignInManager.SignOutAsync();
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> RegisterVerifyEmail(string email)
        {
            var DB = Context.Request.HttpContext.RequestServices.GetRequiredService<PomeloContext>();
            var EmailSender = Context.Request.HttpContext.RequestServices.GetRequiredService<IEmailSender>();

            // 将先前的验证码作废
            var codes = DB.VerifyCodes
                .Where(x => x.Email == email && x.Type == VerifyCodeType.Register && x.Expire > DateTime.Now && !x.IsUsed)
                .ToList();
            foreach (var c in codes)
                c.Expire = DateTime.Now;

            // 将验证码信息写入数据库
            var VerifyCode = new VerifyCode { Expire = DateTime.Now.AddHours(1), Email = email, Code = (new Random()).Next(1000, 9999), Type = VerifyCodeType.Register };
            DB.VerifyCodes.Add(VerifyCode);

            DB.SaveChanges();

            await EmailSender.SendEmailAsync(email, "Retrieve Password Letter", "Your code is: " + VerifyCode.Code);
            return true;
        }

        public async Task<bool> Register(string email, int Verifycode, string username, string password)
        {
            var DB = Context.Request.HttpContext.RequestServices.GetRequiredService<PomeloContext>();
            var UserManager = Context.Request.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
            var user = DB.Users.Where(x => x.UserName == username).SingleOrDefault();
            if (user != null)
            {
                return false;
            }
            else
            {
                var code = DB.VerifyCodes.Where(x => x.Type == VerifyCodeType.Register && x.Email == email && x.Code == Verifycode).SingleOrDefault();
                if (code != null)
                {
                    var newuser = new User
                    {
                        Email = email,
                        UserName = username,
                    };
                    await UserManager.CreateAsync(newuser, password);
                    var codes = DB.VerifyCodes
                     .Where(x => x.Email == email && x.Type == VerifyCodeType.Register && x.Expire > DateTime.Now && !x.IsUsed)
                     .ToList();
                    foreach (var c in codes)
                        c.Expire = DateTime.Now;
                    var VerifyCode = new VerifyCode { Expire = DateTime.Now.AddHours(1), Email = email, Code = (new Random()).Next(1000, 9999), Type = VerifyCodeType.Register };
                    DB.VerifyCodes.Add(VerifyCode);
                    DB.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        public object GetProjectTemplates()
        {
            var DB = Context.Request.HttpContext.RequestServices.GetRequiredService<PomeloContext>();
            var templates = DB.Templates
                .OrderBy(x => x.Id)
                .ToList();
            if (templates.Count() != 0)
            {
                foreach (var x in templates)
                {
                    return new { Title = x.Title, URL = x.URL, Description = x.Description };
                }
                return true;
            }
            else
            {
                return false;
            }

        }

        public object GetProjects()
        {
            var DB = Context.Request.HttpContext.RequestServices.GetRequiredService<PomeloContext>();
            var projects = DB.Projects
                .Where(x => x.UserName == Context.Request.HttpContext.User.Identity.Name)
                .OrderByDescending(x => x.UpdatedTime)
                .ToList();
            if (projects.Count() != 0)
            {
                foreach (var x in projects)
                {
                    return new
                    {
                        Project = x.Title,
                        Git = x.Git,
                        SshKey = x.SSH,
                        Name = x.UserName,
                        Email = x.Email
                    };
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CreateProject(string PTitle, string Git, string SshKey, string Email)
        {
            var DB = Context.Request.HttpContext.RequestServices.GetRequiredService<PomeloContext>();
            var user = DB.Users.Where(x => x.UserName == Context.Request.HttpContext.User.Identity.Name)
                .SingleOrDefault();
            var project = new Project
            {
                Title = PTitle,
                Git = Git,
                SSH = SshKey,
                UserName = user.UserName,
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
