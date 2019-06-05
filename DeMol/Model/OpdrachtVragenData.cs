using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMol.Model
{
    class OpdrachtVragenData
    {
        public string Opdracht { get; set; }
        public string Description { get; set; }
        public List<Vraag> Vragen { get; set; } = new List<Vraag>();
    }
}
