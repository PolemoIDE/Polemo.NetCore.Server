using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Polemo.NetCore.Server.Models
{
    public class VerifyCode
    {
        public Guid Id { get; set; }

        public string Email { get; set; }

        public string Code { get; set; }

        public DateTime Expire { get; set; }

        public bool IsUsed { get; set; }
    }
}
