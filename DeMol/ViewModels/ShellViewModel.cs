﻿using Caliburn.Micro;
using DeMol.Model;
using DeMol.Properties;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace DeMol.ViewModels
{
    public class ShellViewModel : Conductor<object>.Collection.OneActive
    {
        private readonly SimpleContainer container;

        public ShellViewModel(SimpleContainer container)
        {
            this.container = container;
        }

        private int dag;

        public int Dag
        {
            get
            {
                return dag;
            }
            set
            {
                Set(ref dag, value);

                ShellTitle = $"De Mol - {DagenData.Dagen.First(d => d.Id == dag)?.Naam??""}";
            }
        }
        private string shellTitle;

        public string ShellTitle
        {
            get
            {
                return shellTitle;
            }
            set
            {
                Set(ref shellTitle, value);
            }
        }

        public int AantalSpelers => Spelerdata.Spelers.Count;
        public int AantalSpelersDieDeMolMoetenGeradenHebben => Settings.Default.AantalSpelersDieDeMolMoetenGeradenHebben;
        public DagenData DagenData { get; private set; }
        public SpelersData Spelerdata { get; private set; }

        private string bgSource;

        public string BgSource
        {
            get { return bgSource; }
            set { Set(ref bgSource, value); }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            BgSource = @"./bg.2019.jpg";

            DagenData = Util.SafeReadJson<DagenData>();

            Spelerdata = Util.SafeReadJson<SpelersData>();


            var menuDay = -1;
            var timerminuten = -1;
            var showResult = -1;
            var showQuiz = -1;
            var showEndResult = false;

            var p = new OptionSet()
            {
                { "m|menu=", "Start the Menu screen for day.", (int v) => menuDay = v },
                { "t|timer=", "Start the timer screen with # minutes.", (int v) => timerminuten = v },
                { "r|result=",  "Start the ResultScreen for day", (int v) => showResult = v},
                { "q|quiz=",  "Start the Quiz for day", (int v) => showQuiz = v},
                { "e|endresult",  "Start the EndResult", v => showEndResult = (v!=null) },
            };

            
        List<string> extra;
            try
            {
                extra = p.Parse(Environment.GetCommandLineArgs());
            }
            catch (OptionException ex)
            {
                Console.Write("DeMol: ");
                Console.WriteLine(ex.Message);
                return;
            }

            if (timerminuten > 0)
            {
                var x = container.GetInstance<TimerViewModel>();
                x.Minuten = timerminuten;
                ActivateItem(x);
            }
            else if (showResult > 0)
            {
                var x = container.GetInstance<ResultViewModel>();
                Dag = showResult;
                ActivateItem(x);
            }
            else if (showQuiz > 0)
            {
                var x = container.GetInstance<QuizNaamViewModel>();
                Dag = showQuiz;
                ActivateItem(x);
            }
            else if (showEndResult)
            {
                var x = container.GetInstance<EndResultViewModel>();
                ActivateItem(x);
            }
            else
            {
                var x = container.GetInstance<MenuViewModel>();
                ActivateItem(x);
            }
        }
    }
}
