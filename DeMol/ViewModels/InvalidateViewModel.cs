using System.IO;
using Caliburn.Micro;
using DeMol.Model;

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
            var admin = Util.GetAdminDataOfSelectedDag(container);
            foreach (var gespeeldeOpdrachtData in admin.OpdrachtenGespeeld)
            {
                File.Delete($@".\Files\antwoorden.{gespeeldeOpdrachtData.OpdrachtId}.json");
            }
           
        }
    }
}