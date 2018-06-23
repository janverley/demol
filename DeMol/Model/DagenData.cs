using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMol.Model
{
    public class Dag
    {
        public int Id { get; set; }
        public string Naam { get; set; }
    }

    public class DagenData
    {
        public List<Dag> Dagen { get; set; }
    }
}
