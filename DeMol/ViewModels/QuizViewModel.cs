using System.Linq;
using Caliburn.Micro;
using DeMol.Model;

namespace DeMol.ViewModels
{
    public class QuizViewModel : Conductor<object>.Collection.OneActive
    {
        private readonly SimpleContainer container;
        private readonly SmoelenViewModel smoelenViewModel;

        public QuizViewModel(SimpleContainer container)
        {
            this.container = container;

            smoelenViewModel = container.GetInstance<SmoelenViewModel>();
            smoelenViewModel.CanSelectUserDelegate = name =>
            {
                var adminData = Util.GetAdminData(container);

                var result = !adminData.HeeftQuizGespeeld.Any(s => s.Naam.SafeEqual(name));
                return result;
            };

            smoelenViewModel.DoNext = vm => StartQuiz(vm.Naam);
        }

        public DagViewModel SelectedDag { get; set; }

        protected override void OnActivate()
        {
            base.OnActivate();
            StartSmoel();
        }

        public void StartSmoel()
        {
            ActivateItem(smoelenViewModel);
        }

        public void StartQuiz(string naam)
        {
            var vragenlijstViewModel = container.GetInstance<VragenLijstViewModel>();
            vragenlijstViewModel.Naam = naam;
            ActivateItem(vragenlijstViewModel);
        }
    }
}