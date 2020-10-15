using System.Collections.Generic;

namespace DeMol.Model
{
    public class AntwoordenData
    {
        public string OpdrachtId { get; set; }
        public string Dag { get; set; }

        public int MaxTeVerdienen { get; set; }
        public int EffectiefVerdiend { get; set; }

        public List<Speler> Spelers { get; set; } = new List<Speler>();
    }
}