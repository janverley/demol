using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMol.ViewModels
{
    public class OpdrachtViewModel : PropertyChangedBase
    {
        private string naam;
        private bool vandaagGespeeld;

        public string Naam
        {
            get { return naam; }
            set { Set(ref naam, value); }
        }

        public bool VandaagGespeeld
        {
            get { return vandaagGespeeld; }
            set { Set(ref vandaagGespeeld, value); }
        }

    }
}
