using Caliburn.Micro;
using DeMol.Model;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

namespace DeMol.ViewModels
{
    public class QuizNaamViewModel : Screen
    {
        public QuizNaamViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;
        }

        private string naam;
        private string message;
        private readonly ShellViewModel conductor;
        private readonly SimpleContainer container;

        public string Naam
        {
            get { return naam; }
            set
            {
                if (Set(ref naam, value))
                {
                    Validate();
                }
            }
        }

        private void Validate()
        {
            if (!string.IsNullOrEmpty(Naam) && !container.GetInstance<ShellViewModel>().Spelerdata.Spelers.Any(s => s.Naam.SafeEqual(Naam)))
            {
                Message = $"'{Naam}' ken ik niet.";
            }
            else
            {
                Message = "";
            }
        }

        public string Message
        {
            get { return message; }
            set
            {
                if (Set(ref message, value))
                {
                    NotifyOfPropertyChange(() => CanStart);
                }
            }
        }

        public bool VragenGevonden => Util.DataFileFoundAndValid<VragenData>(container.GetInstance<ShellViewModel>().Dag);

        public bool CanStart => !string.IsNullOrEmpty(Naam) && string.IsNullOrEmpty(Message) && VragenGevonden;

        public Action<QuizNaamViewModel> DoNext { get; set; } = vm => { };

        public void Start()
        {
            DoNext(this);

        }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (e?.Key == Key.Enter && CanStart)
            {
                Start();
            }
            if (e?.Key == Key.Escape )
            {
                Menu();
            }
        }
        protected override void OnActivate()
        {
            base.OnActivate();
            Naam = "";

            if (!VragenGevonden)
            {
                Message = $"Geen Vragen gevonden!";
                NotifyOfPropertyChange(() => VragenGevonden);
            }
        }

        public void Menu()
        {
            var x = container.GetInstance<MenuViewModel>();
            conductor.ActivateItem(x);
        }
    }
}
