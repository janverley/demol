using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMol.Model
{
    public enum OpdrachtStatus
    {
        OK,
        NOK,
        NA
    };

    public class PasvragenVerdiend
    {
        public string Naam { get; set; }
        public int PasVragenVerdiend { get; set; }
    }

    public class AdminData
    {
        public AdminData()
        {
            Pasvragen = new List<PasvragenVerdiend>();
            OpdrachtenGespeeld = new List<string>();
            IsVerteldOfZeDeMolZijn = new List<SpelerInfo>();
            VragenCodes = new List<string>();
        }
        public List<PasvragenVerdiend> Pasvragen { get; set; }
        public List<string> OpdrachtenGespeeld { get; set; }
        public List<SpelerInfo> IsVerteldOfZeDeMolZijn { get; set; }
        public List<string> VragenCodes { get; set; }

    }

    public class FinaleData
    {
        public FinaleData()
        {
            FinaleVragen = new List<FinaleVraag>();
        }
        public List<FinaleVraag> FinaleVragen { get; set; }
    }

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

