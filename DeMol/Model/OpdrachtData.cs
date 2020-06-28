using System.Collections.Generic;

namespace DeMol.Model
{
    public class OpdrachtData
    {
        public string Opdracht { get; set; }
        public string Description { get; set; }
        public List<Vraag> Vragen { get; set; } = new List<Vraag>();
    }
}