using System.Collections.Generic;

namespace DeMol.Model
{
    internal class OpdrachtData
    {
        public string Opdracht { get; set; }
        public string Description { get; set; }
        public List<Vraag> Vragen { get; set; } = new List<Vraag>();

        public Dag GespeeldOpDag { get; set; }

        public bool DeLosIsOntmaskerd { get; set; }
        
        public int MaxTeVerdienenBedrag { get; set; }
        public int VerdiendBedrag { get; set; }

        public SpelerInfo WasMolInDezeOpdracht { get; set; }
        
        
    }
}