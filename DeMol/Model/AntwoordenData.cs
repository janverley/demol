using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMol.Model
{
    public class Speler
    {
        public Speler()
        {
            Antwoorden = new Dictionary<string, string>();
        }
        public bool IsDeMol { get; set; }
        public string Naam { get; set; }
        public string DeMolIs { get; set; }
        public Dictionary<string,string> Antwoorden { get; set; }
        public TimeSpan Tijd { get; set; }
    }

    public class AntwoordenData
    {
        public string Dag { get; set; }
        public List<Speler> Spelers { get; set; } = new List<Speler>();
    }
}
