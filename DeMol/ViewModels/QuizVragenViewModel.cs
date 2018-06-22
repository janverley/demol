using Caliburn.Micro;
using DeMol.Model;
using System;
using System.Collections.Generic;
using System.Linq;
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

            speler = new Speler { Naam = Naam.ToLower() };

            var vragen = Util.SafeReadJson<VragenData>($@".\Files\vragen.{container.GetInstance<MenuViewModel>().SelectedDag.Id}.json");

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

            speler.Antwoorden.Add(quizVraag.AntwoordToNote);

            speler.IsDeMol = IsDeMol;

            speler.Tijd = diff;

            var antwoorden = Util.SafeReadJson<AntwoordenData>($@".\Files\antwoorden.{container.GetInstance<MenuViewModel>().SelectedDag.Id}.json");

            antwoorden.Spelers.Add(speler);

            Util.SafeFileWithBackup($@".\Files\antwoorden.{container.GetInstance<MenuViewModel>().SelectedDag.Id}.json", antwoorden);

            var x = container.GetInstance<QuizOuttroViewModel>();
            
            conductor.ActivateItem(x);

        }
    }
}
