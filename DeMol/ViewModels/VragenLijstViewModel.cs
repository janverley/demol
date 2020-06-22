using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Caliburn.Micro;
using DeMol.Model;

namespace DeMol.ViewModels
{
    public class VragenLijstViewModel : Conductor<object>.Collection.OneActive
    {
        private readonly SimpleContainer container;

        private readonly List<OpdrachtData> gespeeldeOpdrachten;

        //private readonly List<OpdrachtData> gespeeldeOpdrachten;
        private readonly IEnumerable<OpdrachtData> opdrachtenData;
        private readonly int selectedDagId;

        public VragenLijstViewModel(SimpleContainer container)
        {
            this.container = container;


            var adminData = Util.GetAdminData(container);

            selectedDagId  = container.GetInstance<ShellViewModel>().Dag;
            opdrachtenData = Util.AlleOpdrachtData();

            gespeeldeOpdrachten = opdrachtenData.Where(od => od.GespeeldOpDag == selectedDagId).ToList();
        }


        public bool CanPrevious => true;
        public bool CanNext => true;
        public string Naam { get; set; }

        protected override void OnActivate()
        {
            Items.Clear();
            var opdrachtenCount = gespeeldeOpdrachten.Count();

            for (var i = 0; i < opdrachtenCount; i++)
            {
                var opdrachtData = gespeeldeOpdrachten[i];

                var jijmol = container.GetInstance<QuizBenJijDeMolViewModel>();
                jijmol.Naam         = Naam;
                jijmol.OpdrachtData = opdrachtData;
                jijmol.DoNext = model =>
                {
                    if (model.IsDeMol)
                    {
                        var x =
                            Items.SkipWhile(item => !Equals(item, ActiveItem)).Skip(1)
                                    .FirstOrDefault(y => y is QuizVragenViewModel) as
                                QuizVragenViewModel;
                        x.IsDeMol = true;
                        ActivateItem(x);
                    }
                    else
                    {
                        var x =
                            Items.SkipWhile(item => !Equals(item, ActiveItem)).Skip(1)
                                    .FirstOrDefault(y => y is QuizWieIsDeMolViewModel) as
                                QuizWieIsDeMolViewModel;
                        ActivateItem(x);
                    }
                };
                Items.Add(jijmol);

                var wieisdemol = container.GetInstance<QuizWieIsDeMolViewModel>();
                wieisdemol.Naam         = Naam;
                wieisdemol.OpdrachtData = opdrachtData;
                wieisdemol.DoNext = model =>
                {
                    var x =
                        Items.SkipWhile(item => !Equals(item, ActiveItem)).Skip(1)
                                .FirstOrDefault(y => y is QuizVragenViewModel) as
                            QuizVragenViewModel;
                    x.IsDeMol = false;
                    x.DeMolIs = model.DeMolIs;
                    ActivateItem(x);
                };

                Items.Add(wieisdemol);

                var vragen = container.GetInstance<QuizVragenViewModel>();

                var vragenCodes = VragenCodesFromGespeeldeOpdracht(opdrachtData);

                vragen.VragenCodes = vragenCodes;
                vragen.OpdrachtId  = opdrachtData.Opdracht;
                vragen.Naam        = Naam;
                vragen.DoNext = model2 =>
                {
                    var isLast = !Items.SkipWhile(item => !Equals(item, ActiveItem)).Skip(1).Any();

                    if (isLast)
                    {
                        var adminData = Util.GetAdminData(container);
                        adminData.HeeftQuizGespeeld.Add(new SpelerInfo {Naam = Naam});
                        Util.SafeAdminData(container, adminData);


                        var q = Parent as QuizViewModel;
                        q.StartSmoel();
                    }
                    else
                    {
                        ActivateItem(Items.SkipWhile(item => !Equals(item, ActiveItem)).Skip(1).FirstOrDefault());
                    }
                };
                Items.Add(vragen);
            }

            ActivateItem(Items.First());
            base.OnActivate();
        }

        public void Previous()
        {
        }

        public void Next()
        {
        }

        private void StartLoop(string naam, OpdrachtData gespeeldeOpdracht)
        {
            var opdrachtId = gespeeldeOpdracht.Opdracht;

            var op = opdrachtenData.FirstOrDefault(o => o.Opdracht.SafeEqual(opdrachtId));

            var x2 = container.GetInstance<QuizBenJijDeMolViewModel>();
            x2.Naam         = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(naam.ToLower());
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

            return vragenCodes;
        }

        // private void BeginVragenVoorMol(string naam,
        //                                 OpdrachtData gespeeldeOpdracht)
        // {
        //     var vragenCodes = VragenCodesFromGespeeldeOpdracht(gespeeldeOpdracht);
        //
        //     var x3 = container.GetInstance<QuizVragenViewModel>();
        //             
        //     x3.VragenCodes = vragenCodes;
        //     x3.OpdrachtId = gespeeldeOpdracht.Opdracht;
        //     x3.Naam = naam;
        //     x3.IsDeMol = true;
        //     x3.DeMolIs = naam;
        //             
        //     ActiveItem = x3;
        //     x3.Activate();
        // }
        //
        // private void BeginVragenVoorNietMol(object sender, DeactivationEventArgs e, string naam,
        //                                     OpdrachtData gespeeldeOpdracht)
        // {
        //     var vragenCodes = VragenCodesFromGespeeldeOpdracht(gespeeldeOpdracht);
        //
        //
        //     var ss = (QuizWieIsDeMolViewModel) sender;
        //     
        //
        //     var x3 = container.GetInstance<QuizVragenViewModel>();
        //             
        //     x3.VragenCodes = vragenCodes;
        //     x3.OpdrachtId = gespeeldeOpdracht.Opdracht;
        //     x3.Naam = naam;
        //     x3.IsDeMol = false;
        //     x3.DeMolIs = ss.Opties.Single(o => o.IsSelected).OptieText;
        //             
        //     ActivateItem(x3);
        //
        // }

        // public void OnQuizBenJijDeMolViewModelClose(string naam, OpdrachtData opdrachData, bool isDeMol)
        // {
        //     if (!isDeMol)
        //     {
        //         var x4 = container.GetInstance<QuizWieIsDeMolViewModel>();
        //         x4.OpdrachtData = opdrachData;
        //         x4.Naam = naam;
        //         ActivateItem(x4);
        //
        //         //x4.Deactivated += (sender, e) => BeginVragenVoorNietMol(sender, e, naam, opdrachData);
        //     }
        //         
        //     //BeginVragenVoorMol(naam, opdrachData);           
        // }
    }
}