using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMol.Model
{
    class OpdrachtVragen
    {
        public string Opdracht { get; set; }
        public List<Vraag> Vragen { get; set; } = new List<Vraag>();
    }

    class OpdrachtVragenData
    {
        public List<OpdrachtVragen> OpdrachtVragen { get; set; } = new List<OpdrachtVragen>();
    }
}
