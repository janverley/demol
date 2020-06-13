using System;
using System.Collections.Generic;

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
        public Dictionary<string, string> Antwoorden { get; set; }
        public TimeSpan Tijd { get; set; }
    }
}