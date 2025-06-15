using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using Caliburn.Micro;
using DeMol.Model;

namespace DeMol.ViewModels
{
    public class QuizWieIsDeMolViewModel : Screen
    {
        private readonly ShellViewModel conductor;
        private readonly SimpleContainer container;
        private string naam;
        private OpdrachtData opdrachtData;

        public QuizWieIsDeMolViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;
        }

        public BindableCollection<OptieViewModel> Opties { get; set; } = new BindableCollection<OptieViewModel>();

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

        public string Text =>
            $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Naam.ToLower())}, wie denk jij dat {Opdracht} was?";

        public string Opdracht => Util.OpdrachtUiNaam(OpdrachtData);

        public OpdrachtData OpdrachtData
        {
            get => opdrachtData;
            set
            {
                opdrachtData = value;

                var opdrachtIsOverMol = opdrachtData.Opdracht.SafeEqual("a");
                var spelers = container.GetInstance<ShellViewModel>().Spelerdata.Spelers;
                foreach (var speler in spelers.Where(s => s.KanMolZijn == opdrachtIsOverMol))
                {
                    var optie = new OptieViewModel(speler.Naam);
                    optie.PropertyChanged += Optie_PropertyChanged;

                    Opties.Add(optie);
                }
            }
        }

        public bool CanStart => Opties.Any(o => o.IsSelected);
        public Action<QuizWieIsDeMolViewModel> DoNext { get; set; }

        public string DeMolIs => Opties.Single(o => o.IsSelected).OptieText;

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