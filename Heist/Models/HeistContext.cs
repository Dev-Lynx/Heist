using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heist.Models
{
    public class HeistContext
    {
        public string Url { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
    }
}
