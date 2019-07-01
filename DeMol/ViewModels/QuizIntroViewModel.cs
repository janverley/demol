using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeMol.ViewModels
{
    class QuizIntroViewModel : Screen
    {
        private readonly ShellViewModel conductor;
        private readonly SimpleContainer container;
        private string naam;

        public QuizIntroViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;

            
        }

        public string Naam
        {
            get { return naam; }
            set
            {
                if (Set(ref naam, value))
                {
                    var dag = container.GetInstance<ShellViewModel>().Dag;
                    ditIsDeMolVandaag = container.GetInstance<ShellViewModel>().IsDeMol(dag, Naam);
                    NotifyOfPropertyChange(nameof(Text));
                }
            }
        }

        public string Text => ditIsDeMolVandaag ? $"{Naam}, jij was vandaag De Mol" : $"{Naam}, jij was vandaag niet De Mol";

        public bool ditIsDeMolVandaag { get; private set; }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (e?.Key == Key.Enter)
            {
                Start();
            }
            if (e?.Key == Key.Escape)
            {
                Menu();
            }
        }

        public void Start()
        {
            if (ditIsDeMolVandaag)
            {
                var x = container.GetInstance<QuizVragenViewModel>();
                x.Naam = Naam;
                x.IsDeMol = true;
                conductor.ActivateItem(x);
            }
            else
            {
                var x = container.GetInstance<QuizWieIsDeMolViewModel>();
                x.Naam = Naam;
                conductor.ActivateItem(x);
            }
        }
        public void Menu()
        {
            var x = container.GetInstance<MenuViewModel>();
            conductor.ActivateItem(x);
        }
    }
}
