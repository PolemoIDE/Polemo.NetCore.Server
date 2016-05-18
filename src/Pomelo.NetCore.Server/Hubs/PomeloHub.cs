using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.NetCore.Server.Models;
using Pomelo.NetCore.Azure;

namespace Pomelo.NetCore.Server.Hubs
{
    public partial class PomeloHub : Hub
    {
        public override async Task OnConnected()
        {
            var DB = Context.Request.HttpContext.RequestServices.GetRequiredService<PomeloContext>();
            //var SignInManager = Context.Request.HttpContext.RequestServices.GetRequiredService<SignInManager<User>>();
            var UserManager = Context.Request.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
            var User = await UserManager.FindByNameAsync(Context.User.Identity.Name);
            var Claims = await UserManager.GetClaimsAsync(User);

            var NodeClaim = Claims.Where(x => x.Type == "Owned VM");
            if (NodeClaim.Count() == 0)
            {
                VMManagement VMMgr = new VMManagement("subscriptionId", "tenantId", "clientId", "appPassword");
                //VMMgr.CreateVirtualMachineAsync()
            }
            else
            {
                var NodeId = Guid.Parse(NodeClaim.First().Value);
                var Node = DB.Nodes.Where(x => x.Id == NodeId).First();
                var IP = Node.IP;

                // Start VM
            }


            await base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            return base.OnDisconnected(stopCalled);
        }
    }
}
