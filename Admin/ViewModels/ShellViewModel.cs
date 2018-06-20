using Caliburn.Micro;
using System.Windows.Controls;

namespace Admin.ViewModels
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

            ActivateItem(container.GetInstance<MenuViewModel>());
        }
    }
}
