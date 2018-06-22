using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeMol.ViewModels
{
    public class QuizOuttroViewModel : Screen
    {
        private readonly ShellViewModel conductor;
        private readonly SimpleContainer container;

        public QuizOuttroViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;
        }

        public void Next()
        {
            var x = container.GetInstance<QuizIntroViewModel>();
            conductor.ActivateItem(x);
        }
        public void OnKeyDown(KeyEventArgs e)
        {
            if (e?.Key == Key.Enter)
            {
                Next();
            }
        }
    }
}
