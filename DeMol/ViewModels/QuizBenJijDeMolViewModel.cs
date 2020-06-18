using Caliburn.Micro;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using DeMol.Model;

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

        public string BenJijDeMol => $"{Naam}, was jij De Mol bij opdracht {Opdracht}?";
        public string Opdracht => Util.OpdrachtUINaam(OpdrachtData);

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

        public bool IsDeMol => optieJa.IsSelected;
        public bool CanStart => Opties.Any(o => o.IsSelected);
        public OpdrachtData OpdrachtData { get; set; }

        public void Start()
        {
            var x = this.Parent as QuizLoopViewModel;
            x.OnQuizBenJijDeMolViewModelClose(Naam, OpdrachtData, IsDeMol);
            TryClose();

            //conductor.DeactivateItem(this, true);

            //OnDeactivate(true);
            // if (optieJa.IsSelected)
            // {
            //     var x = container.GetInstance<QuizVragenViewModel>();
            //     x.Naam = Naam;
            //     x.IsDeMol = true;
            //     conductor.ActivateItem(x);
            // }
            // else
            // {
            //     var x = container.GetInstance<QuizWieIsDeMolViewModel>();
            //     x.Naam = Naam;
            //     conductor.ActivateItem(x);
            // }
        }
        public void Menu()
        {
            var x = container.GetInstance<MenuViewModel>();
            conductor.ActivateItem(x);
        }

    }
}

