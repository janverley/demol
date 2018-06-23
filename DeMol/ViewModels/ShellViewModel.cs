using Caliburn.Micro;
using NDesk.Options;
using System;
using System.Collections.Generic;
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

        protected override void OnInitialize()
        {
            base.OnInitialize();

            var menuDay = -1;
            var timerminuten = -1;
            var showResult = -1;

            var p = new OptionSet()
            {
                { "m|menu=", "Start the Menu screen for day.", (int v) => menuDay = v },
                { "t|timer=", "Start the timer screen with # minutes.", (int v) => timerminuten = v },
                { "r|result=",  "Start the ResultScreen for day", (int v) => showResult = v},
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
                x.Dag = showResult;
                ActivateItem(x);
            }
            else
            {
                var x = container.GetInstance<MenuViewModel>();

                if (menuDay > 0)
                {
                    x.Dag = menuDay;
                }
                ActivateItem(x);
            }
        }
    }
}
