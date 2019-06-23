using Caliburn.Micro;
using DeMol.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeMol.ViewModels
{
    class SmoelenViewModel : Screen
    {
        private readonly ShellViewModel conductor;
        private readonly SimpleContainer container;
        private AntwoordenData antwoorden;

        public SmoelenViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;
        }
        public void SelectUser(string name)
        {
        }

        public bool CanSelectUser(string name)
        {
            var result = !antwoorden.Spelers.Any(s => s.Naam.SafeEqual(name));
            return result;
        }

        public ICommand SelectUserCommand { get { return new DelegateCommand(SelectUser, CanSelectUser); } }

        protected override void OnActivate()
        {
            antwoorden = Util.SafeReadJson<AntwoordenData>(container.GetInstance<ShellViewModel>().Dag);
            base.OnActivate();
        }
        public void Menu()
        {
            var x = container.GetInstance<MenuViewModel>();
            conductor.ActivateItem(x);
        }

    }
}
