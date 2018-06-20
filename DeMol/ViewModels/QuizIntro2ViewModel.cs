using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeMol.ViewModels
{
    class QuizIntro2ViewModel : Screen
    {
        private readonly ShellViewModel conductor;
        private readonly SimpleContainer container;
        private string naam;
        private bool isDeMol;

        public QuizIntro2ViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;

            var ja = new OptieViewModel("Ja");
            ja.PropertyChanged += Optie_PropertyChanged;
            var nee = new OptieViewModel("Nee");
            nee.PropertyChanged += Optie_PropertyChanged;

            Opties = new BindableCollection<OptieViewModel> { ja,nee };
        }

        private void Optie_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OptieViewModel.IsSelected))
            {
                NotifyOfPropertyChange(() => CanStart);
            }
        }

        public BindableCollection<OptieViewModel> Opties { get; set; }

        public string Naam
        {
            get { return naam; }
            set
            {
                if (Set(ref naam, value))
                {
                    NotifyOfPropertyChange(() => BenJijDeMol);
                }
            }
        }

        public string BenJijDeMol => $"{Naam}, was jij vandaag De Mol?";

        public bool IsDeMol
        {
            get { return isDeMol; }
            set
            {
                if (Set(ref isDeMol, value))
                {
                }
            }
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (e?.Key == Key.Enter && CanStart)
            {
                Start();
            }
            if (e?.Key == Key.Escape)
            {
                Menu();
            }
        }

        public bool CanStart => Opties.Any(o => o.IsSelected);
        public void Start()
        {
            var x = container.GetInstance<QuizVragenViewModel>();
            x.Naam = Naam;
            x.IsDeMol = IsDeMol;
            conductor.ActivateItem(x);
        }
        public void Menu()
        {
            var x = container.GetInstance<MenuViewModel>();
            conductor.ActivateItem(x);
        }

    }
}
