using System;
using Caliburn.Micro;

namespace DeMol.ViewModels
{
    internal class SmoelenViewModel : Screen
    {
        private readonly ShellViewModel conductor;
        private readonly SimpleContainer container;

        public SmoelenViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;
        }

        public DelegateCommand SelectUserCommand => new DelegateCommand(SelectUser, CanSelectUser);

        public Action<SmoelenViewModel> DoNext { get; set; } // = vm => { };

        public Func<string, bool> CanSelectUserDelegate { get; set; } // = vm => { };

        public string Naam { get; set; }

        public void SelectUser(string name)
        {
            Naam = name;
            DoNext(this);
        }

        public bool CanSelectUser(string name)
        {
            return CanSelectUserDelegate(name);
        }

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