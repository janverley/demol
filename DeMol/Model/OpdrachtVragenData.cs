using System.Collections.Generic;

namespace DeMol.Model
{
    internal class OpdrachtVragenData
    {
        public string Opdracht { get; set; }
        public string Description { get; set; }
        public List<Vraag> Vragen { get; set; } = new List<Vraag>();
    }
}