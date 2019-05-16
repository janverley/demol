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


    public class Opdrachten
    {
        public OpdrachtStatus OpdrachtStatus1 { get; set; }
        public OpdrachtStatus OpdrachtStatus2 { get; set; }
        public OpdrachtStatus OpdrachtStatus3 { get; set; }
    }
    public class PasvragenVerdiend
    {
        public string Naam { get; set; }
        public int PasVragenVerdiend { get; set; }
    }

    public class AdminData
    {
        public AdminData()
        {
            Opdrachten = new Opdrachten { OpdrachtStatus1 = OpdrachtStatus.NOK, OpdrachtStatus2 = OpdrachtStatus.NA, OpdrachtStatus3 = OpdrachtStatus.NA };
            Pasvragen = new List<PasvragenVerdiend>();
        }

        public Opdrachten Opdrachten { get; set; }
        public List<PasvragenVerdiend> Pasvragen { get; set; }
    }
}
