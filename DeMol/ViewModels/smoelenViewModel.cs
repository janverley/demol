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
            Naam = name;
            DoNext(this);
        }

        public bool CanSelectUser(string name)
        {
            return CanSelectUserDelegate(name);
        }

        public DelegateCommand SelectUserCommand { get { return new DelegateCommand(SelectUser, CanSelectUser); } }

        public Action<SmoelenViewModel> DoNext { get; set; }// = vm => { };

        public Func<string, bool> CanSelectUserDelegate { get; set; }// = vm => { };

        public string Naam { get; set; }

        public void Menu()
        {
            var x = container.GetInstance<MenuViewModel>();
            conductor.ActivateItem(x);
        }

        protected override void OnActivate()
        {
            SelectUserCommand.RaiseCanExecuteChanged();
            base.OnActivate();
        }
    }
}
