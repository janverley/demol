using System;
using System.Collections.Generic;

namespace DeMol.Model
{
    public class FinaleSpeler
    {
        public FinaleSpeler()
        {
            DeMolIsPerOpdrachtId = new Dictionary<string, string>();
            AntwoordenPerVraagId = new Dictionary<string, string>();
        }
        public string Naam { get; set; }

        public Dictionary<string, string> DeMolIsPerOpdrachtId { get; set; }
        public Dictionary<string, string> AntwoordenPerVraagId { get; set; }
        public TimeSpan Tijd { get; set; }
    }
}