using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Polemo.NetCore.Server.Models
{
    public class Template
    {
        public Guid Id { get; set; }

        public int PRI { get; set; }

        [MaxLength(64)]
        public string Title { get; set; }

        public string Description { get; set; }

        [MaxLength(128)]
        public string URL { get; set; }
    }
}
