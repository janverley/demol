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
                var adminData = Util.GetAdminDataOfSelectedDag(container);

                var result = !adminData.HeeftQuizGespeeld.Any(s => s.Naam.SafeEqual(name));
                return result;
            };

            smoelenViewModel.DoNext = vm => StartScore(vm.Naam);
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

        public void StartScore(string naam)
        {
         
            var x = container.GetInstance<ScoreViewModel>();
            x.Naam = naam;
            
            ActivateItem(x);
        }

        
        public void StartQuiz(string naam)
        {
            var vragenlijstViewModel = container.GetInstance<VragenLijstViewModel>();
            vragenlijstViewModel.Naam = naam;
            ActivateItem(vragenlijstViewModel);
        }
    }
}