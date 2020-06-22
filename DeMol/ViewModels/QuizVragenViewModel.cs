using System;
using System.Collections.Generic;
using System.Windows.Input;
using Caliburn.Micro;
using DeMol.Model;

namespace DeMol.ViewModels
{
    public class QuizVragenViewModel : Screen
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

        public QuizVragenViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;

            QuizVraagViewModelFactory = vraagCode =>
            {
                var vraag = Util.GetVraagFromCode(vraagCode);
                return new QuizVraagViewModel(vraag, vraagCode);
            };
            //
            // DoNext = quizVragenViewModel =>
            // {
            //     var x = container.GetInstance<QuizOuttroViewModel>();
            //     x.Naam = quizVragenViewModel.Naam;
            //     conductor.ActivateItem(x);
            // };
        }

        public string Naam
        {
            get => naam;
            set => Set(ref naam, value);
        }

        public bool IsDeMol
        {
            get => isDeMol;
            set
            {
                if (Set(ref isDeMol, value))
                {
                }
            }
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

        public Action<QuizVragenViewModel> DoNext { get; set; }

        protected override void OnActivate()
        {
            base.OnActivate();
            startTime = DateTime.UtcNow;

            if (IsDeMol)
            {
                Message = "Jij bent De Mol, dus je moet op alle vragen goed antwoorden!";
            }
            else
            {
                Message = "";
            }


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

            var alleAntwoorden = Util.SafeReadJson<AntwoordenData>(OpdrachtId);

            var speler = new Speler
            {
                Naam       = Naam,
                DeMolIs    = DeMolIs,
                IsDeMol    = IsDeMol,
                Tijd       = diff,
                Antwoorden = antwoorden
            };

            alleAntwoorden.Spelers.Add(speler);

            Util.SafeFileWithBackup(alleAntwoorden, OpdrachtId);

            DoNext(this);
        }
    }
}