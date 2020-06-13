using System.Collections.Generic;

namespace DeMol.Model
{
    public class AntwoordenData
    {
        public string Dag { get; set; }
        public List<Speler> Spelers { get; set; } = new List<Speler>();
    }
}