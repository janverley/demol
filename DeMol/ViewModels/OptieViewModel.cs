using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMol.ViewModels
{
    public class OptieViewModel : PropertyChangedBase
    {
        private string optieText;
        private bool isSelected;

        public OptieViewModel(string optieText)
        {
            this.optieText = optieText;
        }

        public string OptieText
        {
            get { return optieText; }
            set
            {
                Set(ref optieText, value);
            }
        }

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                Set(ref isSelected, value);
            }
        }


    }
}
