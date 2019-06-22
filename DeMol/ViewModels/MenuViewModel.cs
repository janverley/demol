using DeMol.Model;
using Caliburn.Micro;
using System;
using System.Linq;
using DeMol.Properties;
using System.Globalization;
using System.Collections.Generic;

namespace DeMol.ViewModels
{

    public class MenuViewModel : Screen
    {

        public MenuViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            Dagen.Clear();
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
                    if (value != null)
                    {
                        container.GetInstance<ShellViewModel>().Dag = SelectedDag.Id;
                    }
                    UpdateButtonStates();
                }
            }
        }

        private void UpdateButtonStates()
        {
            NotifyOfPropertyChange(() => CanSaveAdmin);
            NotifyOfPropertyChange(() => CanStartQuiz);
            NotifyOfPropertyChange(() => CanValidate);
            NotifyOfPropertyChange(() => CanStartMolAanduiden);
        }

        public void SelectedDagChanged()
        {
            if (SelectedDag != null)
            {
                container.GetInstance<ShellViewModel>().Dag = SelectedDag.Id;
                var adminData = Util.SafeReadJson<AdminData>(container.GetInstance<ShellViewModel>().Dag);
                //var opdrachtenVragenData = Util.SafeReadJson<OpdrachtVragenData>();

                // als file niet gevonden
                foreach (var speler in container.GetInstance<ShellViewModel>().Spelerdata.Spelers)
                {
                    if (!adminData.Pasvragen.Any(pv => pv.Naam.SafeEqual(speler.Naam)))
                    {
                        adminData.Pasvragen.Add(new PasvragenVerdiend { Naam = speler.Naam, PasVragenVerdiend = 0 });
                    }
                }

                var bestaandeOpdrachtVragen = GetAllOpdrachtVragen();

                Pasvragen.Clear();
                foreach (var item in adminData.Pasvragen)
                {
                    Pasvragen.Add(new PasVraagViewModel { Naam = item.Naam, PasVragenVerdiend = item.PasVragenVerdiend });
                }

                OpdrachtenGespeeld.Clear();
                foreach (var item in bestaandeOpdrachtVragen)
                {
                    OpdrachtenGespeeld.Add(new OpdrachtViewModel{ Id = item.Opdracht, Naam = $"{item.Opdracht.ToUpper()} - {item.Description}", VandaagGespeeld = false });
                }

                if (!VragenGevonden)
                {
                    Message = "Vragen File niet gevonden!";
                }
                else
                {
                    Message = "";
                }
            }
        }

        private IEnumerable<OpdrachtVragenData> GetAllOpdrachtVragen()
        {
            var result = new List<OpdrachtVragenData>();

            var allChars = "abcdefghijklmnopqrstuvwxyz";

            foreach (var @char in allChars.ToCharArray())
            {
                var opdrachtId = @char.ToString();
                if (Util.DataFileFoundAndValid<OpdrachtVragenData>(opdrachtId))
                {
                    var opdrachtVragenData = Util.SafeReadJson<OpdrachtVragenData>(opdrachtId);
                    result.Add(opdrachtVragenData);
                }
            }

            return result;
        }

        private readonly IConductor conductor;
        private readonly SimpleContainer container;

        public BindableCollection<DagViewModel> Dagen { get; } = new BindableCollection<DagViewModel>();
        public BindableCollection<PasVraagViewModel> Pasvragen { get; } = new BindableCollection<PasVraagViewModel>();
        public BindableCollection<OpdrachtViewModel> OpdrachtenGespeeld { get; } = new BindableCollection<OpdrachtViewModel>();

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

            foreach (var item in Pasvragen)
            {
                newAdminData.Pasvragen.Add(new PasvragenVerdiend { Naam = item.Naam, PasVragenVerdiend = item.PasVragenVerdiend });
            }
            foreach (var item in OpdrachtenGespeeld.Where(o => o.VandaagGespeeld))
            {
                newAdminData.OpdrachtenGespeeld.Add(item.Id);
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
        public bool CanStartMolAanduiden => SelectedDag != null && VragenGevonden;

        private bool VragenGevonden => Util.DataFileFoundAndValid<VragenData>(container.GetInstance<ShellViewModel>().Dag);

        public void StartMolAanduiden()
        {
            var quizNaamViewModel = container.GetInstance<QuizNaamViewModel>();
            quizNaamViewModel.DoNext = (vm) =>
            {
                var jijBentDeMolViewModel = container.GetInstance<JijBentDeMolViewModel>();
                jijBentDeMolViewModel.Dag = SelectedDag;
                jijBentDeMolViewModel.IsMorgen = false;
                jijBentDeMolViewModel.Naam = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(vm.Naam.ToLower());
                jijBentDeMolViewModel.DoNext = (_) =>
                {
                    conductor.ActivateItem(quizNaamViewModel);
                };

                conductor.ActivateItem(jijBentDeMolViewModel);
            };

            conductor.ActivateItem(quizNaamViewModel);
        }

        public void StartQuiz()
        {
            var x = container.GetInstance<QuizNaamViewModel>();
            x.DoNext = (vm) =>
            {
                var x2 = container.GetInstance<QuizBenJijDeMolViewModel>();
                x2.Naam = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(vm.Naam.ToLower());
                conductor.ActivateItem(x2);
            };

            conductor.ActivateItem(x);
        }
        public bool CanValidate => SelectedDag != null;

        public void Validate()
        {
            var x = container.GetInstance<ValidateViewModel>();
            conductor.ActivateItem(x);
        }
        public void EndResult()
        {
            var x = container.GetInstance<EndResultViewModel>();
            conductor.ActivateItem(x);
        }

        public void Smoelen()
        {
            var x = container.GetInstance<SmoelenViewModel>();
            conductor.ActivateItem(x);
        }
    }
}
