using DeMol.Model;
using Caliburn.Micro;
using System;
using System.Linq;
using DeMol.Properties;

namespace DeMol.ViewModels
{

    public class MenuViewModel : Screen
    {

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

            foreach (var dag in container.GetInstance<ShellViewModel>().DagenData.Dagen)
            {
                Dagen.Add(new DagViewModel(dag.Id, dag.Naam));
            }

            // if set, preselect SelectedDag
            if (container.GetInstance<ShellViewModel>().Dag > 0 && Dagen.Any(d => d.Id == container.GetInstance<ShellViewModel>().Dag))
            {
                SelectedDag = Dagen.First(d => d.Id == container.GetInstance<ShellViewModel>().Dag);
                SelectedDagChanged();
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


        private DagViewModel selectedDag;

        public DagViewModel SelectedDag
        {
            get { return selectedDag; }
            set
            {
                if (Set(ref selectedDag, value))
                {
                    container.GetInstance<ShellViewModel>().Dag = SelectedDag.Id;
                    UpdateButtonStates();
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
            container.GetInstance<ShellViewModel>().Dag = SelectedDag.Id;

            var data = Util.SafeReadJson<AdminData>(container.GetInstance<ShellViewModel>().Dag);

            // als file niet gevonden
            foreach (var speler in container.GetInstance<ShellViewModel>().Spelerdata.Spelers)
            {
                if (!data.Pasvragen.Any(pv => pv.Naam.SafeEqual(speler.Naam)))
                {
                    data.Pasvragen.Add(new PasvragenVerdiend { Naam = speler.Naam, PasVragenVerdiend = 0 });
                }
            }

            Pasvragen.Clear();
            foreach (var item in data.Pasvragen)
            {
                Pasvragen.Add(new PasVraagViewModel { Naam = item.Naam, PasVragenVerdiend = item.PasVragenVerdiend });
            }

            op1 = data.Opdrachten.op1;
            op2 = data.Opdrachten.op2;
            op3 = data.Opdrachten.op3;

            if (!VragenGevonden)
            {
                Message = "Vragen File niet gevonden!";
            }
            else
            {
                Message = "";
            }
        }

        private IConductor conductor;
        private readonly SimpleContainer container;

        public BindableCollection<DagViewModel> Dagen { get; } = new BindableCollection<DagViewModel>();
        public BindableCollection<PasVraagViewModel> Pasvragen { get; } = new BindableCollection<PasVraagViewModel>();

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

        private bool UnLocked => LockString.Equals(Settings.Default.pwd, StringComparison.InvariantCultureIgnoreCase);

        public bool CanSaveAdmin => SelectedDag != null && UnLocked;

        public void Timer()
        {
            var x = container.GetInstance<TimerViewModel>();
            x.Minuten = 45;
            conductor.ActivateItem(x);
        }

        public void SaveAdmin()
        {
            var newAdminData = new AdminData();
            newAdminData.Opdrachten.op1 = op1;
            newAdminData.Opdrachten.op2 = op2;
            newAdminData.Opdrachten.op3 = op3;


            foreach (var item in Pasvragen)
            {
                newAdminData.Pasvragen.Add(new PasvragenVerdiend { Naam = item.Naam, PasVragenVerdiend = item.PasVragenVerdiend });
            }

            Util.SafeFileWithBackup(newAdminData, SelectedDag.Id);
        }

        private string message;

        public string Message
        {
            get
            {
                return message;
            }
            set
            {
                Set(ref message, value);
            }
        }

        public bool CanStartQuiz => SelectedDag != null && VragenGevonden;

        private bool VragenGevonden => Util.DataFileFoundAndValid<VragenData>(container.GetInstance<ShellViewModel>().Dag);


        public void StartQuiz()
        {
            var x = container.GetInstance<QuizNaamViewModel>();
            conductor.ActivateItem(x);
        }
        public bool CanValidate => SelectedDag != null;

        public void Validate()
        {
            var x = container.GetInstance<ValidateViewModel>();
            conductor.ActivateItem(x);
        }

    }
}
