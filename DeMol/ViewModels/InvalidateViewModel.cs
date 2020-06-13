using System.IO;
using Caliburn.Micro;

namespace DeMol.ViewModels
{
    public class InvalidateViewModel : Screen
    {
        private readonly ShellViewModel conductor;
        private readonly SimpleContainer container;

        public InvalidateViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;
        }

        public void Menu()
        {
            var x = container.GetInstance<MenuViewModel>();
            conductor.ActivateItem(x);
        }

        public void Invalidate()
        {
            File.Delete($@".\Files\antwoorden.{container.GetInstance<ShellViewModel>().Dag}.json");
        }
    }
}