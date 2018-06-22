using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMol.ViewModels
{
    public class CheckViewModel:PropertyChangedBase
    {
        public CheckViewModel(string text, bool isOk)
        {
            Text = text;
            IsOk = isOk;
        }

        public string Text { get; }
        public bool IsOk { get; }
    }
}
