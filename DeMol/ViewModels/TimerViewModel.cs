using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace DeMol.ViewModels
{
    public class TimerViewModel : Screen
    {
        private readonly ShellViewModel conductor;
        private readonly SimpleContainer container;
        private readonly DispatcherTimer timer = new DispatcherTimer();

        public TimerViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;

            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(1);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            left = left - timer.Interval;

            if (left <= TimeSpan.Zero)
            {
                left = TimeSpan.Zero;
                container.GetInstance<ShellViewModel>().BgSource = @"./bg.2019.red.jpg";
                Stop();
            }

            NotifyOfPropertyChange(() => Tijd);
        }

        protected override void OnActivate()
        {
            Reset();
            UpdateControlStates();
            base.OnActivate();
        }

        protected override void OnDeactivate(bool close)
        {
            timer.Stop();
            UpdateControlStates();
            base.OnDeactivate(close);

        }
        private TimeSpan left = TimeSpan.Zero;

        public string Tijd
        {
            get { return left.ToString("mm\\:ss"); }
        }

        public void Start()
        {
            timer.Start();
            UpdateControlStates();
        }

        public int Minuten { get; set; }

        public void Stop()
        {
            timer.Stop();
            UpdateControlStates();
        }

        private void UpdateControlStates()
        {
            NotifyOfPropertyChange(() => MinutenIsEnabled);
            NotifyOfPropertyChange(() => CanStop);
            NotifyOfPropertyChange(() => CanStart);
        }

        public bool CanStop => timer.IsEnabled;
        public bool CanStart => !timer.IsEnabled;

        public bool MinutenIsEnabled => !timer.IsEnabled;

        public void Reset()
        {
            left = TimeSpan.FromMinutes(Minuten);
            NotifyOfPropertyChange(() => Tijd);
        }

        public void Menu()
        {
            var x = container.GetInstance<MenuViewModel>();
            conductor.ActivateItem(x);
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (e?.Key == Key.Space && CanStart)
            {
                Start();
            }
            if (e?.Key == Key.Space && CanStop)
            {
                Stop();
            }
            if (e?.Key == Key.Escape)
            {
                Menu();
            }
        }
    }
}
