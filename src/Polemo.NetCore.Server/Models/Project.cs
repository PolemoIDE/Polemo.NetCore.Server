using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Polemo.NetCore.Server.Models
{
    public class Project
    {
        public string Git { get; set; }

        public string SSH { get; set; }

        public string Title { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public DateTime UpdatedTime { get; set; } = DateTime.Now;
    }
}
