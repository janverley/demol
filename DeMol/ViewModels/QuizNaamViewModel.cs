using Caliburn.Micro;
using DeMol.Model;
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
            if (!container.GetInstance<MenuViewModel>().Spelerdata.Spelers.Any(s => s.Naam.SafeEqual(Naam)))
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
        protected override void OnActivate()
        {
            base.OnActivate();

        }

        public bool CanStart => !string.IsNullOrEmpty(Naam) && string.IsNullOrEmpty(Message);
        public void Start()
        {
            var x = container.GetInstance<QuizBenJijDeMolViewModel>();
            x.Naam = Naam;
            conductor.ActivateItem(x);

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

        public void Menu()
        {
            var x = container.GetInstance<MenuViewModel>();
            conductor.ActivateItem(x);
        }
    }
}
