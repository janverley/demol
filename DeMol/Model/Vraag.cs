using System.Collections.Generic;

namespace DeMol.Model
{
    public class Vraag
    {
        public bool MeerdereOptiesMogelijk;
        public string Text { get; set; }
        public List<string> Opties { get; set; } = new List<string>();
        
        
    }

}