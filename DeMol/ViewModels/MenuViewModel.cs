using System;
using System.Collections.Generic;
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

                    //SelectedDagChanged();

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

        public bool CanEndQuiz
        {
            get
            {
                var result = false;

                foreach (var dag in container.GetInstance<ShellViewModel>().DagenData.Dagen)
                {
                    //var antwoorden = Util.SafeReadJson<AntwoordenData>(dag.Id);
                    //if (antwoorden.Spelers.Count(s => s.IsDeMol) != 1)
                    //{
                    //    result = false;
                    //}
                    //else
                    {
                        result = true;
                    }
                }

                return result;
            }
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
            if (container.GetInstance<ShellViewModel>().Dag > 0 &&
                Dagen.Any(d => d.Id == container.GetInstance<ShellViewModel>().Dag))
            {
                SelectedDag = Dagen.First(d => d.Id == container.GetInstance<ShellViewModel>().Dag);
            }
        }

        protected override void OnDeactivate(bool close)
        {
            //if (SelectedDag != null)
            //{
            //    SaveAdmin();
            //}
            base.OnDeactivate(close);
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
                var adminData = Util.GetAdminDataOfSelectedDag(container);
                //var opdrachtenVragenData = Util.SafeReadJson<OpdrachtVragenData>();

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
                    var algesavedOpdrachtdata =
                        adminData.OpdrachtenGespeeld.FirstOrDefault(o => o.OpdrachtId.SafeEqual(opdrachtData.Opdracht));

                    //var opdrachtData = opdrachtDatas.First(od => od.Opdracht == adminData.OpdrachtenGespeeld gespeeldeOpdrachtData.OpdrachtId);

                    OpdrachtenGespeeld.Add(
                        new OpdrachtViewModel(opdrachtData, algesavedOpdrachtdata));
                    // {
                    //     Id = opdrachtData.Opdracht,
                    //     OpdrachtData = opdrachtData,
                    //     Naam = Util.OpdrachtUINaam(opdrachtData),
                    //     VandaagGespeeld = (algesavedOpdrachtdata != null),
                    //     MaxTeVerdienen = algesavedOpdrachtdata?.MaxTeVerdienenBedrag??0,
                    //     EffectiefVerdiend = algesavedOpdrachtdata?.VerdiendBedrag??0
                    //     });
                }

                //if (!VragenGevonden)
                //{
                //    Message = "Vragen File niet gevonden!";
                //}
                //else
                //{
                //    Message = "";
                //}
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

                var gespeeldeOpdracht = new GespeeldeOpdrachtData()
                {
                    OpdrachtId = item.Id,
                    EffectiefVerdiend = item.EffectiefVerdiend,
                    MaxTeVerdienen = item.MaxTeVerdienen
                };
                    
                newAdminData.OpdrachtenGespeeld.Add(gespeeldeOpdracht);
            }

            // var vragenCodes = new List<string>();
            // foreach (var gespeeldeOpdracht in newAdminData.OpdrachtenGespeeld)
            // {
            //     var opdrachtVragen = Util.SafeReadJson<OpdrachtData>(gespeeldeOpdracht.Opdracht);
            //
            //     for (var i = 0; i < opdrachtVragen.Vragen.Count; i++)
            //     {
            //         var x = Util.GetVraagAndCode(opdrachtVragen, i);
            //         vragenCodes.Add(x.Item1);
            //     }
            // }
            //
            // var extraVragen = Util.SafeReadJson<OpdrachtData>("x");
            // for (var i = vragenCodes.Count; i < Settings.Default.aantalVragenPerDag; i++)
            // {
            //     var r = new Random().Next(extraVragen.Vragen.Count);
            //     var x = Util.GetVraagAndCode(extraVragen, r);
            //
            //     if (!vragenCodes.Contains(x.Item1))
            //     {
            //         vragenCodes.Add(x.Item1);
            //     }
            //     else
            //     {
            //         i--;
            //     }
            // }
            //
            // vragenCodes = vragenCodes.Shuffle(new Random()).ToList();
            // newAdminData.VragenCodes.Clear();
            // foreach (var vragenCode in vragenCodes)
            // {
            //     newAdminData.VragenCodes.Add(vragenCode);
            // }


            Util.SafeFileWithBackup(newAdminData, SelectedDag.Id);
            UpdateButtonStates();

            File.Delete($@".\Files\antwoorden.{SelectedDag.Id}.json");
        }

        // public void StartMolAanduiden()
        // {
        //     var x = container.GetInstance<SmoelenViewModel>();
        //
        //     x.CanSelectUserDelegate = name =>
        //     {
        //         var adminData = Util.GetAdminData(container);
        //         var result = !adminData.IsVerteldOfZeDeMolZijn.Any(s => s.Naam == name);
        //         return result;
        //     };
        //
        //     x.DoNext = vm =>
        //     {
        //         var adminData = Util.GetAdminData(container);
        //         adminData.IsVerteldOfZeDeMolZijn.Add(new SpelerInfo {Naam = vm.Naam});
        //         Util.SafeFileWithBackup(adminData, container.GetInstance<ShellViewModel>().Dag);
        //
        //
        //         var jijBentDeMolViewModel = container.GetInstance<JijBentDeMolViewModel>();
        //         jijBentDeMolViewModel.Dag = SelectedDag;
        //         jijBentDeMolViewModel.IsMorgen = false;
        //         jijBentDeMolViewModel.Naam = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(vm.Naam.ToLower());
        //         jijBentDeMolViewModel.DoNext = _ => { conductor.ActivateItem(x); };
        //
        //         conductor.ActivateItem(jijBentDeMolViewModel);
        //     };
        //
        //     conductor.ActivateItem(x);
        // }

        public void StartQuiz()
        {
            var x = container.GetInstance<QuizViewModel>();

            // x.CanSelectUserDelegate = name =>
            // {
            //     var antwoorden = Util.SafeReadJson<AntwoordenData>(container.GetInstance<ShellViewModel>().Dag);
            //     var result = !antwoorden.Spelers.Any(s => s.Naam.SafeEqual(name));
            //     return result;
            // };
            //
            // x.DoNext = vm =>
            // {
            //     var x2 = container.GetInstance<QuizViewModel>();
            //     x2.Naam = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(vm.Naam.ToLower());
            //     conductor.ActivateItem(x2);
            // };

            //x.SelectedDag = SelectedDag;

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
            
// alle admindatas ophalen
// alle gespeekde opdracthen in lijst samensteken
            
            // var gespeeldeOpdrachtenIds = new List<string>();
            //
            // foreach (var dag in container.GetInstance<ShellViewModel>().DagenData.Dagen)
            // {
            //     var adminadata = Util.SafeReadJson<AdminData>(dag.Id);
            //
            //     foreach (var gespeeldeOpdrachtData in adminadata.OpdrachtenGespeeld)
            //     {
            //         gespeeldeOpdrachtenIds.Add(gespeeldeOpdrachtData.OpdrachtId);
            //     }
            // }
            //
            // var opdrachtDatas = Util.AlleOpdrachtData().Where(od =>  gespeeldeOpdrachtenIds.Any(i => i== od.Opdracht));
            //
            // var finaleAdminData = Util.SafeReadJson<FinaleData>();
            // if (!finaleAdminData.FinaleVragen.Any())
            // {
            //     var finaleVragen = new List<FinaleVraag>();
            //
            //     var alleGespeeldeOpdrachten = new List<string>();
            //     foreach (var dag in container.GetInstance<ShellViewModel>().DagenData.Dagen)
            //     {
            //         var antwoorden       = Util.SafeReadJson<AntwoordenData>(dag.Id);
            //         var deMol            = antwoorden.Spelers.Single(s => s.IsDeMol);
            //         var juisteAntwoorden = deMol.Antwoorden;
            //
            //         var adminData = Util.SafeReadJson<AdminData>(dag.Id);
            //         foreach (var gespeeldeOpdracht in opdrachtDatas)
            //         {
            //             if (!alleGespeeldeOpdrachten.Contains(gespeeldeOpdracht.Opdracht))
            //             {
            //                 alleGespeeldeOpdrachten.Add(gespeeldeOpdracht.Opdracht);
            //
            //                 var opdrachtVragen = Util.SafeReadJson<OpdrachtData>(gespeeldeOpdracht.Opdracht);
            //
            //                 for (var i = 0; i < opdrachtVragen.Vragen.Count; i++)
            //                 {
            //                     var tup = Util.GetVraagAndCode(opdrachtVragen, i);
            //
            //                     var juistAntwoord = juisteAntwoorden[tup.Item1];
            //
            //                     var finaleVraag = new FinaleVraag
            //                     {
            //                         Dag           = dag,
            //                         Description   = opdrachtVragen.Description,
            //                         Opdracht      = opdrachtVragen.Opdracht,
            //                         Vraag         = tup.Item2,
            //                         VraagCode     = tup.Item1,
            //                         JuistAntwoord = juistAntwoord
            //                     };
            //
            //                     finaleVragen.Add(finaleVraag);
            //                 }
            //             }
            //         }
            //     }
            //
            //
            //     for (var i = 0; i < Math.Min(finaleVragen.Count, Settings.Default.aantalVragenWeekWinnaar); i++)
            //     {
            //         var r           = new Random().Next(finaleVragen.Count);
            //         var finaleVraag = finaleVragen[r];
            //         if (!finaleAdminData.FinaleVragen.Any(fv => fv.VraagCode == finaleVraag.VraagCode))
            //         {
            //             finaleAdminData.FinaleVragen.Add(finaleVraag);
            //         }
            //         else
            //         {
            //             i--;
            //         }
            //     }
            //
            //     Util.SafeFileWithBackup(finaleAdminData);
            // }
            //
            // var x = container.GetInstance<SmoelenViewModel>();
            //
            // x.CanSelectUserDelegate = name =>
            // {
            //     var antwoorden = Util.SafeReadJson<AntwoordenData>("finale");
            //     var result     = !antwoorden.Spelers.Any(s => s.Naam.SafeEqual(name));
            //     return result;
            // };

            // x.DoNext = vm =>
            // {
            //     var finaleAdminData2 = Util.SafeReadJson<FinaleData>();
            //
            //     var x2 = container.GetInstance<QuizVragenViewModel>();
            //
            //     x2.QuizVraagViewModelFactory = vraagCode =>
            //     {
            //         var fv = finaleAdminData2.FinaleVragen.Single(fv2 => fv2.VraagCode == vraagCode);
            //
            //         var result = new QuizVraagViewModel(fv.Vraag, fv.VraagCode);
            //         result.Text = $"{fv.Dag.Naam}: {fv.Description}\n{fv.Vraag.Text} ({fv.VraagCode})";
            //
            //         return result;
            //     };
            //
            //     x2.IsDeMol = false;
            //     x2.OpdrachtId = "finale";
            //     x2.VragenCodes = finaleAdminData2.FinaleVragen.Select(fv => fv.VraagCode).ToList();
            //     x2.Naam = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(vm.Naam.ToLower());
            //
            //     x2.DoNext = quizVragenViewModel => { conductor.ActivateItem(x); };
            //
            //     conductor.ActivateItem(x2);
            // };

//            conductor.ActivateItem(x);
        }

        // public void Smoelen()
        // {
        //     var x = container.GetInstance<SmoelenViewModel>();
        //     conductor.ActivateItem(x);
        // }
    }
}