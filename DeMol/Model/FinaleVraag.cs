namespace DeMol.Model
{
    public class FinaleVraag
    {
        public Dag Dag { get; set; }
        public Vraag Vraag { get; set; }
        public string VraagCode { get; set; }
        public string Opdracht { get; set; }
        public string Description { get; set; }
        public string JuistAntwoord { get; set; }
    }
}