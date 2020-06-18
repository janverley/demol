using System.Collections.Generic;

namespace DeMol.Model
{
    public class OpdrachtData
    {
        public string Opdracht { get; set; }
        public string Description { get; set; }
        public List<Vraag> Vragen { get; set; } = new List<Vraag>();

        public int GespeeldOpDag { get; set; }

        public bool DeMolIsOntmaskerd { get; set; }
        
        public int MaxTeVerdienenBedrag { get; set; }
        
        public int VerdiendBedrag { get; set; }

        public SpelerInfo WasMolInDezeOpdracht { get; set; }
        
        
    }
}