using DeMol.Model;
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

namespace DeMol.ViewModels
{
    public class QuizVragenViewModel : Screen
    {
        private string naam;

        public string Naam
        {
            get { return naam; }
            set { Set(ref naam, value); }
        }
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

        private DateTime startTime;
        private List<QuizVraagViewModel> quizVraagViewModels = new List<QuizVraagViewModel>();
        private Speler speler;
        private QuizVraagViewModel quizVraag;
        private int index;
        private IConductor conductor;
        private readonly SimpleContainer container;
        private bool isDeMol;

        public QuizVragenViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            startTime = DateTime.UtcNow;

            quizVraagViewModels.Clear();

            speler = new Speler { Naam = Naam };

            string contents = File.ReadAllText($@".\Files\vragen.{container.GetInstance<MenuViewModel>().SelectedDag.Id}.json");
            var vragen = JsonConvert.DeserializeObject<VragenData>(contents);

            foreach (var vraag in vragen.Vragen)
            {
                quizVraagViewModels.Add(new QuizVraagViewModel(vraag.Text, vraag.Opties??Enumerable.Empty<string>())); 
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

        public void OnKeyDown(KeyEventArgs e)
        {
            if (e?.Key == Key.Enter)
            {
                if (CanNext)
                {
                    Next();
                }
                else if (CanStop)
                {
                    Stop();
                }
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

            // noteer antwoord
            speler.Antwoorden.Add(quizVraag.AntwoordToNote);

            speler.IsDeMol = IsDeMol;

            speler.Tijd = diff;

            AntwoordenData antwoorden = new AntwoordenData { Dag = container.GetInstance<MenuViewModel>().SelectedDag.Id.ToString()};
            if (File.Exists($@".\Files\antwoorden.{container.GetInstance<MenuViewModel>().SelectedDag.Id}.json"))
            {
                string antwoordenJson = File.ReadAllText($@".\Files\antwoorden.{container.GetInstance<MenuViewModel>().SelectedDag.Id}.json");
                antwoorden = JsonConvert.DeserializeObject<AntwoordenData>(antwoordenJson);
            }

            antwoorden.Spelers.Add(speler);
            var data = JsonConvert.SerializeObject(antwoorden, Formatting.Indented);
            File.WriteAllText($@".\Files\antwoorden.{container.GetInstance<MenuViewModel>().SelectedDag.Id}.json", data);

            var x = container.GetInstance<QuizOuttroViewModel>();
            
            conductor.ActivateItem(x);

        }
    }
}
