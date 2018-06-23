using DeMol.Model;
using Caliburn.Micro;
using System;
using System.Linq;

namespace DeMol.ViewModels
{

    public class MenuViewModel : Screen
    {
        public int AantalSpelers => Spelerdata.Spelers.Count;
        public int AantalSpelersDieDeMolMoetenGeradenHebben => 2;


        public MenuViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;
        }

        private Status _op1;
        private Status _op2;
        private Status _op3;

        protected override void OnActivate()
        {
            base.OnActivate();

            DagenData = Util.SafeReadJson<DagenData>($@".\Files\dagen.json");
            Spelerdata = Util.SafeReadJson<SpelersData>($@".\Files\spelers.json");

            foreach (var dag in DagenData.Dagen)
            {
                Dagen.Add(new DagViewModel(dag.Id, dag.Naam));
            }

            // if set, preselect SelectedDag
            if (Dag > 0 && Dagen.Any(d => d.Id == Dag))
            {
                SelectedDag = Dagen.First(d => d.Id == Dag);
            }
        }

        protected override void OnDeactivate(bool close)
        {
            if (SelectedDag != null)
            {
                SaveAdmin();
            }
            base.OnDeactivate(close);
        }

        public BindableCollection<DagViewModel> Dagen => new BindableCollection<DagViewModel>();

        private DagViewModel selectedDag;

        public DagViewModel SelectedDag
        {
            get { return selectedDag; }
            set
            {
                if (Set(ref selectedDag, value))
                {
                    UpdateButtonStates();
                    Dag = SelectedDag.Id;
                }
            }
        }

        private void UpdateButtonStates()
        {
            NotifyOfPropertyChange(() => CanSaveAdmin);
            NotifyOfPropertyChange(() => CanStartQuiz);
            NotifyOfPropertyChange(() => CanValidate);
        }

        public void SelectedDagChanged()
        {
            var data = Util.SafeReadJson<AdminData>($@".\Files\admin.{SelectedDag.Id}.json");

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

        public BindableCollection<PasVraagViewModel> Pasvragen => new BindableCollection<PasVraagViewModel>();

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

        public string LockString
        {
            get { return lockString; }
            set
            {
                if (Set(ref lockString, value))
                {
                    UpdateButtonStates();
                }
            }
        }

        private string lockString = "";

        private bool UnLocked => LockString.Equals("asd", StringComparison.InvariantCultureIgnoreCase);

        public bool CanSaveAdmin => SelectedDag != null && UnLocked;

        public void Timer()
        {
            var x = container.GetInstance<TimerViewModel>();
            x.Minuten = 45;
            conductor.ActivateItem(x);
        }

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

            Util.SafeFileWithBackup($@".\Files\admin.{SelectedDag.Id}.json", newData);
        }

        public bool CanStartQuiz => SelectedDag != null && UnLocked;


        public void StartQuiz()
        {
            var x = container.GetInstance<QuizNaamViewModel>();
            conductor.ActivateItem(x);
        }
        public bool CanValidate => SelectedDag != null && UnLocked;

        public DagenData DagenData { get; private set; }
        public SpelersData Spelerdata { get; private set; }
        public int Dag { get; internal set; }

        public void Validate()
        {
            var x = container.GetInstance<ValidateViewModel>();
            x.Dag = SelectedDag.Id;
            conductor.ActivateItem(x);
        }

    }
}
