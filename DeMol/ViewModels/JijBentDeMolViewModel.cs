using Caliburn.Micro;

namespace DeMol.ViewModels
{
    internal class JijBentDeMolViewModel : Screen
    {
        private readonly ShellViewModel conductor;
        private readonly SimpleContainer container;

        public JijBentDeMolViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            Message = $"...";
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
        public void Menu()
        {
            var x = container.GetInstance<MenuViewModel>();
            conductor.ActivateItem(x);
        }

    }
}