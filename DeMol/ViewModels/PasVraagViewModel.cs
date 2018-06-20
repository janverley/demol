using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.ViewModels
{
    public class PasVraagViewModel : PropertyChangedBase
    {
        private string naam;
        private int pasvragenVerdiend;

        public string Naam
        {
            get { return naam; }
            set { Set(ref naam, value); }
        }

        public int PasVragenVerdiend
        {
            get { return pasvragenVerdiend; }
            set { Set(ref pasvragenVerdiend, value); }
        }
    }
}
