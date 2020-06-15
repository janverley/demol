using System.Globalization;
using Caliburn.Micro;
using DeMol.Model;

namespace DeMol.ViewModels
{
    public class QuizViewModel : Screen
    {
        private readonly SimpleContainer container;

        public QuizViewModel(SimpleContainer container)
        {
            this.container = container;
            QuizLoop = new QuizLoopViewModel(container);
        }
        
        public QuizLoopViewModel QuizLoop { get; private set; }
        public DagViewModel SelectedDag { get; set; }

        protected override void OnActivate()
        {
            var adminData = Util.SafeReadJson<AdminData>(SelectedDag.Id);

            foreach (var gespeeldeOpdrachtData in adminData.OpdrachtenGespeeld)
            {
                var opdrachtId = gespeeldeOpdrachtData.OpdrachtId;
                
                var vragenData = Util.SafeReadJson<OpdrachtData>(opdrachtId);

                foreach (var vraag in vragenData.Vragen)
                {
                    
                }
                
            }
            
            base.OnActivate();
        }
    }

    public class QuizLoopViewModel : Conductor<object> 
    {
        private readonly SimpleContainer container;

        public QuizLoopViewModel(SimpleContainer container)
        {
            this.container = container;
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnActivate();
            var x = container.GetInstance<SmoelenViewModel>();
            x.CanSelectUserDelegate = name =>
            {
                return true;
            };
            
            x.DoNext = vm =>
            {
                // var x2 = container.GetInstance<QuizViewModel>();
                // //x2.Naam = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(vm.Naam.ToLower());
                // ActivateItem(x2);
            };

            ActivateItem(x);
        }
    }
}