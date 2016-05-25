using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace Pomelo.NetCore.Server
{
    public class Program
    {
        public static Azure.VMManagement VMManagetment { get; private set; }

        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseStartup<Startup>()
                    .Build();

            VMManagetment = new Azure.VMManagement("6fef287b-09fc-4d87-8dc1-bb154aa68b7a", "821e2823-d712-46fb-885c-46cc60d8ee66",
                "60b8650e-26f1-4782-b740-955f551d0776", "7H2sjm1GG7qf3+cSZ3sGl7VivwstyhWeTkEkS+ENIOw=",
                "https://pomeloide.blob.core.windows.net/system/Microsoft.Compute/Images/image/node-osDisk.a6ec0791-e772-405b-ab17-d7fbbaf67390.vhd");

            host.Run();
        }
    }
}
