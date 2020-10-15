using Caliburn.Micro;

namespace DeMol.ViewModels
{
    public class BoodschapViewModel : Screen
    {
        private string text;
        private ShellViewModel conductor;
        private SimpleContainer container;

        public BoodschapViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;
        }

        public string Text
        {
            get => text;
            set => Set(ref text, value);
        }

        public void Menu()
        {
            var x = container.GetInstance<MenuViewModel>();
            conductor.ActivateItem(x);
        }

    }
}