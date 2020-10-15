using System.Linq;
using Caliburn.Micro;
using DeMol.Model;
using DeMol.Views;

namespace DeMol.ViewModels
{
    public class FinaleQuizViewModel : Conductor<object>.Collection.OneActive
    {
        private readonly SimpleContainer container;
        private readonly SmoelenViewModel smoelenViewModel;

        public FinaleQuizViewModel(SimpleContainer container)
        {
            this.container = container;

            smoelenViewModel = container.GetInstance<SmoelenViewModel>();
            smoelenViewModel.CanSelectUserDelegate = name =>
            {
                var antwoordenData = Util.SafeReadJson<FinaleAntwoordenData>();

                var result = !antwoordenData.Spelers.Any(s => s.Naam.SafeEqual(name));
                return result;
            };

            smoelenViewModel.DoNext = vm => StartFinaleVragen(vm.Naam);
        }

        public override object GetView(object context = null)
        {
            return new QuizView();
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            StartSmoel();
        }

        public void StartSmoel()
        {
            ActivateItem(smoelenViewModel);
        }

        public void StartFinaleVragen(string naam)
        {
            var vragenlijstViewModel = container.GetInstance<FinaleVragenLijstViewModel>();
            vragenlijstViewModel.Naam = naam;
            ActivateItem(vragenlijstViewModel);
        }
    }
}