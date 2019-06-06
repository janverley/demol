﻿using Caliburn.Micro;
using DeMol.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Threading;

namespace DeMol.ViewModels
{
    internal class JijBentDeMolViewModel : Screen
    {
        private readonly ShellViewModel conductor;
        private readonly SimpleContainer container;
        private readonly DispatcherTimer timer = new DispatcherTimer();

        public JijBentDeMolViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;

            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(10);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            Next();
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            var mollen = new List<int> { 7, 8, 1, 0, 4, 3, 2, 6, 5 };

            var did = (Dag.Id-1)%mollen.Count;

            var mol = mollen[did];
                
                var demol = container.GetInstance<ShellViewModel>().Spelerdata.Spelers[mol];

            var vandaagMorgen = IsMorgen ? "morgen" : "vandaag";

            var ja = $"{Naam}, {vandaagMorgen} ben jij de mol.\n\nProbeer zo veel mogelijk opdrachten te laten mislukken! \nVeel succes!";
            var nee = $"{Naam}, {vandaagMorgen} ben jij niet de mol.\n\nProbeer de mol te ontmaskeren en onthoud zo veel mogelijk van wat er gebeurd.\nVeel succes!";

            var m = Util.SafeEqual(Naam, demol.Naam) ? ja : nee;

            Message = $"Opgelet, deze boodschap verdwijnt na 10 seconden.\n\n{m}\n\n Roep de volgende speler.";

            timer.Start();
        }

        protected override void OnDeactivate(bool close)
        {
            timer.Stop();
            base.OnDeactivate(close);
        }

        public string Message
        {
            get { return message; }
            set { Set(ref message, value); }
        }

        private string naam;
        private string message;

        public string Naam
        {
            get { return naam; }
            set { Set(ref naam, value ); }
        }

        public DagViewModel Dag { get; internal set; }

        public Action<JijBentDeMolViewModel> DoNext { get; set; }
        public bool IsMorgen { get; internal set; }

        public void Next()
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