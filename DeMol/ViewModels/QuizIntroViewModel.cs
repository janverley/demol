using Admin.Model;
using Caliburn.Micro;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Admin.ViewModels
{
    public class QuizIntroViewModel : Screen
    {
        public QuizIntroViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;
        }

        private string naam;
        private readonly ShellViewModel conductor;
        private readonly SimpleContainer container;

        public string Naam
        {
            get { return naam; }
            set
            {
                if (Set(ref naam, value))
                {
                    NotifyOfPropertyChange(() => CanStart);
                }
            }
        }

        protected override void OnActivate()
        {
            base.OnActivate();

        }

        public bool CanStart => !string.IsNullOrEmpty(Naam);
        public void Start()
        {
            var x = container.GetInstance<QuizVragenViewModel>();
            x.Naam = Naam;
            conductor.ActivateItem(x);

        }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (e?.Key == Key.Enter && CanStart)
            {
                Start();
            }
            if (e?.Key == Key.Escape )
            {
                Menu();
            }
        }

        public void Menu()
        {
            var x = container.GetInstance<MenuViewModel>();
            conductor.ActivateItem(x);
        }
    }
}
