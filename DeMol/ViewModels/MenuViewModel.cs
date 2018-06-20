using DeMol.Model;
using Caliburn.Micro;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMol.ViewModels
{

    public class MenuViewModel : Screen
    {

        public MenuViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;
            Pasvragen = new BindableCollection<PasVraagViewModel>();
        }

        private Status _op1;
        private Status _op2;
        private Status _op3;

        public BindableCollection<DagViewModel> Dagen
        {
            get
            {
                return new BindableCollection<DagViewModel>
                {
                    new DagViewModel(1,"zaterdag 30 juni"),
                    new DagViewModel(2,"zondag 1 juli"),
                    new DagViewModel(3,"maandag 2 juli"),
                    new DagViewModel(4,"dinsdag 3 juli"),
                    new DagViewModel(5,"woensdag 4 juli"),
                    new DagViewModel(5,"donderdag 5 juli"),
                    new DagViewModel(5,"vrijdag 6 juli"),
                };
            }
        }

        private DagViewModel selectedDag;

        public DagViewModel SelectedDag
        {
            get { return selectedDag; }
            set {
                if (Set(ref selectedDag, value))
                {
                    NotifyOfPropertyChange(() => CanSaveAdmin);
                    NotifyOfPropertyChange(() => CanShowResult);
                    NotifyOfPropertyChange(() => CanStartQuiz);
                    NotifyOfPropertyChange(() => CanValidate);
                }
            }
        }

        public void SelectedDagChanged()
        {
            string contents = File.ReadAllText($@".\Files\admin.{SelectedDag.Id}.json");
            var data = JsonConvert.DeserializeObject<AdminData>(contents);


            foreach (var item in data.Pasvragen)
            {
                Pasvragen.Add(new PasVraagViewModel { Naam = item.Naam, PasVragenVerdiend = item.PasVragenVerdiend });
            }

            op1 = data.Opdrachten.op1;
            op2 = data.Opdrachten.op2;
            op3 = data.Opdrachten.op3;
        }

        private IConductor conductor;
        private readonly SimpleContainer container;

        public BindableCollection<PasVraagViewModel> Pasvragen { get; private set; }

        public Status op1
        {
            get { return _op1; }
            set { Set(ref _op1, value); }
        }

        public Status op2
        {
            get { return _op2; }
            set { Set(ref _op2, value); }
        }

        public Status op3
        {
            get { return _op3; }
            set { Set(ref _op3, value); }
        }

        public bool CanSaveAdmin => SelectedDag != null;

        public void SaveAdmin()
        {
            var newData = new AdminData();
            newData.Opdrachten.op1 = op1;
            newData.Opdrachten.op2 = op2;
            newData.Opdrachten.op3 = op3;


            foreach (var item in Pasvragen)
            {
                newData.Pasvragen.Add(new PasvragenVerdiend { Naam = item.Naam, PasVragenVerdiend = item.PasVragenVerdiend });
            }

            var data = JsonConvert.SerializeObject(newData, Formatting.Indented);

            File.WriteAllText($@".\Files\admin.{SelectedDag.Id}.json", data);
        }

        public bool CanStartQuiz => SelectedDag != null;


        public void StartQuiz()
        {
            var x = container.GetInstance<QuizIntroViewModel>();
            conductor.ActivateItem(x);
        }
        public bool CanValidate => SelectedDag != null;

        public void Validate()
        {
            var x = container.GetInstance<ValidateViewModel>();
            //x.Dag = SelectedDag.Id;
            conductor.ActivateItem(x);
        }
        public bool CanInvalidateAnswers => SelectedDag != null;

        public void InvalidateAnswers()
        {
            var x = container.GetInstance<InvalidateViewModel>();
            //x.Dag = SelectedDag.Id;
            conductor.ActivateItem(x);
        }

        public bool CanShowResult => SelectedDag != null;

        public void ShowResult()
        {
            var x = container.GetInstance<ResultViewModel>();
            //x.Dag = SelectedDag.Id;
            conductor.ActivateItem(x);
        }
    }
}
