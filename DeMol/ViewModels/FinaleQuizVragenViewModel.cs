using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Caliburn.Micro;
using DeMol.Model;

namespace DeMol.ViewModels
{
    public class FinaleQuizVragenViewModel : Screen
    {
        private readonly Dictionary<string, string> antwoorden = new Dictionary<string, string>();
        private readonly IConductor conductor;
        private readonly SimpleContainer container;
        private readonly List<QuizVraagViewModel> quizVraagViewModels = new List<QuizVraagViewModel>();
        private int index;
        private bool isDeMol;

        private string message;
        private string naam;
        private QuizVraagViewModel quizVraag;


        private DateTime startTime;

        public FinaleQuizVragenViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;

            QuizVraagViewModelFactory = vraagCode =>
            {
                var vraag = Util.GetVraagFromCode(vraagCode);
                return new QuizVraagViewModel(vraag, vraagCode);
            };
        }

        public string Naam
        {
            get => naam;
            set => Set(ref naam, value);
        }

        public List<string> VragenCodes { get; set; }

        public string OpdrachtId { get; set; }

        public string Message
        {
            get => message;
            set => Set(ref message, value);
        }

        public Func<string, QuizVraagViewModel> QuizVraagViewModelFactory { get; set; }

        public QuizVraagViewModel QuizVraag
        {
            get => quizVraag;
            set => Set(ref quizVraag, value);
        }


        public bool CanPrevious => index > 0;
        public bool CanNext => index < quizVraagViewModels.Count - 1;
        public bool CanStop => index == quizVraagViewModels.Count - 1;

        public string DeMolIs { get; set; }

        public Action<FinaleQuizVragenViewModel> DoNext { get; set; }

        protected override void OnActivate()
        {
            base.OnActivate();
            startTime = DateTime.UtcNow;

            quizVraagViewModels.Clear();
            foreach (var vraagCode in VragenCodes)
            {
                var vm = QuizVraagViewModelFactory(vraagCode);
                quizVraagViewModels.Add(vm);
            }

            QuizVraag = quizVraagViewModels[index];

            NotifyOfPropertyChange(() => CanNext);
            NotifyOfPropertyChange(() => CanPrevious);
            NotifyOfPropertyChange(() => CanStop);
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

        public void Next()
        {
            NoteerAntwoord();

            index++;
            QuizVraag = quizVraagViewModels[index];
            NotifyOfPropertyChange(() => CanNext);
            NotifyOfPropertyChange(() => CanPrevious);
            NotifyOfPropertyChange(() => CanStop);
        }

        public void Previous()
        {
            // NoteerAntwoord();

            index--;
            QuizVraag = quizVraagViewModels[index];
            NotifyOfPropertyChange(() => CanNext);
            NotifyOfPropertyChange(() => CanPrevious);
            NotifyOfPropertyChange(() => CanStop);
        }

        private void NoteerAntwoord()
        {
            if (QuizVraag != null)
            {
                // noteer antwoord
                if (antwoorden.ContainsKey(QuizVraag.VraagID))
                {
                    antwoorden.Remove(QuizVraag.VraagID);
                }

                antwoorden.Add(QuizVraag.VraagID, QuizVraag.AntwoordToNote);
            }
        }

        public void Stop()
        {
            var diff = DateTime.UtcNow - startTime;

            NoteerAntwoord();

            var alleAntwoorden = Util.SafeReadJson<FinaleAntwoordenData>();


            if (alleAntwoorden.Spelers.Any(s => s.Naam.SafeEqual(Naam)))
            {
                // tijd optellen en antwoorden toeveogen en mol naam toevoegen

                var bestaandeSpeler = alleAntwoorden.Spelers.Single(s => s.Naam.SafeEqual(Naam));

                bestaandeSpeler.Tijd += diff;

                foreach (var antwoord in antwoorden)
                {
                    bestaandeSpeler.AntwoordenPerVraagId.Add(antwoord.Key, antwoord.Value);
                }

                bestaandeSpeler.DeMolIsPerOpdrachtId.Add(OpdrachtId, DeMolIs);
            }
            else
            {
                var speler = new FinaleSpeler
                {
                    Naam = Naam,
                    Tijd = diff,
                    AntwoordenPerVraagId = antwoorden
                };

                speler.DeMolIsPerOpdrachtId.Add(OpdrachtId, DeMolIs);

                alleAntwoorden.Spelers.Add(speler);
            }

            Util.SafeFileWithBackup(alleAntwoorden, OpdrachtId);

            DoNext(this);
        }
    }
}