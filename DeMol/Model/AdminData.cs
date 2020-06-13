using System.Collections.Generic;

namespace DeMol.Model
{
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
}