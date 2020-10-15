using System;
using System.Windows.Input;
using System.Windows.Threading;
using Caliburn.Micro;

namespace DeMol.ViewModels
{
    public class TimerViewModel : Screen
    {
        private readonly ShellViewModel conductor;
        private readonly SimpleContainer container;
        private readonly DispatcherTimer timer = new DispatcherTimer();
        private TimeSpan left = TimeSpan.Zero;

        public TimerViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;

            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(1);
        }

        public string Tijd => left.ToString("mm\\:ss");

        public int Minuten { get; set; }

        public bool CanStop => timer.IsEnabled;
        public bool CanStart => !timer.IsEnabled;

        public bool MinutenIsEnabled => !timer.IsEnabled;

        private void Timer_Tick(object sender, EventArgs e)
        {
            left = left - timer.Interval;

            if (left <= TimeSpan.Zero)
            {
                left = TimeSpan.Zero;
                container.GetInstance<ShellViewModel>().BgSource = @"./bg.2020.red.jpg";
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

        public void Start()
        {
            timer.Start();
            UpdateControlStates();
        }

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