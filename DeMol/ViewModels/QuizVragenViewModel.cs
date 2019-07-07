﻿using Caliburn.Micro;
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
        private readonly List<QuizVraagViewModel> quizVraagViewModels = new List<QuizVraagViewModel>();
        private Speler speler;
        private QuizVraagViewModel quizVraag;
        private int index;
        private readonly IConductor conductor;
        private readonly SimpleContainer container;
        private bool isDeMol;

        public QuizVragenViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;
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

            speler = new Speler { Naam = Naam };

            var admin = Util.SafeReadJson<AdminData>(container.GetInstance<ShellViewModel>().Dag);
            var opdrachtenVanVandaag = admin.OpdrachtenGespeeld;

            if (!opdrachtenVanVandaag.Any())
            {
                Message = $"Er is iets mis: Er lijken geen opdrachten gespeeld vandaag. Bel me, schrijf me :)";
                index = -1;
                return;
            }

            var randomVraagCodes = admin.VragenCodes.Shuffle(new Random());

            quizVraagViewModels.Clear();
            foreach (var vraagCode in randomVraagCodes)
            {
                var vraag = Util.GetVraagFromCode(vraagCode);
                quizVraagViewModels.Add(new QuizVraagViewModel(vraag, vraagCode)); 
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
                speler.Antwoorden.Add(QuizVraag.VraagID, QuizVraag.AntwoordToNote);
            }
        }

        public void Stop()
        {
            var diff = DateTime.UtcNow - startTime;

            NoteerAntwoord();

            speler.DeMolIs = DeMolIs;
            speler.IsDeMol = IsDeMol;

            speler.Tijd = diff;

            var antwoorden = Util.SafeReadJson<AntwoordenData>(container.GetInstance<ShellViewModel>().Dag);

            antwoorden.Spelers.Add(speler);

            Util.SafeFileWithBackup(antwoorden, container.GetInstance<ShellViewModel>().Dag);

            var x = container.GetInstance<QuizOuttroViewModel>();
            x.Naam = Naam;
            conductor.ActivateItem(x);

        }
    }
}
