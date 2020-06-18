using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Caliburn.Micro;
using DeMol.Model;
using DeMol.Properties;

namespace DeMol.ViewModels
{
    public class QuizViewModel : Screen
    {
        private readonly SimpleContainer container;

        public QuizViewModel(SimpleContainer container)
        {
            this.container = container;
        }

        public QuizLoopViewModel QuizLoop { get; private set; }
        public DagViewModel SelectedDag { get; set; }

        protected override void OnActivate()
        {
            QuizLoop = new QuizLoopViewModel(container, SelectedDag);
            base.OnActivate();
        }
    }

    public class QuizLoopViewModel : Conductor<object>
    {
        private readonly SimpleContainer container;
        //private readonly List<OpdrachtData> gespeeldeOpdrachten;
        private IEnumerable<OpdrachtData> opdrachtenData;
        private int selectedDagId;

        public QuizLoopViewModel(SimpleContainer container, DagViewModel selectedDag)
        {
            this.container = container;
            var adminData = Util.SafeReadJson<AdminData>(selectedDag.Id);

            selectedDagId = selectedDag.Id;
            //gespeeldeOpdrachten = adminData.OpdrachtenGespeeld;

            opdrachtenData = Util.AlleOpdrachtData();
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            var x = container.GetInstance<SmoelenViewModel>();
            x.CanSelectUserDelegate = name => { return true; };

            x.DoNext = vm =>
            {
                foreach (var gespeeldeOpdracht in opdrachtenData.Where(od => od.GespeeldOpDag == selectedDagId))
                {
                    StartLoop(vm.Naam, gespeeldeOpdracht);
                }
            };

            ActivateItem(x);
        }

        
        
        private void StartLoop(string naam, OpdrachtData gespeeldeOpdracht)
        {
            var opdrachtId = gespeeldeOpdracht.Opdracht;

            var op = opdrachtenData.FirstOrDefault(o => o.Opdracht.SafeEqual(opdrachtId));
                
            var x2 = container.GetInstance<QuizBenJijDeMolViewModel>();
            x2.Naam = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(naam.ToLower());
            x2.OpdrachtData = op;
            
            ActivateItem(x2);
        }

        private static List<string> VragenCodesFromGespeeldeOpdracht(OpdrachtData gespeeldeOpdracht)
        {
            var vragenCodes = new List<string>();

            var opdrachtVragen = Util.SafeReadJson<OpdrachtData>(gespeeldeOpdracht.Opdracht);

            for (var i = 0; i < opdrachtVragen.Vragen.Count; i++)
            {
                var x = Util.GetVraagAndCode(opdrachtVragen, i);
                vragenCodes.Add(x.Item1);
            }

            var extraVragen = Util.SafeReadJson<OpdrachtData>("x");
            for (var i = vragenCodes.Count; i < Settings.Default.aantalVragenPerDag; i++)
            {
                var r = new Random().Next(extraVragen.Vragen.Count);
                var x = Util.GetVraagAndCode(extraVragen, r);

                if (!vragenCodes.Contains(x.Item1))
                {
                    vragenCodes.Add(x.Item1);
                }
                else
                {
                    i--;
                }
            }

            return vragenCodes;
        }

        private void BeginVragenVoorMol(string naam,
                                        OpdrachtData gespeeldeOpdracht)
        {
            var vragenCodes = VragenCodesFromGespeeldeOpdracht(gespeeldeOpdracht);

            var x3 = container.GetInstance<QuizVragenViewModel>();
                    
            x3.VragenCodes = vragenCodes;
            x3.OpdrachtId = gespeeldeOpdracht.Opdracht;
            x3.Naam = naam;
            x3.IsDeMol = true;
            x3.DeMolIs = naam;
                    
            ActivateItem(x3);

        }

        private void BeginVragenVoorNietMol(object sender, DeactivationEventArgs e, string naam,
                                            OpdrachtData gespeeldeOpdracht)
        {
            var vragenCodes = VragenCodesFromGespeeldeOpdracht(gespeeldeOpdracht);


            var ss = (QuizWieIsDeMolViewModel) sender;
            

            var x3 = container.GetInstance<QuizVragenViewModel>();
                    
            x3.VragenCodes = vragenCodes;
            x3.OpdrachtId = gespeeldeOpdracht.Opdracht;
            x3.Naam = naam;
            x3.IsDeMol = false;
            x3.DeMolIs = ss.Opties.Single(o => o.IsSelected).OptieText;
                    
            ActivateItem(x3);

        }

        public void OnQuizBenJijDeMolViewModelClose(string naam, OpdrachtData opdrachData, bool isDeMol)
        {
            if (!isDeMol)
            {
                var x4 = container.GetInstance<QuizWieIsDeMolViewModel>();
                x4.Opdracht = Util.OpdrachtUINaam(opdrachData);
                x4.Naam = naam;
                ActivateItem(x4);

                x4.Deactivated += (sender, e) => BeginVragenVoorNietMol(sender, e, naam, opdrachData);
            }
                
            BeginVragenVoorMol(naam, opdrachData);           
        }
    }
}