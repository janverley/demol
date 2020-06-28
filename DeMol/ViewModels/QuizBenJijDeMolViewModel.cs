using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Caliburn.Micro;
using DeMol.Model;

namespace DeMol.ViewModels
{
    internal class QuizBenJijDeMolViewModel : Screen
    {
        private readonly ShellViewModel conductor;
        private readonly SimpleContainer container;
        private string naam;

        private readonly OptieViewModel optieJa = new OptieViewModel("Ja");
        private readonly OptieViewModel optieNee = new OptieViewModel("Nee");


        public QuizBenJijDeMolViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;

            optieJa.PropertyChanged += Optie_PropertyChanged;
            optieNee.PropertyChanged += Optie_PropertyChanged;

            Opties = new BindableCollection<OptieViewModel> {optieJa, optieNee};
        }

        public BindableCollection<OptieViewModel> Opties { get; set; }

        public string Naam
        {
            get => naam;
            set
            {
                if (Set(ref naam, value))
                {
                }
            }
        }

        public string BenJijDeMol => $"{Naam}, was jij De Mol bij opdracht {Opdracht}?";
        public string Opdracht => Util.OpdrachtUiNaam(OpdrachtData);

        public bool IsDeMol => optieJa.IsSelected;
        public bool CanStart => Opties.Any(o => o.IsSelected);
        public OpdrachtData OpdrachtData { get; set; }

        public Action<QuizBenJijDeMolViewModel> DoNext { get; set; }

        private void Optie_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OptieViewModel.IsSelected))
            {
                NotifyOfPropertyChange(() => CanStart);
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

        public void Start()
        {
            DoNext(this);
        }

        public void Menu()
        {
            var x = container.GetInstance<MenuViewModel>();
            conductor.ActivateItem(x);
        }
    }
}