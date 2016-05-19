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
        private IDictionary<string, string> UserConnectionIdDictionary = new Dictionary<string, string>();

        public override Task OnConnected()
        {
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            // FIXME: change it
            var username = "";
            UserConnectionIdDictionary.Add(username, Context.ConnectionId);

            Task.Factory.StartNew(async () =>
            {
                await Task.Delay(600000);


                await Program.VMManagetment.DeallocateVirtualMachineAsync(username);
            });

            return base.OnDisconnected(stopCalled);
        }
    }
}
