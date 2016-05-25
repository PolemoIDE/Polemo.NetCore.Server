using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pomelo.NetCore.Server.Models
{
    public class Project
    {
        public Guid Id { get; set; }
        
        public string Git { get; set; }

        public string Password { get; set; }

        public string Title { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public DateTime UpdatedTime { get; set; } = DateTime.Now;
    }
}
