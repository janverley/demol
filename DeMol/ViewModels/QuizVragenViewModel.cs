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
        private Dictionary<string, string> antwoorden = new Dictionary<string, string>();

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

        public List<string> VragenCodes { get; set; }

        public string DagId { get; set; }


        private DateTime startTime;
        private readonly List<QuizVraagViewModel> quizVraagViewModels = new List<QuizVraagViewModel>();
        private QuizVraagViewModel quizVraag;
        private int index;
        private readonly IConductor conductor;
        private readonly SimpleContainer container;
        private bool isDeMol;

        public QuizVragenViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;

            QuizVraagViewModelFactory = (vraagCode) =>
            {
                var vraag = Util.GetVraagFromCode(vraagCode);
                return new QuizVraagViewModel(vraag, vraagCode);
            };

            DoNext = (QuizVragenViewModel quizVragenViewModel) =>
            {
                var x = container.GetInstance<QuizOuttroViewModel>();
                x.Naam = quizVragenViewModel.Naam;
                conductor.ActivateItem(x);
            };

        }

        private string message;

        public string Message
        {
            get
            {
                return message;
            }
            set
            {
                Set(ref message, value);
            }
        }

        public Func<string, QuizVraagViewModel> QuizVraagViewModelFactory { get; set; }

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

        public string DeMolIs { get; set; }

        public void Next()
        {
            NoteerAntwoord();

            index++;
            QuizVraag = quizVraagViewModels[index];
            NotifyOfPropertyChange(() => CanNext);
            NotifyOfPropertyChange(() => CanStop);
        }

        private void NoteerAntwoord()
        {
            if (QuizVraag != null)
            {
                // noteer antwoord
                antwoorden.Add(QuizVraag.VraagID, QuizVraag.AntwoordToNote);
            }
        }

        public void Stop()
        {
            var diff = DateTime.UtcNow - startTime;

            NoteerAntwoord();

            var alleAntwoorden = Util.SafeReadJson<AntwoordenData>(DagId);

            var speler = new Speler
            {
                Naam = Naam,
                DeMolIs = DeMolIs,
                IsDeMol = IsDeMol,
                Tijd = diff,
                Antwoorden = antwoorden
            };

            alleAntwoorden.Spelers.Add(speler);

            Util.SafeFileWithBackup(alleAntwoorden, DagId);

            DoNext(this);
        }

        public Action<QuizVragenViewModel> DoNext{ get; set; }
    }
}
