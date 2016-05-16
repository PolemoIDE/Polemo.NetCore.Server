using System;
using System.ComponentModel.DataAnnotations;

namespace Pomelo.NetCore.Server.Models
{
    public class RequestDeniedLog
    {
        public Guid Id { get; set; }

        [MaxLength(64)]
        public string IP { get; set; }

        public DateTime Time { get; set; }
    }
}
