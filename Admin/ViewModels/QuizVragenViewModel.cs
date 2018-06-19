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

namespace Admin.ViewModels
{
    class QuizVragenViewModel : Screen
    {
        private string naam;
        private int dag;

        public string Naam
        {
            get { return naam; }
            set { Set(ref naam, value); }
        }
        public int Dag
        {
            get { return dag; }
            set
            {
                Set(ref dag, value);
            }
        }

        private DateTime startTime;
        private List<QuizVraagViewModel> quizVraagViewModels = new List<QuizVraagViewModel>();
        private Speler speler;
        private QuizVraagViewModel quizVraag;
        private int index;
        private readonly INavigationService navigationService;

        public QuizVragenViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            startTime = DateTime.UtcNow;

            Debug.Assert(Dag > 0);

            quizVraagViewModels.Clear();

            speler = new Speler { Naam = Naam };

            string contents = File.ReadAllText($@".\Files\vragen.{Dag}.json");
            var vragen = JsonConvert.DeserializeObject<VragenData>(contents);

            foreach (var vraag in vragen.Vragen)
            {
                quizVraagViewModels.Add(new QuizVraagViewModel(vraag.Text, vraag.Opties)); 
            }

            QuizVraag = quizVraagViewModels[index];

            NotifyOfPropertyChange(() => CanNext);
            NotifyOfPropertyChange(() => CanStop);
        }

        public QuizVraagViewModel QuizVraag
        {
            get { return quizVraag; }
            set
            {
                Set(ref quizVraag, value);
            }
        }

        public bool CanNext => index < (quizVraagViewModels.Count - 1);
        public bool CanStop => index  == quizVraagViewModels.Count - 1;
        public void Next()
        {
            // noteer antwoord
            speler.Antwoorden.Add(quizVraag.AntwoordToNote);

            index++;
            QuizVraag = quizVraagViewModels[index];
            NotifyOfPropertyChange(() => CanNext);
            NotifyOfPropertyChange(() => CanStop);
        }

        public void Stop()
        {
            var diff = DateTime.UtcNow - startTime;
            speler.Tijd = diff;

            string antwoordenJson = File.ReadAllText($@".\Files\antwoorden.{Dag}.json");
            var antwoorden = JsonConvert.DeserializeObject<AntwoordenData>(antwoordenJson);
            antwoorden.Spelers.Add(speler);

            var data = JsonConvert.SerializeObject(antwoorden, Formatting.Indented);
            File.WriteAllText($@".\Files\antwoorden.{Dag}.json", data);

            navigationService.NavigateToViewModel<QuizIntroViewModel>(new Dictionary<string, object> { { "Dag", Dag } });
        }
    }
}
