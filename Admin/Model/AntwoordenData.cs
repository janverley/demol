using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.Model
{
    public class Speler
    {
        public string Naam { get; set; }
        public List<string> Antwoorden { get; set; }
        public TimeSpan Tijd { get; set; }
    }

    public class AntwoordenData
    {
        public string Dag { get; set; }
        public List<Speler> Spelers { get; set; }
    }
}
