using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeMol.ViewModels
{
    class QuizBenJijDeMolViewModel : Screen
    {
        private readonly ShellViewModel conductor;
        private readonly SimpleContainer container;
        private string naam;

        OptieViewModel optieJa = new OptieViewModel("Ja");
        OptieViewModel optieNee = new OptieViewModel("Nee");


        public QuizBenJijDeMolViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;

            optieJa.PropertyChanged += Optie_PropertyChanged;
            optieNee.PropertyChanged += Optie_PropertyChanged;

            Opties = new BindableCollection<OptieViewModel> { optieJa,optieNee };
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
                }
            }
        }

        public string BenJijDeMol => $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Naam.ToLower())}, was jij vandaag De Mol?";

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
            if (optieJa.IsSelected)
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
