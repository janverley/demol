using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMol.Model
{
    class MollenData
    {
        public List<Mol> Mollen { get; set; }
    }

    public class Mol
    {
        public int DagId { get; set; }
        public string Naam { get; set; }
    }
}
