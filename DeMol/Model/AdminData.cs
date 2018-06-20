using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMol.Model
{
    public enum Status
    {
        OK,
        NOK,
        NA
    };


    public class Opdrachten
    {
        public Status op1 { get; set; }
        public Status op2 { get; set; }
        public Status op3 { get; set; }
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
            Opdrachten = new Opdrachten();
            Pasvragen = new List<PasvragenVerdiend>();
        }

        public Opdrachten Opdrachten { get; set; }
        public List<PasvragenVerdiend> Pasvragen { get; set; }
    }
}
