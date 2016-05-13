using System;
using System.ComponentModel.DataAnnotations;

namespace Polemo.NetCore.Server.Models
{
    public class RequestList
    {
        public Guid Id { get; set; }

        [MaxLength(64)]
        public string IP { get; set; }

        public bool IsFailed { get; set; }

        public DateTime Time { get; set; }
    }
}
