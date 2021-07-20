using System;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using DeMol.Model;
using DeMol.Properties;

namespace DeMol.ViewModels
{
    public class MenuViewModel : Screen
    {
        private readonly IConductor conductor;
        private readonly SimpleContainer container;

        private string lockString = "";

        private string message;


        private DagViewModel selectedDag;

        public MenuViewModel(ShellViewModel conductor, SimpleContainer container)
        {
            this.conductor = conductor;
            this.container = container;
        }


        public DagViewModel SelectedDag
        {
            get => selectedDag;
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

        public BindableCollection<DagViewModel> Dagen { get; } = new BindableCollection<DagViewModel>();
        public BindableCollection<PasVraagViewModel> Pasvragen { get; } = new BindableCollection<PasVraagViewModel>();

        public BindableCollection<OpdrachtViewModel> OpdrachtenGespeeld { get; } =
            new BindableCollection<OpdrachtViewModel>();

        public string LockString
        {
            get => lockString;
            set
            {
                if (Set(ref lockString, value))
                {
                    UpdateButtonStates();
                }
            }
        }

        private bool UnLocked => LockString.Equals(Settings.Default.pwd, StringComparison.InvariantCultureIgnoreCase);

        public bool CanSaveAdmin => SelectedDag != null && UnLocked;

        public string Message
        {
            get => message;
            set => Set(ref message, value);
        }

        public bool CanStartQuiz => SelectedDag != null && AdminIsSaved;
        public bool CanStartMolAanduiden => SelectedDag != null; // && VragenGevonden;

        private bool AdminIsSaved => Util.DataFileFoundAndValid<AdminData>(container.GetInstance<ShellViewModel>().Dag);
        public bool CanValidate => SelectedDag != null;

        public bool CanEndResult => UnLocked;
        public bool CanEndQuiz => UnLocked;

        protected override void OnActivate()
        {
            LockString = "";
            
            base.OnActivate();

            Dagen.Clear();
            foreach (var dag in container.GetInstance<ShellViewModel>().DagenData.Dagen)
            {
                Dagen.Add(new DagViewModel(dag.Id, dag.Naam));
            }

            SelectedDag = Dagen.First();
            SelectedDagChanged();
            // // if set, preselect SelectedDag
            // if (container.GetInstance<ShellViewModel>().Dag > 0 &&
            //     Dagen.Any(d => d.Id == container.GetInstance<ShellViewModel>().Dag))
            // {
            //     SelectedDag = Dagen.First(d => d.Id == container.GetInstance<ShellViewModel>().Dag);
            // }
        }


        private void UpdateButtonStates()
        {
            NotifyOfPropertyChange(() => CanSaveAdmin);
            NotifyOfPropertyChange(() => CanStartQuiz);
            NotifyOfPropertyChange(() => CanValidate);
            NotifyOfPropertyChange(() => CanStartMolAanduiden);
            NotifyOfPropertyChange(() => CanEndQuiz);
            NotifyOfPropertyChange(() => CanEndResult);
        }

        public void SelectedDagChanged()
        {
            if (SelectedDag != null)
            {
                container.GetInstance<ShellViewModel>().Dag = SelectedDag.Id;
                var adminData = Util.GetAdminDataOfSelectedDag(container);

                // als file niet gevonden
                foreach (var speler in container.GetInstance<ShellViewModel>().Spelerdata.Spelers)
                {
                    if (!adminData.Pasvragen.Any(pv => pv.Naam.SafeEqual(speler.Naam)))
                    {
                        adminData.Pasvragen.Add(new PasvragenVerdiend {Naam = speler.Naam, PasVragenVerdiend = 0});
                    }
                }

                var opdrachtDatas = Util.AlleOpdrachtData();

                Pasvragen.Clear();
                foreach (var item in adminData.Pasvragen)
                {
                    Pasvragen.Add(new PasVraagViewModel {Naam = item.Naam, PasVragenVerdiend = item.PasVragenVerdiend});
                }

                OpdrachtenGespeeld.Clear();
                foreach (var opdrachtData in opdrachtDatas)
                {
                    var antwoorden = Util.SafeReadJson<AntwoordenData>(opdrachtData.Opdracht);

                    OpdrachtenGespeeld.Add(
                        new OpdrachtViewModel(opdrachtData,
                            adminData.OpdrachtenGespeeld.Any(o => o.OpdrachtId.SafeEqual(opdrachtData.Opdracht)),
                            antwoorden.MaxTeVerdienen,
                            antwoorden.EffectiefVerdiend
                        ));
                }

                UpdateButtonStates();
            }
        }

        public void Timer()
        {
            var x = container.GetInstance<TimerViewModel>();
            x.Minuten = 45;
            conductor.ActivateItem(x);
        }

        public void SaveAdmin()
        {
            var newAdminData = Util.SafeReadJson<AdminData>(SelectedDag.Id);

            newAdminData.Pasvragen.Clear();
            foreach (var item in Pasvragen)
            {
                newAdminData.Pasvragen.Add(new PasvragenVerdiend
                    {Naam = item.Naam, PasVragenVerdiend = item.PasVragenVerdiend});
            }

            newAdminData.OpdrachtenGespeeld.Clear();
            foreach (var item in OpdrachtenGespeeld.Where(op => op.VandaagGespeeld))
            {
                //Util.SafeFileWithBackup(item.OpdrachtData, item.OpdrachtData.Opdracht);

                var gespeeldeOpdracht = new GespeeldeOpdrachtData
                {
                    OpdrachtId = item.Id,
                    EffectiefVerdiend = item.EffectiefVerdiend,
                    MaxTeVerdienen = item.MaxTeVerdienen
                };

                newAdminData.OpdrachtenGespeeld.Add(gespeeldeOpdracht);
            }

            foreach (var item in OpdrachtenGespeeld)
            {
                // stockeer values in antwoordenData
                var antwoorden = Util.SafeReadJson<AntwoordenData>(item.Id);

                if (item.VandaagGespeeld)
                {
                    antwoorden.Dag = SelectedDag.Id.ToString();
                }

                antwoorden.EffectiefVerdiend = item.EffectiefVerdiend;
                antwoorden.MaxTeVerdienen = item.MaxTeVerdienen;
                Util.SafeFileWithBackup(antwoorden, item.Id);
            }

            Util.SafeFileWithBackup(newAdminData, SelectedDag.Id);
            UpdateButtonStates();

            File.Delete($@".\Files\antwoorden.{SelectedDag.Id}.json");
        }


        public void StartQuiz()
        {
            var x = container.GetInstance<QuizViewModel>();
            conductor.ActivateItem(x);
        }

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

        public void EndQuiz()
        {
            var x = container.GetInstance<FinaleQuizViewModel>();
            conductor.ActivateItem(x);
        }
    }
}