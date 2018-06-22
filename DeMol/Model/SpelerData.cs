using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMol.Model
{
    public class SpelerInfo
    {
        public string Naam { get; set; }
    }

    public class SpelersData
    {
        public List<SpelerInfo> Spelers { get; set; }
    }
}
