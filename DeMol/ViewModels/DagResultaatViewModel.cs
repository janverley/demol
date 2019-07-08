using Caliburn.Micro;
using DeMol.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMol.ViewModels
{
    class DagResultaatViewModel : Screen
    {
        private readonly ShellViewModel conductor;
        private readonly SimpleContainer container;
        private string text;

        public DagResultaatViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;
        }

        public string Text
        {
            get { return text; }
            set { Set(ref text, value); }
        }
        public void Menu()
        {
            var x = container.GetInstance<MenuViewModel>();
            conductor.ActivateItem(x);
        }

        protected override void OnActivate()
        {
        

            base.OnActivate();
        }
    }
}
