using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.Model
{
    public class AntwoordenData
    {
        public string Dag { get; set; }
        public string Deelnemer { get; set; }
        public List<string> Antwoorden { get; set; }
    }
}
