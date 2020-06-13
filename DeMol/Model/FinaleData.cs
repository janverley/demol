using System.Collections.Generic;

namespace DeMol.Model
{
    public class FinaleData
    {
        public FinaleData()
        {
            FinaleVragen = new List<FinaleVraag>();
        }

        public List<FinaleVraag> FinaleVragen { get; set; }
    }
}