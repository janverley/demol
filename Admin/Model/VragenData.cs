using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.Model
{
    public class Vraag
    {
        public string Text { get; set; }
        public List<string> Opties { get; set; } = new List<string>();
    }

    public class VragenData
    {
        public string Dag { get; set; }
        public List<Vraag> Vragen { get; set; } = new List<Vraag>();
    }
}
