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
        }
        public List<PasvragenVerdiend> Pasvragen { get; set; }
        public List<string> OpdrachtenGespeeld { get; set; }
        public List<SpelerInfo> IsVerteldOfZeDeMolZijn { get; set; }

    }
}
