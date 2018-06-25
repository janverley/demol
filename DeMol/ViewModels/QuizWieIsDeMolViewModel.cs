using Caliburn.Micro;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

namespace DeMol.ViewModels
{
    public class QuizWieIsDeMolViewModel : Screen
    {
        private readonly ShellViewModel conductor;
        private readonly SimpleContainer container;
        private string naam;

        public QuizWieIsDeMolViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;


            var spelers = container.GetInstance<ShellViewModel>().Spelerdata.Spelers;
            foreach (var speler in spelers)
            {
                var optie = new OptieViewModel(speler.Naam);
                optie.PropertyChanged += Optie_PropertyChanged;

                Opties.Add(optie);
            }
        }

        private void Optie_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OptieViewModel.IsSelected))
            {
                NotifyOfPropertyChange(() => CanStart);
            }
        }

        public BindableCollection<OptieViewModel> Opties { get; set; } = new BindableCollection<OptieViewModel>();
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

        public string Text => $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Naam.ToLower())}, wie denk jij dat vandaag De Mol was?";

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
            x.IsDeMol = false;
            x.DeMolIs = Opties.Single(o => o.IsSelected).OptieText;
            conductor.ActivateItem(x);
        }
        public void Menu()
        {
            var x = container.GetInstance<MenuViewModel>();
            conductor.ActivateItem(x);
        }
    }
}